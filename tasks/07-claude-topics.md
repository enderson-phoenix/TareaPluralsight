# Fase 7 — Temas de Claude Code (40% de la evaluación)

Estos temas se intercalan con el desarrollo. No los dejes todos para el final — algunos bloquean etapas anteriores (T-01, T-02) o son el resultado natural de etapas anteriores (T-03, T-04).

Cada tema tiene: qué hacer, qué prompt usar con Claude Code, y qué evidencia guardar en `EVIDENCE.md`.

---

## T-01 — Plan Mode + Ask Mode

**Cuándo hacerlo:** Antes de implementar la Fase 3 (capa de aplicación).

**Qué hacer:**
1. En Claude Code, activa Plan Mode antes de implementar los handlers.
2. Describe el problema: "Necesito implementar los 3 casos de uso con CQRS y MediatR en Clean Architecture".
3. Deja que Claude genere el plan. Revisa, ajusta si es necesario.
4. Aprueba el plan para que Claude empiece a implementar.

**Prompt sugerido para activar Plan Mode:**
```
Voy a implementar la capa de Application con CQRS usando MediatR.
Necesito: CreateOrderCommand, AssignTechnicianCommand y GetOrdersByStatusQuery,
cada uno con su handler. ¿Puedes generar un plan de implementación antes de empezar?
```

**Evidencia a guardar en `EVIDENCE.md`:**
- Captura de pantalla o texto del plan que generó Claude.
- Nota de qué cambios hiciste al plan antes de aprobarlo.

---

## T-02 — /init y CLAUDE.md

**Cuándo hacerlo:** Al inicio del proyecto, justo después del Setup (Fase 1).

**Qué hacer:**
1. En Claude Code, ejecuta el comando `/init` en la raíz del proyecto.
2. Claude generará un `CLAUDE.md` base. Personalízalo con:

**Secciones mínimas en CLAUDE.md:**
```markdown
## Stack
- .NET 8 / C# 12
- Clean Architecture (Domain → Application → Infrastructure → Api)
- EF Core con SQLite
- MediatR para CQRS
- xUnit + Moq + FluentAssertions

## Convenciones
- Nombres en inglés (código), español solo en comentarios de dominio
- Commands y Queries en Records inmutables
- Private setters en entidades del dominio
- Un handler por archivo, mismo directorio que su command/query

## Rutas importantes
- `src/CalSystem.Domain/` — Entidades e interfaces (sin dependencias externas)
- `src/CalSystem.Application/` — Casos de uso (Commands, Queries, Handlers)
- `src/CalSystem.Infrastructure/` — EF Core, SQLite, repositorios
- `src/CalSystem.Api/` — Controllers y configuración HTTP
- `tests/CalSystem.Tests/` — Pruebas unitarias

## Restricciones
- No agregar lógica de negocio fuera del Domain
- No referenciar Infrastructure desde Api directamente (solo via DI)
- No usar datos en memoria en producción — siempre SQLite
```

**Evidencia a guardar en `EVIDENCE.md`:**
- Captura del comando `/init` ejecutado.
- Contenido final del `CLAUDE.md` (o referencia al archivo).

---

## T-03 — Test-Driven Development

**Cuándo hacerlo:** Al implementar la Fase 6 (pruebas), específicamente UC-01.

**Qué hacer:**
El ciclo TDD está detallado en [`06-tests.md`](06-tests.md). Lo importante es documentar cada paso.

**Evidencia requerida en `EVIDENCE.md`:**

```markdown
## T-03 — Ciclo TDD: CreateOrderHandler

### 1. Test escrito ANTES del handler
[Pega aquí el código del test]

### 2. Resultado del test en ROJO (falla esperada)
[Pega la salida de `dotnet test` mostrando el error de compilación o falla]

### 3. Implementación mínima del handler
[Pega aquí el código del handler]

### 4. Resultado del test en VERDE
[Pega la salida de `dotnet test` mostrando "Passed: 2"]
```

---

## T-04 — Documentation Guidelines

**Cuándo hacerlo:** Después de implementar el Domain y la Api (Fases 2 y 5).

### Parte A: Generar README.md con Claude

**Prompt sugerido:**
```
Genera el README.md principal del proyecto. El proyecto se llama "CalSystem" 
(Sistema de Órdenes de Servicio Técnico para Phoenix Calibration DR).
Stack: .NET 8, Clean Architecture, EF Core SQLite, MediatR, xUnit.
Incluye: descripción, requisitos previos, cómo instalar y ejecutar, 
los 3 endpoints disponibles con ejemplos de request/response, y cómo correr los tests.
```

### Parte B: Generar XML comments en `ServiceOrder`

**Prompt sugerido:**
```
Agrega XML documentation comments a la clase ServiceOrder en 
src/CalSystem.Domain/Entities/ServiceOrder.cs. 
Documenta la clase, todas las propiedades y los métodos Create, AssignTechnician y Close.
```

El resultado se ve en el intellisense de Visual Studio / Rider.

**Evidencia a guardar en `EVIDENCE.md`:**
- Indicar que `README.md` fue generado con Claude (fecha y prompt usado).
- Captura del antes/después de la clase `ServiceOrder` con los XML comments.

