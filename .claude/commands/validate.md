---
description: Valida que un cambio de código no rompe compilación, tests, migraciones ni arquitectura en CalSystem. Sin argumentos, auto-detecta cambios desde git.
argument-hint: "qué cambió (opcional) — ej: 'entidad ServiceOrder', 'handler CreateOrder'. Si se omite, se detecta desde git."
---

Actúa como el agente validador de cambios del proyecto CalSystem.

---

## PASO 0 — Determinar el contexto del cambio

**Si "$ARGUMENTS" NO está vacío:** el developer describió el cambio. Úsalo directamente como contexto y salta al PASO 1.

**Si "$ARGUMENTS" está vacío:** debes detectar qué cambió usando git. Ejecuta estos comandos en orden:

```bash
# 1. Ver archivos con cambios sin commitear (staged + unstaged)
git status --short 2>&1

# 2. Ver archivos modificados no commiteados
git diff --name-only 2>&1
git diff --cached --name-only 2>&1

# 3. Si no hay cambios sin commitear, ver el último commit
git show --name-only --format="Commit: %s" HEAD 2>&1 | head -20
```

Con esa información, construye automáticamente el contexto:
- Archivos en `Domain/Entities/` → tipo: **entidad de dominio**
- Archivos en `Application/Orders/Commands/` → tipo: **command/handler**
- Archivos en `Application/Orders/Queries/` → tipo: **query/handler**
- Archivos en `Infrastructure/Persistence/Repositories/` → tipo: **repositorio**
- Archivos en `Infrastructure/Migrations/` → tipo: **migración EF Core**
- Archivos en `Api/Controllers/` → tipo: **endpoint/controller**
- Archivos en `Infrastructure/Persistence/AppDbContext.cs` → tipo: **DbContext**
- `Program.cs` o `DependencyInjection.cs` → tipo: **configuración DI**

Anuncia lo que detectaste antes de continuar:
> "Detecté cambios en: [lista de archivos]. Tipo de cambio: [tipo]. Procediendo a validar..."

---

## PASO 1 — Identifica el tipo de cambio y el alcance de checks

Según el tipo detectado o descrito, determina qué checks son relevantes:

| Tipo de cambio       | Compilación | Tests | Migraciones | Clean Arch | DI | Anti-patrones | Contrato API |
|----------------------|:-----------:|:-----:|:-----------:|:----------:|:--:|:-------------:|:------------:|
| Entidad de dominio   | ✓ | ✓ | ✓ | ✓ | — | — | — |
| Handler/Command      | ✓ | ✓ | — | ✓ | ✓ | — | ✓ |
| Query/Handler        | ✓ | ✓ | — | ✓ | ✓ | ✓ | ✓ |
| Repositorio          | ✓ | ✓ | — | ✓ | ✓ | ✓ | — |
| Migración EF Core    | ✓ | — | ✓ | — | — | — | — |
| Endpoint/Controller  | ✓ | ✓ | — | — | — | — | ✓ |
| DbContext            | ✓ | ✓ | ✓ | ✓ | ✓ | — | — |
| Configuración DI     | ✓ | ✓ | — | ✓ | ✓ | — | — |

---

## PASO 2 — Compilación (siempre, BLOQUEANTE)

```bash
dotnet build --no-incremental -v minimal 2>&1
```

- **PASS**: "Build succeeded" sin errores.
- **FAIL**: Reporta el error exacto (archivo + línea + mensaje CS). **Detente aquí — salta directo al PASO 7 (plan de corrección).**
- **WARN**: "warning CS" — anota y continúa.

---

## PASO 3 — Tests (siempre, BLOQUEANTE)

```bash
dotnet test --no-build --logger "console;verbosity=minimal" 2>&1
```

- **PASS**: 0 failed, 0 errored.
- **FAIL**: Reporta nombre del test, excepción y línea. **Detente aquí — salta al PASO 7.**
- **SKIP**: Si el proyecto de tests no existe aún, marca SKIP y continúa.

---

## PASO 4 — Migraciones EF Core (si aplica)

Solo si el cambio involucra entidades o DbContext:

```bash
dotnet ef migrations list --project src/CalSystem.Infrastructure --startup-project src/CalSystem.Api 2>&1
```

Luego verifica si los archivos de entidad modificados tienen una migración correspondiente buscando en `src/CalSystem.Infrastructure/Migrations/` la migración más reciente y leyendo su contenido.

- **PASS**: La migración cubre los cambios del modelo.
- **WARN**: Entidad modificada sin migración nueva. Indica exactamente qué falta.

---

## PASO 5 — Límites de Clean Architecture (si aplica)

```bash
# Domain no debe referenciar nada
cat src/CalSystem.Domain/CalSystem.Domain.csproj 2>/dev/null | grep ProjectReference

# Application no debe referenciar Infrastructure
cat src/CalSystem.Application/CalSystem.Application.csproj 2>/dev/null | grep Infrastructure
```

- **PASS**: Sin referencias inválidas.
- **FAIL**: Referencia que viola la jerarquía Domain ← Application ← Infrastructure ← Api.

---

## PASO 6 — Checks específicos adicionales

Ejecuta solo los que corresponden al tipo del cambio (tabla del PASO 1):

**Registro en DI** — si cambió una interfaz, repositorio o handler nuevo:
```bash
cat src/CalSystem.Infrastructure/DependencyInjection.cs 2>/dev/null
cat src/CalSystem.Application/DependencyInjection.cs 2>/dev/null
```
Verifica que el nuevo tipo está registrado.

