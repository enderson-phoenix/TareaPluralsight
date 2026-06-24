# Hook: build-validator

> Documentación del Custom Hook implementado en `.claude/settings.json`.
> Los hooks de Claude Code se configuran en `settings.json`, no como archivos ejecutables separados.

---

## Propósito

Garantizar que ningún cambio de código rompa la compilación ni los tests sin que el desarrollador se dé cuenta. El hook actúa como un quality gate automático que corre en segundo plano mientras Claude trabaja.

---

## Hooks implementados

### 1. PostToolUse — Build automático tras editar `.cs`

**Evento:** `PostToolUse` con matcher `Edit|Write`  
**Tipo:** `command` (PowerShell)  
**Bloqueante:** No — solo informa

**Qué hace:** Cada vez que Claude escribe o edita un archivo `.cs`, ejecuta `dotnet build --no-incremental` automáticamente. Muestra el resultado en la consola de Claude Code:
- `[PASS] Build OK` — compilación exitosa
- `[FAIL] N error(s)` — con los primeros errores para diagnóstico rápido

**Por qué:** Detectar errores de compilación inmediatamente después de cada edición, antes de que se acumulen varios archivos rotos.

```json
{
  "PostToolUse": [
    {
      "matcher": "Edit|Write",
      "hooks": [
        {
          "type": "command",
          "shell": "powershell",
          "statusMessage": "change-validator: checking build...",
          "command": "try { $p = ($env:CLAUDE_TOOL_INPUT | ConvertFrom-Json -EA Stop).file_path; if ($p -match '\\.cs$') { $r = dotnet build --no-incremental -v q 2>&1; $e = ($r | Select-String ' error ').Count; if ($e -gt 0) { Write-Host ('[FAIL] ' + $e + ' error(s)') } else { Write-Host '[PASS] Build OK' } } } catch {}"
        }
      ]
    }
  ]
}
```

---

### 2. PreToolUse Op4 — Build + tests antes de `git commit`

**Evento:** `PreToolUse` con matcher `Bash`, filtro `if: Bash(git commit*)`  
**Tipo:** `command` (PowerShell)  
**Bloqueante:** Sí — `exit 1` cancela el commit

**Qué hace:** Intercepta cualquier `git commit` y ejecuta `dotnet build && dotnet test` antes de permitirlo. Si algo falla, el commit es cancelado con un mensaje claro.

**Por qué:** Evitar commits con código roto en el repositorio. Es el primer nivel de la cadena de gates pre-commit.

---

### 3. PreToolUse Op5 — Análisis de arquitectura antes de `git commit`

**Evento:** `PreToolUse` con matcher `Bash`, filtro `if: Bash(git commit*)`  
**Tipo:** `agent` (claude-sonnet-4-6)  
**Bloqueante:** Sí — si el agente detecta problemas CRÍTICOS

**Qué hace:** Si Op4 pasa, un agente revisa los archivos staged buscando violaciones de Clean Architecture, migraciones faltantes, registros DI omitidos y anti-patrones. Solo bloquea si encuentra problemas críticos.

**Por qué:** El build y los tests no detectan violaciones arquitecturales. Este gate agrega una capa de análisis semántico que los compiladores no pueden hacer.

---

### 4. Stop Op3 — Validación en background al terminar turno

**Evento:** `Stop`  
**Tipo:** `command` con `asyncRewake: true`  
**Bloqueante:** No — pero despierta a Claude si hay problema

**Qué hace:** Cuando Claude termina de responder, en segundo plano revisa si hubo cambios en archivos `.cs` vía `git diff`. Si los hay, ejecuta build + test. Si algo falla, usa `exit 2` para despertar a Claude con el mensaje del error para que lo corrija automáticamente.

**Por qué:** Capturar regresiones que ocurren entre turnos, cuando el desarrollador aún no ha pedido una validación explícita.

---

## Ubicación del código completo

El código completo de los 4 hooks está en:

```
.claude/settings.json → "hooks": { ... }
```

Ver también `.claude/GUIDE.md` sección "Hooks en settings.json" para documentación completa con ejemplos, tipos de hook y exit codes.
