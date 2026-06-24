---
description: Validación inteligente con clasificación por severidad. Crítico bloquea automáticamente, Importante pregunta qué hacer, Sugerencia informa sin bloquear.
argument-hint: "qué cambió (opcional) — si se omite, auto-detecta desde git"
---

Actúa como el validador inteligente de CalSystem. A diferencia de `/validate`, este comando clasifica los problemas por severidad y responde diferente según el nivel.

---

## PASO 0 — Detectar contexto (si no hay argumentos)

Si "$ARGUMENTS" está vacío, detecta qué cambió:

```bash
git status --short 2>&1
git diff --name-only 2>&1
git diff --cached --name-only 2>&1
git show --name-only --format="Commit: %s" HEAD 2>&1 | head -15
```

Anuncia qué detectaste antes de continuar.

---

## PASO 1 — Ejecutar todos los checks

Corre los mismos checks que `/validate`:

```bash
# Compilación
dotnet build --no-incremental -v minimal 2>&1

# Tests
dotnet test --no-build --logger "console;verbosity=minimal" 2>&1

# Migraciones (si aplica)
dotnet ef migrations list --project src/CalSystem.Infrastructure --startup-project src/CalSystem.Api 2>&1

# Límites Clean Architecture
cat src/CalSystem.Domain/CalSystem.Domain.csproj 2>/dev/null | grep ProjectReference
cat src/CalSystem.Application/CalSystem.Application.csproj 2>/dev/null | grep Infrastructure

# Anti-patrones
grep -rn "\.Result\b\|\.Wait()\|GetAwaiter" src/ 2>/dev/null
grep -rn "foreach" src/CalSystem.Infrastructure/ 2>/dev/null
```

Lee también los archivos modificados que detectaste o que el developer indicó.

---

## PASO 2 — Clasificar cada problema por severidad

Clasifica **cada hallazgo individual** en una de tres categorías:

### 🔴 CRÍTICO — bloquea el desarrollo
Problemas que hacen que la aplicación no compile o no funcione:
- Error de compilación (CS xxxx)
- Test fallido
- Referencia circular entre proyectos de Clean Architecture
- Handler registrado dos veces en DI

### 🟡 IMPORTANTE — requiere decisión del developer
Problemas que no bloquean ahora pero crearán deuda o bugs en el futuro próximo:
- Entidad modificada sin migración correspondiente
- Nueva interfaz sin registro en DI
- Contrato de API desalineado con el Command/Query
- Llamada síncrona a base de datos (`.Result`, `.Wait()`)

### 🔵 SUGERENCIA — informativo, no bloquea
Mejoras recomendadas para escalabilidad y mantenibilidad:
- Falta `AsNoTracking()` en query de solo lectura
- Propiedad pública con setter público en una entidad de dominio
- Handler en el directorio equivocado
- Nombre de DTO que no sigue la convención del proyecto

---

## PASO 3 — Responder según severidad

### Si hay problemas 🔴 CRÍTICO:

Muestra inmediatamente:
```
🔴 BLOQUEADO — se encontraron N problema(s) crítico(s)

[descripción del error exacto, archivo y línea]

Generando plan de corrección automático...
```

Genera el `FIX-PLAN.md` automáticamente (sin preguntar) con los pasos para resolver los críticos primero. Escríbelo en la raíz del proyecto con la herramienta Write.

No presentes los problemas de menor severidad hasta que los críticos estén resueltos — evita abrumar al developer.

---

### Si NO hay críticos pero SÍ hay problemas 🟡 IMPORTANTE:

Para **cada** problema importante, presenta las opciones disponibles y espera respuesta:

```
🟡 DECISIÓN REQUERIDA — [descripción del problema]

Contexto: [explica por qué esto importa y qué pasa si no se resuelve]

¿Qué prefieres hacer?

  A) [Acción inmediata — ej: crear la migración ahora]
     Impacto: resuelve el problema de raíz
     Comando: `[comando exacto]`

  B) [Acción diferida — ej: marcar como TODO y continuar]
     Impacto: crea deuda técnica, puede causar error en producción más adelante
     Acción: agrego un comentario // TODO en el archivo

  C) [Ver más información antes de decidir]
     Impacto: te muestro el archivo afectado y el contexto completo

Responde con A, B o C (o escribe tu propia decisión):
```

Espera la respuesta del developer. Ejecuta lo que decidió. Luego pasa al siguiente problema importante si hay más de uno.

---

### Si solo hay 🔵 SUGERENCIAS (y nada crítico ni importante):

Muestra un resumen en bloque único, sin interrupciones:

```
✅ APROBADO — cambio seguro

🔵 Sugerencias opcionales (no bloquean):

  • [sugerencia 1] — [por qué mejoraría el código]
    📁 src/CalSystem/.../Archivo.cs línea X

  • [sugerencia 2] — ...

Puedes ignorarlas o resolverlas cuando tengas tiempo.
```

No generes FIX-PLAN.md para sugerencias. Si el developer quiere resolverlas, puede pedir `/validate-smart` después de cada una.

---

### Si no hay ningún problema:

```
✅ APROBADO — todos los checks pasaron sin observaciones

[tabla de checks con todos en PASS]

Si existe un FIX-PLAN.md previo, lo marco como resuelto.
```

Usa Write para agregar `✅ Resuelto — [fecha]` al final del FIX-PLAN.md si existe.

---

## PASO 4 — Reporte final de severidad

Al terminar (después de resolver los interactivos), muestra la tabla de resumen:

```
╔═══════════════════════════════════════════════════════╗
║      REPORTE validate-smart — CalSystem               ║
╚═══════════════════════════════════════════════════════╝

🔴 Críticos:    X  (deben resolverse antes de continuar)
🟡 Importantes: X  (decisiones tomadas: A/B/C por cada uno)
🔵 Sugerencias: X  (opcionales)

Veredicto final: [BLOQUEADO / APROBADO CON ADVERTENCIAS / APROBADO]
```

---

**Cuándo usar este comando vs los otros:**

| Situación | Comando |
|-----------|---------|
| Quiero validación rápida sin interrupciones | `/validate` |
| Quiero análisis en subagente aislado | `/validate-agent` |
| Quiero decidir qué hacer con cada problema | `/validate-smart` |
| Antes de un commit importante | `/validate-smart` |