**Anti-patrones de escalabilidad** — si cambió repositorio o query:
```bash
grep -rn "\.Result\b\|\.Wait()\|GetAwaiter" src/ 2>/dev/null
grep -n "foreach" src/CalSystem.Infrastructure/**/*.cs 2>/dev/null
```
Busca: bloqueos síncronos, N+1 queries, falta de `AsNoTracking()` en métodos de solo lectura.

**Contrato de API** — si cambió un command/query o endpoint:
```bash
cat src/CalSystem.Api/Controllers/ServiceOrdersController.cs 2>/dev/null
```
Compara propiedades del Command/Query con el DTO del controller. Reporta cualquier desajuste.

---

## PASO 7 — Reporte final y plan de corrección

### Tabla de resultados

Produce siempre esta tabla (omite filas SKIP):

```
╔══════════════════════════════════════════════════════════════╗
║          REPORTE DE VALIDACIÓN — CalSystem                   ║
║  Cambio: [describe qué cambió y en qué archivos]             ║
╚══════════════════════════════════════════════════════════════╝

| Check                         | Resultado | Detalle                        |
|-------------------------------|-----------|--------------------------------|
| 1. Compilación                | ✅ / ❌   |                                |
| 2. Tests                      | ✅ / ❌   | X passed, Y failed             |
| 3. Migraciones EF Core        | ✅ / ⚠️   |                                |
| 4. Límites Clean Architecture | ✅ / ❌   |                                |
| 5. Registro en DI             | ✅ / ⚠️   |                                |
| 6. Anti-patrones              | ✅ / ⚠️   |                                |
| 7. Contrato de API            | ✅ / ⚠️   |                                |
```

### Veredicto

- Si hay ❌: **BLOQUEADO**
- Si solo hay ⚠️: **ADVERTENCIAS**
- Si todo ✅ / ⏭️: **APROBADO**

### Plan de corrección (solo si hay ❌ o ⚠️)

Cuando el veredicto sea BLOQUEADO o ADVERTENCIAS, genera este plan de corrección con checkboxes ordenados por dependencia (lo que debe hacerse primero va primero):

```
## Plan de corrección — [descripción del problema]

Resuelve los siguientes puntos en orden. Marca cada uno antes de pasar al siguiente.

- [ ] 1. [Acción específica con el comando exacto o el cambio de código preciso]
         Archivo: [ruta exacta] | Línea: [número si aplica]
         Comando: `[comando exacto a ejecutar]`

- [ ] 2. [Siguiente acción]
         ...

- [ ] N. Ejecutar `/validate` nuevamente para confirmar que todos los checks pasan.

> Nota: Los puntos están ordenados por dependencia.
> No avances al punto 2 sin haber completado y verificado el punto 1.
```

Cada ítem del plan debe ser **accionable y específico**: no escribas "corregir el error", escribe "en `src/CalSystem.Domain/Entities/ServiceOrder.cs` línea 42, cambia `public string CalibrationDate` por `public DateTime? CalibrationDate { get; private set; }`".

---

---

## PASO 8 — Guardar FIX-PLAN.md en disco (solo si hay ❌ o ⚠️)

Si el veredicto fue **BLOQUEADO** o **ADVERTENCIAS**, usa la herramienta Write para crear o sobreescribir el archivo `FIX-PLAN.md` en la raíz del proyecto con este formato exacto:

```markdown
# FIX-PLAN — [descripción corta del problema]

> Generado por `/validate` el [fecha y hora]
> Veredicto: [BLOQUEADO / ADVERTENCIAS]
> Cambio analizado: [qué cambió]

## Checks fallidos

| Check | Resultado | Detalle |
|-------|-----------|---------|
| [nombre] | ❌ / ⚠️ | [descripción del problema] |

## Pasos para corregir

<!-- Marca cada [ ] con [x] a medida que completes el paso -->

- [ ] 1. [Acción específica]
         📁 Archivo: `[ruta exacta]`
         💻 Comando: `[comando exacto]`

- [ ] 2. [Siguiente acción]
         📁 Archivo: `[ruta exacta]`
         💻 Comando: `[comando exacto]`

- [ ] N. Ejecutar `/validate` nuevamente para confirmar que todos los checks pasan.

---

## Notas del desarrollador

<!-- Espacio para que el developer anote qué encontró, qué decidió, por qué -->

```

Reglas para escribir FIX-PLAN.md:
- Si ya existe un `FIX-PLAN.md` del ciclo anterior, **sobreescríbelo** — representa el estado actual.
- Los pasos deben ir en orden de dependencia: no se puede completar el paso 2 sin el 1.
- Cada paso tiene exactamente un archivo o comando. Si hay dos acciones, son dos pasos.
- Termina siempre con un paso que diga "Ejecutar `/validate` para confirmar resolución".

Después de escribir el archivo, notifica al developer:
> "📄 FIX-PLAN.md guardado en la raíz del proyecto. Ábrelo, trabaja en los puntos marcados con `[ ]` y márcalos `[x]` a medida que los resuelvas. Cuando termines, ejecuta `/validate` para confirmar."

Si el veredicto fue **APROBADO**, y existe un `FIX-PLAN.md` de un ciclo anterior, elimínalo o agrega al final del archivo:
```markdown
## ✅ Resuelto
Todos los checks pasaron. Este plan fue completado satisfactoriamente.
```

---

**Ejemplos de uso:**
```
/validate                                          ← auto-detecta desde git
/validate entidad ServiceOrder — campo nuevo       ← contexto manual
/validate handler AssignTechnicianHandler          ← contexto manual
/validate migración AddCalibrationDate             ← contexto manual
```