---

## T-05 — Security Review

**Cuándo hacerlo:** Después de implementar el endpoint `POST /api/orders` (Fase 5).

**Prompt sugerido:**
```
Revisa el endpoint POST /api/orders en ServiceOrdersController.cs desde una perspectiva de seguridad.
Considera: validación de input, inyección, exposición de datos sensibles, 
autenticación/autorización, manejo de errores. 
¿Cuáles son los riesgos actuales y cómo los mitigarías?
```

**Evidencia a guardar en `EVIDENCE.md`:**
- El prompt exacto usado.
- La respuesta completa de Claude con los hallazgos.
- Si implementaste alguna corrección a partir de la revisión, menciónala.

> Es válido que Claude diga "no se encontraron vulnerabilidades críticas" — lo importante es que la revisión se hizo y quedó documentada.

---

## T-06 — GitHub MCP Integration

**Cuándo hacerlo:** En cualquier momento después de tener el repositorio en GitHub.

**Opciones válidas (elige una):**

### Opción A: Crear un Issue desde Claude Code
```
Usa GitHub MCP para crear un issue en el repositorio con el título:
"feat: agregar validación de campos vacíos en CreateOrderCommand"
y descripción: "Actualmente el endpoint acepta campos vacíos. 
Se debe agregar validación de FluentValidation para CustomerName, Equipment y ProblemDescription."
```

### Opción B: Crear un Pull Request
Si tienes cambios en una rama, pídele a Claude que cree el PR via MCP.

### Opción C: Revisar un PR existente
```
Usa GitHub MCP para revisar el PR número X del repositorio y 
dame un resumen de los cambios.
```

**Evidencia a guardar en `EVIDENCE.md`:**
- Captura de la acción realizada (issue creado, PR abierto, etc.).
- URL del recurso creado en GitHub.

---

## T-07 — Custom Skill

**Cuándo hacerlo:** En cualquier momento. Puede crearlo Claude o tú.

**Skill sugerido:** Generador de entidad de dominio DDD.

**Crear el archivo:** `.claude/commands/new-entity.md`

```markdown
---
description: Genera una entidad de dominio DDD completa con propiedades, método Create estático y XML comments
argument-hint: NombreEntidad prop1:tipo prop2:tipo ...
---

Genera una entidad de dominio DDD en C# para el proyecto CalSystem con las siguientes características:

Entidad: $ARGUMENTS

Reglas:
- Namespace: CalSystem.Domain.Entities
- Constructor privado (requerido por EF Core)
- Propiedades con `private set`
- Método estático `Create(...)` que retorna la entidad
- Usar `Guid.NewGuid()` para el Id
- Incluir XML documentation comments en clase, propiedades y método Create
- El archivo va en: src/CalSystem.Domain/Entities/{NombreEntidad}.cs

Genera solo el código C# listo para usar, sin explicaciones adicionales.
```

**Uso del skill:**
```
/new-entity Equipment serialNumber:string brand:string calibrationDate:DateTime
```

**Evidencia a guardar en `EVIDENCE.md`:**
- Contenido del archivo del skill.
- Ejemplo de uso con el output generado.

---

## T-08 — Custom Hook

**Cuándo hacerlo:** Al inicio del proyecto, junto con la configuración del entorno.

**Hook sugerido:** Log de cada vez que Claude ejecuta `dotnet test`.

Edita (o crea) `.claude/settings.json`:

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "command",
            "command": "powershell -Command \"if ($env:CLAUDE_TOOL_INPUT -match 'dotnet test') { $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'; Add-Content -Path 'test-runs.log' -Value \"[$timestamp] dotnet test ejecutado\" }\""
          }
        ]
      }
    ]
  }
}
```

> Este hook registra en `test-runs.log` cada vez que se ejecutan los tests, con timestamp.

**Alternativa más simple** — hook que valida antes de hacer commit:
```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "command",
            "command": "powershell -Command \"if ($env:CLAUDE_TOOL_INPUT -match 'git commit') { Write-Host '🔍 Hook: Verificando que los tests pasan antes del commit...' }\""
          }
        ]
      }
    ]
  }
}
```

**Evidencia a guardar en `EVIDENCE.md`:**
- Contenido del hook en `settings.json`.
- Descripción de qué hace y cuándo se activa.
- Captura del hook ejecutándose (opcional pero recomendado).

---

## Checklist de Temas Claude Code

- [ ] T-01: Plan capturado en `EVIDENCE.md` antes de implementar Application
- [ ] T-02: `CLAUDE.md` en la raíz con stack, convenciones, rutas y restricciones
- [ ] T-03: Ciclo TDD documentado (rojo → verde) en `EVIDENCE.md`
- [ ] T-04: `README.md` generado + XML comments en `ServiceOrder`
- [ ] T-05: Revisión de seguridad de `POST /api/orders` documentada
- [ ] T-06: Acción real en GitHub via MCP con evidencia (URL del recurso)
- [ ] T-07: Archivo `.claude/commands/new-entity.md` creado y probado
- [ ] T-08: Hook configurado en `.claude/settings.json` y documentado

**¡Proyecto completado!** Verifica el [`00-overview.md`](00-overview.md) para el checklist final.
