# Guía de Claude Code Tooling — CalSystem

> Referencia completa de Commands, Agents y Hooks implementados en este proyecto.
> Para cualquier persona que quiera entender cómo funciona cada pieza y cómo usarla.

---

## Índice

1. [Conceptos clave](#conceptos-clave)
2. [Estructura del proyecto](#estructura)
3. [Commands (Slash Commands)](#commands)
   - [/validate](#validate)
   - [/validate-agent](#validate-agent)
   - [/validate-smart](#validate-smart)
4. [Agents (Subagentes)](#agents)
   - [change-validator](#change-validator)
5. [Hooks en settings.json](#hooks)
   - [Tipos de hook](#tipos-de-hook)
   - [Eventos disponibles](#eventos)
   - [Nuestros hooks](#nuestros-hooks)
6. [Flujos de trabajo](#flujos)
7. [Cómo crear los tuyos](#crear)
8. [Referencia rápida](#referencia)

---

## 1. Conceptos clave

### ¿Qué es un Command (Slash Command)?

Un archivo `.md` en `.claude/commands/` que Claude ejecuta cuando escribes `/nombre` en el chat.

- **Cómo funciona:** Claude lee el archivo como un prompt de instrucciones y lo ejecuta en el contexto actual de la conversación.
- **Cuándo usarlo:** Acciones que invocas manualmente con contexto específico.
- **Variable especial:** `$ARGUMENTS` se reemplaza con todo lo que escribes después del slash.

```
/validate entidad ServiceOrder
         └────────────────────── esto es $ARGUMENTS
```

### ¿Qué es un Agent (Subagente personalizado)?

Un archivo `.md` en `.claude/agents/` que define un "rol" especializado con su propio system prompt y herramientas limitadas.

- **Cómo funciona:** Corre en un subcontexto aislado; su output de herramientas no contamina la conversación principal.
- **Cuándo usarlo:** Cuando quieres análisis profundo sin llenar el contexto principal, o cuando necesitas un rol con un conjunto fijo de herramientas.
- **Diferencia con command:** El command corre *en* la conversación. El agent corre *aparte* de ella.

### ¿Qué es un Hook?

Un comando que se ejecuta **automáticamente** cuando ocurre un evento en Claude Code, sin que tú lo pidas.

- **Cómo funciona:** Configurado en `settings.json` con `"hooks": { "EVENTO": [...] }`.
- **Cuándo usarlo:** Automatizar validaciones, bloquear acciones peligrosas, logging.
- **Diferencia clave:** No lo invocas tú — el sistema lo dispara según el evento.

```
Tú editas ServiceOrder.cs
         └── [AUTOMÁTICO] Hook PostToolUse → dotnet build → [PASS]
```

---

## 2. Estructura del proyecto

```
.claude/
├── agents/
│   └── change-validator.md     ← Agente: quality gate completo (7 checks)
├── commands/
│   ├── validate.md             ← /validate  (inline + FIX-PLAN.md)
│   ├── validate-agent.md       ← /validate-agent (delega al agente)
│   └── validate-smart.md       ← /validate-smart (por severidad, interactivo)
└── settings.json               ← 4 hooks automáticos
```

---

## 3. Commands (Slash Commands)

### Anatomía de un archivo de command

```markdown
---
description: Una línea. Aparece en el listado de /
argument-hint: "texto de ayuda para los argumentos"
---

Aquí va el prompt que Claude leerá y ejecutará.

El usuario escribió: $ARGUMENTS

## PASO 1 — Nombre del paso
Instrucciones...

## PASO 2 — Siguiente paso
Instrucciones...
```

**Reglas de escritura:**
- Escribe el cuerpo como instrucciones para Claude, no como texto para el usuario.
- `$ARGUMENTS` puede estar vacío si el usuario no pasó argumentos — planifica para ese caso.
- Los bloques de código en el body son comandos que Claude ejecutará con la herramienta Bash.
- Incluye ejemplos de uso al final del archivo.

---

### /validate

**Propósito:** Validar un cambio de código. Si no hay argumentos, auto-detecta desde git.

**Opciones implementadas:** 1 (inline + FIX-PLAN.md) y 2 (escribe archivo al disco)

**Uso:**
```
/validate                                        ← detecta desde git automáticamente
/validate entidad ServiceOrder — campo nuevo     ← contexto manual
/validate handler CreateOrderHandler             ← contexto manual
/validate migración AddCalibrationDate           ← contexto manual
/validate endpoint POST /api/orders              ← contexto manual
```

**Qué hace:**

| Paso | Acción | Bloqueante |
|------|--------|:---:|
| 0 | Si no hay args → `git status/diff/show` para detectar cambios | — |
| 1 | Identifica tipo de cambio y checks relevantes | — |
| 2 | `dotnet build --no-incremental` | ✓ |
| 3 | `dotnet test` | ✓ |
| 4 | Migraciones EF Core sincronizadas (si aplica) | — |
| 5 | Límites Clean Architecture (referencias entre capas) | — |
| 6 | Registro en DI, anti-patrones, contrato de API | — |
| 7 | Reporte final con tabla PASS/FAIL/WARN | — |
| 8 | Si hay fallos → escribe `FIX-PLAN.md` con checkboxes | — |

**FIX-PLAN.md generado cuando hay fallos:**
```markdown
# FIX-PLAN — descripción del problema

> Generado por /validate | Veredicto: BLOQUEADO

## Pasos para corregir

- [ ] 1. Acción específica
         Archivo: `ruta/exacta`
         Comando: `dotnet ef migrations add NombreMigracion ...`

- [ ] N. Ejecutar /validate nuevamente para confirmar.
```

---

### /validate-agent

**Propósito:** Delegar la validación completa al agente `change-validator` en un subcontexto aislado.

**Opción implementada:** 3 (delega al agente)

**Cuándo usar en lugar de /validate:**
- Cambios grandes o críticos donde quieres el análisis más robusto
- Sesiones largas con mucho contexto acumulado (el agente tiene contexto limpio)
- Cuando quieres que el output de herramientas no aparezca en tu conversación principal

**Uso:**
```
/validate-agent                                  ← auto-detecta desde git
/validate-agent entidad ServiceOrder modificada  ← con contexto
```

**Diferencia con /validate:**

| Aspecto | /validate | /validate-agent |
|---------|-----------|-----------------|
| Contexto | Corre en la conversación principal | Corre en subagente aislado |
| Velocidad | Más rápido (sin overhead de subagente) | Más lento pero más limpio |
| Output de tools | Visible en la conversación | Oculto, solo ves el resultado final |
| Cuándo usar | Cambios pequeños/medianos | Cambios grandes o sesión larga |

---

### /validate-smart

**Propósito:** Validación interactiva con clasificación por severidad. Para cada problema te pregunta qué quieres hacer.

**Opción implementada:** 5 (severidad + interactivo)

**Uso:**
```
/validate-smart                                  ← auto-detecta desde git
/validate-smart handler CreateOrderHandler       ← con contexto
```

**Severidades:**

| Nivel | Símbolo | Qué hace | Ejemplos |
|-------|---------|----------|---------|
| CRÍTICO | 🔴 | Genera FIX-PLAN.md automáticamente y para | Error de compilación, test fallido, referencia circular |
| IMPORTANTE | 🟡 | Pregunta qué hacer (A/B/C) por cada problema | Migración faltante, DI sin registrar, bloqueo síncrono |
| SUGERENCIA | 🔵 | Lista al final sin interrumpir | Falta AsNoTracking, convención de nombres, escalabilidad |

**Flujo para problemas importantes (🟡):**
```
🟡 DECISIÓN REQUERIDA — Entidad modificada sin migración

Contexto: ServiceOrder tiene nuevo campo CalibrationDate
sin una migración EF Core que lo cubra.

¿Qué prefieres hacer?

  A) Crear la migración ahora
     Comando: dotnet ef migrations add AddCalibrationDate ...

  B) Marcar como deuda técnica y continuar
     Impacto: la app fallará al arrancar si la DB no tiene el campo

  C) Ver el archivo de entidad antes de decidir

Responde A, B o C:
```

---

## 4. Agents (Subagentes)

### Anatomía de un archivo de agent

```markdown
---
name: nombre-del-agente
description: >
  Descripción de 1-2 líneas. Claude Code usa este texto para decidir
  cuándo sugerir el agente. Sé específico sobre qué hace y cuándo usarlo.
model: claude-sonnet-4-6
tools: Bash, Read, Glob, Grep
---

[System prompt completo del agente]

Tu rol es [definir claramente].
Tu responsabilidad es [qué DEBE y qué NO DEBE hacer].

## Cómo ejecutar tu trabajo
[instrucciones paso a paso]

## Formato de respuesta
[estructura exacta del output esperado]
```

**Campos importantes:**
- `name`: identificador único. Se usa como `subagent_type: "name"` en el Agent tool.
- `model`: el agente puede usar un modelo diferente al de la sesión principal.
- `tools`: **limitar tools** hace al agente más enfocado, predecible y seguro. No le des tools que no necesita.

---

### change-validator

**Archivo:** `.claude/agents/change-validator.md`  
**Model:** claude-sonnet-4-6  
**Tools:** Bash, Read, Glob, Grep

**Propósito:** Quality gate completo para CalSystem. Verifica que cualquier cambio no rompa la aplicación ni la arquitectura.

**Los 7 checks que ejecuta en orden:**

```
CHECK 1 — Compilación         dotnet build --no-incremental    BLOQUEANTE
CHECK 2 — Tests               dotnet test                       BLOQUEANTE
CHECK 3 — Migraciones EF      dotnet ef migrations list         WARN si falta
CHECK 4 — Clean Architecture  lee los .csproj                   BLOQUEANTE si viola
CHECK 5 — Registro en DI      lee DependencyInjection.cs        WARN si falta
CHECK 6 — Anti-patrones       grep de .Result/.Wait()/N+1       WARN si encuentra
CHECK 7 — Contrato de API     lee controller vs command/query   WARN si desalinea
```

**Formato de reporte que produce:**
```
╔══════════════════════════════════════════════╗
║      REPORTE DE VALIDACIÓN — CalSystem       ║
╚══════════════════════════════════════════════╝

| Check                         | Resultado | Detalle              |
|-------------------------------|-----------|----------------------|
| 1. Compilación                | ✅ PASS   |                      |
| 2. Tests                      | ✅ PASS   | 5 passed, 0 failed   |
| 3. Migraciones EF Core        | ⚠️ WARN   | Campo sin migración  |
| 4. Límites Clean Architecture | ✅ PASS   |                      |

## Veredicto: ⚠️ ADVERTENCIAS

## Acciones recomendadas:
1. [CHECK 3] Crear migración: dotnet ef migrations add ...
```

**Cómo invocarlo:**
- **Manualmente desde Claude Code:** `/validate-agent`
- **Desde un workflow o hook:** `subagent_type: "change-validator"`
- **Desde un command:** se invoca en el PASO 2 de `validate-agent.md`

---

## 5. Hooks en settings.json

### Estructura base

```json
{
  "hooks": {
    "EVENTO": [
      {
        "matcher": "NombreDeLaHerramienta",
        "hooks": [
          {
            "type": "command|agent|prompt",
            "campo1": "valor",
            "campo2": "valor"
          }
        ]
      }
    ]
  }
}
```

El `matcher` filtra **qué herramienta** disparó el evento. Para eventos como `Stop` que no son de herramientas, el `matcher` puede omitirse.

---

### Tipos de hook

#### `type: "command"` — Comando de shell

El tipo más común. Ejecuta un comando de shell cuando ocurre el evento.

```json
{
  "type": "command",
  "shell": "powershell",
  "command": "Write-Host 'Hola'; exit 0",
  "statusMessage": "Texto visible al usuario mientras corre...",
  "if": "Bash(git commit*)",
  "timeout": 30,
  "asyncRewake": false
}
```

| Campo | Descripción |
|-------|-------------|
| `shell` | `"bash"` o `"powershell"` (default según OS) |
| `command` | El comando a ejecutar |
| `statusMessage` | Texto que ve el usuario mientras corre el hook |
| `if` | Filtro adicional con sintaxis de permission rules |
| `timeout` | Segundos máximos antes de cancelar |
| `asyncRewake` | Si `true`, corre en background. `exit 2` despierta a Claude |

**Exit codes:**
- `exit 0` → éxito, no interrumpe el flujo
- `exit 1` → error, bloquea la acción (solo en `PreToolUse`)
- `exit 2` → solo con `asyncRewake: true`: despierta a Claude con el output

#### `type: "agent"` — Agente verificador

Corre un agente que puede ejecutar herramientas para verificar algo antes de permitir una acción.

```json
{
  "type": "agent",
  "model": "claude-sonnet-4-6",
  "prompt": "Verifica que... Si hay problema crítico, bloquea.",
  "timeout": 90,
  "statusMessage": "Verificando con agente..."
}
```

- El agente tiene acceso a Bash y herramientas de lectura
- Puede ejecutar `dotnet build`, leer archivos, buscar patrones
- Ideal cuando la verificación requiere razonamiento o múltiples pasos

#### `type: "prompt"` — LLM evaluador (sin herramientas)

El tipo más ligero. Envía el input de la herramienta a un LLM que decide si aprobar o bloquear.

```json
{
  "type": "prompt",
  "prompt": "Evalúa si $ARGUMENTS es seguro para el proyecto CalSystem...",
  "continueOnBlock": false
}
```

- **Limitación:** No puede ejecutar comandos. Solo analiza el texto del input.
- Si el LLM responde `{"ok": false, "reason": "..."}` → bloquea la acción.
- Útil para filtrar comandos peligrosos basándose en texto.

---

### Eventos disponibles

| Evento | Cuándo se dispara | Puede bloquear |
|--------|------------------|:---:|
| `PreToolUse` | Antes de ejecutar cualquier herramienta | ✓ (exit 1) |
| `PostToolUse` | Después de ejecutar una herramienta | — |
| `Stop` | Cuando Claude termina de responder | — |
| `SessionStart` | Al iniciar una sesión de Claude Code | — |
| `SessionEnd` | Al cerrar la sesión | — |
| `PostToolBatch` | Después de un lote de herramientas | — |

El filtro `matcher` en `PreToolUse` y `PostToolUse` puede ser el nombre de la herramienta: `"Bash"`, `"Edit"`, `"Write"`, `"Edit|Write"`, etc.

El filtro `if` agrega un segundo nivel de filtrado con sintaxis de permission rules:
- `Bash(git commit*)` → solo si el comando bash empieza con "git commit"
- `Edit(*.cs)` → solo si el archivo editado termina en ".cs"

---

### Nuestros hooks

#### PostToolUse — Build automático tras editar .cs (Op1+Op2)

```
Trigger: Edit o Write en cualquier archivo
Filtro: solo archivos .cs (verificado en el comando)
Tipo: command
Acción: dotnet build --no-incremental -v q
Output: [PASS] / [WARN N warning(s)] / [FAIL N error(s) + primeros errores]
Bloqueante: NO (es informativo)
```

#### PreToolUse Op4 — Build+test rápido antes de git commit

```
Trigger: Bash con patrón "git commit*"
Tipo: command
Acción: dotnet build && dotnet test
Output: [OK] o [BLOQUEADO] con razón
Bloqueante: SÍ (exit 1 cancela el commit)
```

#### PreToolUse Op5 — Análisis de severidad antes de git commit (agent)

```
Trigger: Bash con patrón "git commit*" (si Op4 pasó)
Tipo: agent (claude-sonnet-4-6)
Acción: verifica migraciones, DI, arquitectura, anti-patrones en archivos staged
Output: BLOCKED con detalles o resumen de severidad
Bloqueante: SÍ (si agente detecta problemas CRÍTICOS)
```

#### Stop Op3 — Validación en background al terminar turno

```
Trigger: Stop (Claude termina de responder)
Tipo: command con asyncRewake: true
Acción: git diff --name-only HEAD → si hay .cs → build+test
Output si falla: exit 2 → Claude recibe system message y vuelve a responder
Output si pasa: exit 0 → silencioso, el usuario no ve nada
Bloqueante: NO (pero despierta a Claude si hay problema)
```

---

## 6. Flujos de trabajo

### Flujo de edición normal

```
1. Editas src/CalSystem.Domain/Entities/ServiceOrder.cs
   └── [AUTO Op1] PostToolUse → dotnet build
       ├── PASS → "[PASS] Build OK" en consola
       └── FAIL → "[FAIL] 2 error(s)" + primeras líneas de error

2. Claude termina su turno
   └── [AUTO Op3] Stop background → check git diff + build+test
       ├── PASS → silencioso
       └── FAIL → Claude despierta: "validate-agent [Op3] detected issues after this turn:"
                  Claude te muestra qué falló y cómo resolverlo.
```

### Flujo de commit

```
git commit -m "feat: add CalibrationDate to ServiceOrder"
    │
    ├── [AUTO Op4] PreToolUse command → build + test
    │   ├── FAIL → commit CANCELADO
    │   │         "[BLOQUEADO] Build fallido. Ejecuta /validate..."
    │   └── PASS → "[OK] Build y tests pasaron."
    │
    └── [AUTO Op5] PreToolUse agent → arquitectura + migraciones
        ├── CRÍTICO → commit CANCELADO con detalles del problema
        └── OK → commit procede ✓
```

### Flujo de análisis profundo

```
/validate-smart

PASO 1: detecta git diff staged
PASO 2: ejecuta todos los checks
PASO 3: clasifica por severidad

🔴 CRÍTICO → genera FIX-PLAN.md → para
🟡 IMPORTANTE → para y pregunta A/B/C por cada uno
🔵 SUGERENCIA → lista al final del reporte

FIN: reporte de severidades:
  🔴 Críticos:    0
  🟡 Importantes: 1 (decisión tomada: A)
  🔵 Sugerencias: 2 (opcionales)
  Veredicto: APROBADO CON ADVERTENCIAS
```

---

## 7. Cómo crear los tuyos

### Template de Command

Crea el archivo en `.claude/commands/mi-command.md` y úsalo con `/mi-command`.

```markdown
---
description: Una línea clara de qué hace este command
argument-hint: "descripción del argumento opcional"
---

[Instrucciones para Claude — escríbelas como órdenes, no como descripción]

Si "$ARGUMENTS" está vacío: [qué hacer cuando no hay args].
Si "$ARGUMENTS" tiene contenido: [qué hacer con los args].

## PASO 1 — Nombre descriptivo
Instrucción clara de qué ejecutar o analizar.
```bash
comando a ejecutar si aplica
```

## PASO 2 — Siguiente paso
Instrucción...

## PASO N — Reportar resultado
Formato esperado del output:
- Usa ✅ PASS / ❌ FAIL / ⚠️ WARN para estados
- Incluye el comando exacto para resolver cada problema

---

**Ejemplos de uso:**
```
/mi-command                         ← sin args
/mi-command contexto específico     ← con args
```
```

**Checklist antes de publicar un command:**
- [ ] El `description` es claro para alguien que no sabe qué hace
- [ ] Manejé el caso de `$ARGUMENTS` vacío
- [ ] Incluí ejemplos de uso al final
- [ ] El formato de output es consistente

---

### Template de Agent

Crea el archivo en `.claude/agents/mi-agente.md`.

```markdown
---
name: mi-agente
description: >
  Descripción de 1-2 líneas. Incluye: qué verifica/hace,
  en qué tipo de proyecto, y cuándo usarlo.
model: claude-sonnet-4-6
tools: Bash, Read, Grep
---

Eres el agente [nombre] del proyecto [nombre del proyecto].
Tu ÚNICO rol es [definir claramente].

## Lo que DEBES hacer
- [responsabilidad 1]
- [responsabilidad 2]

## Lo que NO DEBES hacer
- [límite 1 — por qué importa]
- [límite 2]

## Criterios para BLOQUEAR vs ADVERTIR vs APROBAR
- BLOQUEAR si: [condiciones críticas]
- ADVERTIR si: [condiciones no críticas]
- APROBAR si: [todo OK]

## Pasos de ejecución
1. [primer paso]
2. [segundo paso]

## Formato de respuesta requerido
[estructura exacta del output — tabla, lista, o formato libre]
```

**Checklist de un buen agent:**
- [ ] `tools` está limitado al mínimo necesario (no pongas lo que no usa)
- [ ] El system prompt define claramente CUÁNDO bloquear
- [ ] El formato de output está especificado en el system prompt
- [ ] El `name` es único y no tiene espacios

---

### Template de Hook

Agrega en `.claude/settings.json`:

#### Hook de observación (no bloqueante)
```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "command",
            "shell": "powershell",
            "statusMessage": "Descripción visible al usuario...",
            "command": "tu comando aquí; exit 0"
          }
        ]
      }
    ]
  }
}
```

#### Hook bloqueante pre-acción
```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash",
        "hooks": [
          {
            "type": "command",
            "shell": "powershell",
            "if": "Bash(git commit*)",
            "statusMessage": "Validando antes de continuar...",
            "command": "...; if ($LASTEXITCODE -ne 0) { Write-Host 'BLOQUEADO: razón'; exit 1 }"
          }
        ]
      }
    ]
  }
}
```

#### Hook en background con despertar
```json
{
  "hooks": {
    "Stop": [
      {
        "hooks": [
          {
            "type": "command",
            "shell": "powershell",
            "asyncRewake": true,
            "rewakeMessage": "El hook encontró un problema:",
            "rewakeSummary": "Problema detectado — revisando",
            "command": "...; if ($problema) { Write-Host 'Detalle del problema'; exit 2 }"
          }
        ]
      }
    ]
  }
}
```

**Reglas de exit codes para hooks:**

| Exit code | Efecto |
|-----------|--------|
| `exit 0` | Éxito silencioso |
| `exit 1` | Error — bloquea la acción en PreToolUse |
| `exit 2` | Solo con `asyncRewake: true` — despierta a Claude con el output |

**Variables de entorno disponibles en hooks:**

| Variable | Contenido |
|----------|-----------|
| `$env:CLAUDE_TOOL_INPUT` | JSON con el input de la herramienta que disparó el hook |
| `$env:CLAUDE_TOOL_RESULT` | JSON con el resultado (solo en PostToolUse) |

Ejemplo de uso:
```powershell
$input = $env:CLAUDE_TOOL_INPUT | ConvertFrom-Json
$filePath = $input.file_path   # para Edit/Write
$command  = $input.command     # para Bash
```

---

## 8. Referencia rápida

### ¿Qué usar cuándo?

| Situación | Herramienta |
|-----------|-------------|
| Acabo de implementar un cambio pequeño | `/validate` |
| Hice un cambio grande o crítico | `/validate-agent` |
| Quiero decidir qué hacer con cada problema | `/validate-smart` |
| Quiero que todo se valide solo sin hacer nada | Hooks automáticos (ya configurados) |
| El build falló al editar un .cs | Hook Op1 lo detecta → ejecuta `/validate` para el plan |
| El commit fue bloqueado | Hook Op4 detectó build/test error → ejecuta `/validate` |
| Claude despertó con un mensaje de validate-agent | Hook Op3 detectó problema → sigue las instrucciones |

### Comparativa de commands

| | /validate | /validate-agent | /validate-smart |
|--|-----------|-----------------|-----------------|
| **Contexto** | Principal | Aislado (subagente) | Principal |
| **Velocidad** | Rápido | Más lento | Lento (interactivo) |
| **Interactividad** | No | No | Sí (pregunta por cada 🟡) |
| **FIX-PLAN.md** | Sí | Sí | Solo para 🔴 |
| **Mejor para** | Uso diario | Cambios críticos | Revisión final |

### Comparativa de hooks

| | Op1 (PostToolUse) | Op3 (Stop) | Op4 (PreToolUse) | Op5 (PreToolUse agent) |
|--|--|--|--|--|
| **Trigger** | Editar .cs | Fin de turno | `git commit` | `git commit` (si Op4 pasa) |
| **Tipo** | command | command + asyncRewake | command | agent |
| **Profundidad** | Solo build | Build + test | Build + test | Arquitectura + migraciones |
| **Bloqueante** | No | No | Sí (exit 1) | Sí |
| **Velocidad** | ~5s | Background | ~15s | ~30-60s |

---

*Guía generada con Claude Code · Proyecto CalSystem · Phoenix Calibration DR*
