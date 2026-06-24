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
4. [Agents de validación](#agents)
   - [change-validator](#change-validator)
5. [Sistema de Agentes Expertos](#expertos)
   - [¿Qué es y para qué sirve?](#que-es)
   - [Los 6 agentes expertos](#los-6-agentes)
   - [El orquestador /consult](#consult)
   - [Atajos manuales por experto](#atajos)
   - [Flujo de trabajo con expertos](#flujo-expertos)
   - [¿Qué hace la IA en cada proceso?](#que-hace-la-ia)
6. [Hooks en settings.json](#hooks)
   - [Tipos de hook](#tipos-de-hook)
   - [Eventos disponibles](#eventos)
   - [Nuestros hooks](#nuestros-hooks)
7. [Flujos de trabajo de validación](#flujos)
8. [Cómo crear los tuyos](#crear)
9. [Referencia rápida](#referencia)
10. [Sistema de Gestión de Contexto](#context-manager)

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
│   ├── change-validator.md     ← Agente: quality gate completo (7 checks)
│   ├── domain-expert.md        ← Experto: DDD, entidades, enums, IRepository
│   ├── application-expert.md   ← Experto: CQRS, MediatR, Commands, Handlers
│   ├── infrastructure-expert.md← Experto: EF Core, SQLite, repositorios, migraciones
│   ├── api-expert.md           ← Experto: controllers ASP.NET, Swagger, Program.cs
│   ├── test-expert.md          ← Experto: xUnit, Moq, FluentAssertions, TDD
│   ├── architecture-expert.md  ← Experto: Clean Architecture, límites, SOLID
│   └── frontend-expert.md      ← Experto: Blazor WASM, Razor, HttpClient, CSS
├── commands/
│   ├── validate.md             ← /validate  (inline + FIX-PLAN.md)
│   ├── validate-agent.md       ← /validate-agent (delega al agente)
│   ├── validate-smart.md       ← /validate-smart (por severidad, interactivo)
│   ├── new-entity.md           ← /new-entity (generador DDD)
│   ├── consult.md              ← /consult (orquestador de expertos)
│   ├── domain-expert.md        ← /domain-expert (atajo directo)
│   ├── app-expert.md           ← /app-expert (atajo directo)
│   ├── infra-expert.md         ← /infra-expert (atajo directo)
│   ├── api-expert.md           ← /api-expert (atajo directo)
│   ├── test-expert.md          ← /test-expert (atajo directo)
│   ├── arch-expert.md          ← /arch-expert (atajo directo)
│   └── frontend-expert.md      ← /frontend-expert (atajo directo)
├── skills/
│   └── new-entity.md           ← skill DDD (misma lógica que /new-entity)
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

## 5. Sistema de Agentes Expertos

### ¿Qué es y para qué sirve?

El **Sistema de Agentes Expertos** es un índice inteligente de especialistas por capa del proyecto. Cada agente conoce profundamente su parte del stack (dominio, aplicación, infraestructura, API, tests, arquitectura) y puede asesorarte en diseño, revisión de código y detección de errores específicos de su capa.

**Problema que resuelve:** Cuando trabajas en un proyecto con múltiples capas y tecnologías, es difícil recordar las convenciones exactas de cada capa. El sistema enruta automáticamente a los expertos correctos según lo que estás haciendo, sin que tengas que saber cuál llamar.

**Dos modos de uso:**
- **Automático:** `/consult` detecta qué cambió en git o analiza tu descripción y enruta solo.
- **Manual:** invocas el experto que necesitas directamente con un atajo (`/domain-expert`, `/test-expert`, etc.) o forzando en el orquestador con `@experto`.

```
Sin el sistema:                      Con el sistema:
  "¿Cómo agrego este campo?"     →   /consult agregar campo a ServiceOrder
  → Tú decides qué revisar       →   → Sistema detecta: dominio + infraestructura afectados
  → Tú buscas las convenciones   →   → domain-expert diseña la entidad
  → Tú recuerdas el HasConversion→   → infrastructure-expert da la configuración EF Core
```

---

### Los 7 agentes expertos

Cada agente tiene un system prompt especializado con conocimiento específico de CalSystem: convenciones, archivos actuales, patrones correctos e incorrectos.

#### domain-expert

| Campo | Valor |
|-------|-------|
| **Archivo** | `.claude/agents/domain-expert.md` |
| **Model** | claude-sonnet-4-6 |
| **Tools** | Read, Glob, Grep |
| **Atajo** | `/domain-expert` |

**¿Qué sabe?**
- Diseño DDD: cuándo crear una entidad vs un value object vs solo una propiedad
- Convenciones: `private set`, constructor privado, factory method `Create(...)`, `= default!;`
- Cómo diseñar `IServiceOrderRepository` (qué métodos, qué firma)
- Invariantes de dominio: qué validar en el factory method, qué proteger en los métodos de comportamiento
- Dónde no poner lógica de negocio: no en services, no en handlers, siempre en la entidad

**¿Cuándo invocarlo?**
- Agregar o modificar una entidad (ServiceOrder, Technician, nueva entidad)
- Agregar o modificar un enum (OrderStatus, nuevo enum)
- Diseñar o cambiar `IServiceOrderRepository`
- Cualquier pregunta sobre si algo viola DDD

**¿Qué entrega?**
- El archivo C# completo listo para usar
- Justificación de cada decisión de diseño DDD
- Lista de qué otras capas deben actualizarse en consecuencia

---

#### application-expert

| Campo | Valor |
|-------|-------|
| **Archivo** | `.claude/agents/application-expert.md` |
| **Model** | claude-sonnet-4-6 |
| **Tools** | Read, Glob, Grep, Bash |
| **Atajo** | `/app-expert` |

**¿Qué sabe?**
- CQRS con MediatR 12: cuándo es Command (muta estado) vs Query (solo lectura)
- Estructura de los 3 archivos por caso de uso: `Command.cs` + `Handler.cs` + `DTO.cs`
- Cómo implementar `IRequest<T>` e `IRequestHandler<TRequest, TResponse>`
- Registro automático con `RegisterServicesFromAssembly` en `DependencyInjection.cs`
- Qué retorna cada tipo: Commands → Guid o bool / Queries → IEnumerable\<DTO\>
- Pipeline behaviors de MediatR y cuándo usarlos

**¿Cuándo invocarlo?**
- Nuevo caso de uso (Command + Handler o Query + Handler)
- Revisar si un handler existente tiene responsabilidades que no le corresponden
- Dudas sobre qué debe ir en el Command vs el Handler vs la entidad
- Agregar un DTO nuevo para una query

**¿Qué entrega?**
- Los 3 archivos completos del caso de uso
- Justificación del tipo de retorno
- Confirmación de si el handler queda registrado automáticamente en DI

---

#### infrastructure-expert

| Campo | Valor |
|-------|-------|
| **Archivo** | `.claude/agents/infrastructure-expert.md` |
| **Model** | claude-sonnet-4-6 |
| **Tools** | Read, Glob, Grep, Bash |
| **Atajo** | `/infra-expert` |

**¿Qué sabe?**
- Configuración de EF Core 9 + SQLite: `HasConversion<string>()`, `HasKey`, `IsRequired`, `HasMaxLength`
- `AsNoTracking()` en queries de solo lectura — cuándo y dónde
- Cuándo usar `EnsureCreated()` vs migraciones (`dotnet ef migrations add`)
- Implementación de repositorios: cómo traducir la interfaz del Domain a EF Core
- `SaveChangesAsync()` después de mutaciones, `FindAsync`/`FirstOrDefaultAsync` para lecturas
- Registro en `AddInfrastructure(connectionString)`: `AddDbContext` + `AddScoped`

**¿Cuándo invocarlo?**
- Agregar una nueva entidad al `AppDbContext`
- Implementar un método nuevo en `ServiceOrderRepository`
- Cambio de esquema que requiere migración
- Dudas sobre performance de queries EF Core (N+1, carga lazy vs eager)

**¿Qué entrega?**
- Cambios exactos en `AppDbContext.cs` (DbSet + OnModelCreating)
- Implementación del repositorio con el método nuevo
- Comando de migración exacto si aplica
- Confirmación de registro en DI

---

#### api-expert

| Campo | Valor |
|-------|-------|
| **Archivo** | `.claude/agents/api-expert.md` |
| **Model** | claude-sonnet-4-6 |
| **Tools** | Read, Glob, Grep, Bash |
| **Atajo** | `/api-expert` |

**¿Qué sabe?**
- Diseño RESTful: qué método HTTP usar (POST para crear, PUT para actualizar, GET para leer)
- Status codes correctos: 201 Created, 200 OK, 404 Not Found, 400 Bad Request
- Cómo conectar MediatR: `await _mediator.Send(new MiCommand(...))` en el controller
- Atributos de documentación Swagger: `[ProducesResponseType]`, `[SwaggerOperation]`
- DTOs de request: cuándo crear una clase separada vs usar el Command directamente
- Configuración de middleware en `Program.cs`

**¿Cuándo invocarlo?**
- Agregar un endpoint nuevo
- Revisar status codes de endpoints existentes
- Configurar Swagger para un endpoint
- Cambios en `Program.cs` (nuevo middleware, nueva configuración)

**¿Qué entrega?**
- El método del controller completo con todos sus atributos
- El DTO de request si el endpoint lo necesita
- Tabla de status codes con justificación semántica
- Nota sobre el Command/Query que debe existir en Application

---

#### test-expert

| Campo | Valor |
|-------|-------|
| **Archivo** | `.claude/agents/test-expert.md` |
| **Model** | claude-sonnet-4-6 |
| **Tools** | Read, Glob, Grep, Bash |
| **Atajo** | `/test-expert` |

**¿Qué sabe?**
- Ciclo TDD completo: escribir el test primero (rojo) → implementar mínimo (verde) → refactorizar
- Convención de nombres: `Handle_[Condición]_[ResultadoEsperado]`
- Moq: `Setup(r => r.MiMetodo(...)).Returns(...)`, `Verify(..., Times.Once)`
- FluentAssertions: `.Should().Be()`, `.Should().NotBe()`, `.Should().BeEmpty()`, etc.
- Cuántos tests: happy path + recurso no encontrado + verificación de mocks por cada handler
- `[Fact]` vs `[Theory]` + `[InlineData]` cuándo usar cada uno

**¿Cuándo invocarlo?**
- Nuevo handler que necesita tests
- Seguir TDD para una nueva funcionalidad
- Revisar cobertura de handlers existentes
- Dudas sobre cómo mockear algo específico con Moq

**¿Qué entrega?**
- La clase de test completa (no fragmentos)
- Output de `dotnet test` después de ejecutar
- Lista de casos cubiertos y pendientes

---

#### architecture-expert

| Campo | Valor |
|-------|-------|
| **Archivo** | `.claude/agents/architecture-expert.md` |
| **Model** | claude-sonnet-4-6 |
| **Tools** | Read, Glob, Grep |
| **Atajo** | `/arch-expert` |

**¿Qué sabe?**
- Jerarquía de dependencias de Clean Architecture: Domain ← Application ← Infrastructure ← Api
- Cómo leer los `.csproj` para detectar referencias incorrectas entre proyectos
- Principios SOLID aplicados a este proyecto: SRP en handlers, DIP con repositorios, ISP en interfaces
- Señales de violación: controller que accede a DbContext, handler que usa HttpContext, entidad que llama repositorio
- Cuándo un cambio introduce deuda arquitectural vs cuándo es una violación bloqueante

**¿Cuándo invocarlo?**
- Cambio que toca múltiples capas (domain + infra, app + api)
- Antes de agregar una referencia nueva entre proyectos
- Revisión de si un diseño propuesto cumple Clean Architecture
- Cuando el `/validate` reportó una violación de arquitectura

**¿Qué entrega?**
- Veredicto CUMPLE / VIOLA con evidencia (archivo + línea)
- Corrección exacta si hay violación
- Evaluación de riesgos del diseño propuesto a futuro

---

### El orquestador /consult

`/consult` es el punto de entrada principal del sistema. Analiza la tarea y decide automáticamente qué expertos invocar.

**Uso:**
```
/consult                                    ← auto desde git diff
/consult agregar campo CalibrationDate      ← auto por keywords
/consult @domain nueva entidad Equipment    ← fuerza experto específico
/consult @domain @infra entidad + repo      ← fuerza múltiples expertos
/consult @test AssignTechnicianHandler      ← fuerza solo test-expert
```

#### Modo automático — enrutamiento por keywords

Cuando describes la tarea en texto libre, el orquestador detecta keywords y enruta:

| Keywords en tu descripción | Expertos invocados |
|---------------------------|-------------------|
| entidad, entity, ServiceOrder, Technician, enum, IRepository | **domain** |
| handler, command, query, MediatR, IRequest, DTO | **application** |
| migración, EF Core, DbContext, repositorio, SQLite, tabla | **infrastructure** |
| endpoint, controller, HTTP, Swagger, POST, GET, PUT | **api** |
| test, prueba, mock, xunit, assert, TDD, FluentAssertions | **test** |
| arquitectura, clean, capas, layer, DI, SOLID | **architecture** |
| blazor, razor, componente, component, frontend, UI, página, HttpClient, modal, CSS, kanban | **frontend** |
| Tarea que afecta múltiples capas | todos los relevantes + **architecture** |

#### Modo automático — enrutamiento por archivos git

Cuando no hay descripción, el orquestador lee `git diff` y enruta según archivos modificados:

| Archivos en git diff | Expertos |
|---------------------|----------|
| `Domain/Entities/` | domain + architecture |
| `Domain/Interfaces/` | domain + application |
| `Application/Commands/` o `Queries/` | application + test |
| `Infrastructure/Persistence/` | infrastructure |
| `Infrastructure/DependencyInjection.cs` | infrastructure + architecture |
| `Api/Controllers/` | api |
| `Api/Program.cs` | api + architecture |
| `tests/` | test |
| 3+ capas afectadas | todos los relevantes + architecture |

#### Modo manual — forzar expertos con `@`

Cuando ya sabes qué experto necesitas, usa el prefijo `@` para omitir el enrutamiento automático:

| Flag | Experto forzado |
|------|----------------|
| `@domain` | domain-expert |
| `@app` | application-expert |
| `@infra` | infrastructure-expert |
| `@api` | api-expert |
| `@test` | test-expert |
| `@arch` | architecture-expert |
| `@frontend` | frontend-expert |

Puedes combinar varios `@` en el mismo comando para invocar múltiples expertos.

---

### Atajos manuales por experto

Cuando ya sabes exactamente qué experto necesitas, los atajos directos son más rápidos que pasar por el orquestador:

| Comando | Experto | Cuándo usarlo |
|---------|---------|---------------|
| `/domain-expert diseñar entidad Equipment` | domain-expert | Preguntas puramente de dominio DDD |
| `/app-expert nuevo handler para cerrar orden` | application-expert | Diseño de casos de uso CQRS |
| `/infra-expert agregar DbSet<Equipment>` | infrastructure-expert | Cambios en EF Core o repositorios |
| `/api-expert nuevo endpoint DELETE /api/orders/{id}` | api-expert | Diseño o revisión de endpoints |
| `/test-expert cubrir CloseOrderHandler` | test-expert | Escribir o revisar tests |
| `/arch-expert revisar si este diseño viola límites` | architecture-expert | Auditoría arquitectural |
| `/frontend-expert agregar página de reportes` | frontend-expert | Componentes Blazor, UI, CSS, HttpClient |

La diferencia entre `/consult @domain ...` y `/domain-expert ...` es solo conveniencia — ambos invocan el mismo agente.

---

#### frontend-expert

| Campo | Valor |
|-------|-------|
| **Archivo** | `.claude/agents/frontend-expert.md` |
| **Model** | claude-sonnet-4-6 |
| **Tools** | Read, Glob, Grep, Bash |
| **Atajo** | `/frontend-expert` |

**¿Qué sabe?**
- Estructura de Blazor WASM: `_Imports.razor` global, `App.razor`, `Layouts/`, `Pages/`, `Components/`
- Patrón de servicios HTTP: `OrderApiService` con `GetFromJsonAsync`, `PostAsJsonAsync`, `PutAsJsonAsync`
- Modelos DTO del cliente en `CalSystem.Web/Models/` — deben mantenerse alineados con los DTOs de Application
- Estado en componentes Razor: `@code`, `@bind`, `@onclick`, `EventCallback<T>`, `StateHasChanged()`
- CORS: la API acepta `http://localhost:5200`; el frontend apunta a `http://localhost:5112`
- Convención de rutas: API (5112) + Blazor DevServer (5200)
- CSS en `wwwroot/css/app.css`: variables CSS, clases kanban, modales, formularios

**¿Cuándo invocarlo?**
- Agregar una nueva página Razor (`@page "/ruta"`)
- Crear o modificar un componente reutilizable (modal, tarjeta)
- Integrar un nuevo endpoint de la API en `OrderApiService`
- Problemas de estado o renderizado en componentes
- Cambios de estilo en `app.css`

**¿Qué entrega?**
- El archivo `.razor` completo listo para usar
- Cambios necesarios en `OrderApiService.cs` si el componente llama a la API
- Nuevos modelos en `CalSystem.Web/Models/` si el endpoint retorna un tipo nuevo
- Instrucción de si hay que registrar algo en `CalSystem.Web/Program.cs`

---

### Flujo de trabajo con expertos

#### Ejemplo 1: agregar nueva funcionalidad completa

```
Tarea: "Agregar entidad Equipment con su endpoint GET /api/equipment"

1. /consult agregar entidad Equipment con endpoint GET
           │
           ├── Detecta: entidad (domain) + endpoint (api)
           │   + múltiples capas → agrega architecture
           │
           ├── 🏛️ domain-expert
           │   → Diseña Equipment.cs con private setters, Create(), XML docs
           │   → "Impactos: infra necesita DbSet, app necesita GetEquipmentQuery"
           │
           ├── 🌐 api-expert
           │   → Diseña EquipmentController.cs con GET /api/equipment
           │   → "Necesita GetEquipmentQuery en Application"
           │
           └── 🏗️ architecture-expert
               → "Equipment.cs en Domain/Entities: ✅ correcto"
               → "Controller depende de Application vía MediatR: ✅ correcto"

2. /app-expert implementar GetEquipmentQuery con handler
   → application-expert diseña Query + Handler + EquipmentDto

3. /infra-expert agregar Equipment al AppDbContext
   → infrastructure-expert da DbSet + OnModelCreating + registro en DI

4. /test-expert cubrir GetEquipmentHandler
   → test-expert escribe los tests antes del handler (TDD)
```

#### Ejemplo 2: revisión rápida de un cambio

```
Acabo de modificar ServiceOrder.cs para agregar un campo.

/consult
│
└── Orquestador ejecuta: git diff --name-only HEAD
    → detecta: Domain/Entities/ServiceOrder.cs
    → enruta a: domain-expert + architecture-expert
    │
    ├── 🏛️ domain-expert
    │   → revisa que el campo tenga private set
    │   → verifica que el factory method Create() fue actualizado
    │   → avisa si se necesita migración (pero no la hace él)
    │
    └── 🏗️ architecture-expert
        → verifica que no se introdujo ninguna dependencia nueva
        → confirma que las demás capas siguen referenciando Domain correctamente
```

---

### ¿Qué hace la IA en cada proceso?

Esta sección describe el comportamiento interno de la IA en cada proceso del sistema de expertos.

#### Proceso: `/consult` sin argumentos

```
Claude ejecuta:
  1. Bash → git diff --name-only HEAD (lee archivos modificados)
  2. Analiza la lista de rutas contra la tabla de enrutamiento
  3. Construye la lista de expertos a invocar
  4. Por cada experto: Agent(subagent_type: "nombre-expert", prompt: contexto completo)
  5. Consolida las respuestas en secciones etiquetadas
  6. Escribe la sección "Plan de acción consolidado" con el orden de implementación
```

**Variables que influyen en el enrutamiento:**
- Número de capas afectadas → si son 3+, siempre agrega architecture-expert
- Si hay `Domain/Entities/` en el diff → siempre domain + architecture (juntos)
- Si hay `Application/Commands/` → siempre application + test (el handler necesita tests)

#### Proceso: `/consult [descripción]` con texto

```
Claude ejecuta:
  1. Analiza $ARGUMENTS buscando keywords de cada capa
  2. Construye la lista de expertos por coincidencias
  3. Si hay múltiples capas → agrega architecture-expert
  4. Invoca los expertos en paralelo con el texto completo de la tarea
  5. Si un experto señala dependencias con otra capa, lo agrega a la lista
```

**Razonamiento interno:** el orquestador no solo mapea keywords, también evalúa si la tarea completa cruzará capas aunque la descripción no lo diga explícitamente (por ejemplo, "agregar campo" siempre afecta domain + infra aunque el usuario solo mencione la entidad).

#### Proceso: agente domain-expert ejecutando

```
Claude (como domain-expert) ejecuta:
  1. Read → src/CalSystem.Domain/Entities/ (lee entidades existentes)
  2. Glob → busca todos los archivos en Domain/
  3. Grep → busca la interfaz IServiceOrderRepository
  4. Analiza: ¿el cambio viola alguna invariante? ¿hay private setters? ¿factory method?
  5. Escribe el código C# nuevo o corregido
  6. Lista los archivos de otras capas que deben actualizarse
```

**Qué NO hace:** no escribe en Infrastructure, no mira controllers, no corre `dotnet build`.
Su scope es exclusivamente Domain — esa restricción de tools (Read, Glob, Grep — sin Bash) es intencional.

#### Proceso: agente application-expert ejecutando

```
Claude (como application-expert) ejecuta:
  1. Read → src/CalSystem.Application/ (lee Commands, Queries, Handlers existentes)
  2. Read → src/CalSystem.Domain/Interfaces/ (lee IServiceOrderRepository)
  3. Analiza: ¿es Command o Query? ¿qué retorna? ¿necesita nuevo método en repositorio?
  4. Escribe los 3 archivos: Command/Query + Handler + DTO (si aplica)
  5. Verifica que queda registrado en RegisterServicesFromAssembly
  6. Puede ejecutar Bash para confirmar estructura o dependencias
```

#### Proceso: agente infrastructure-expert ejecutando

```
Claude (como infrastructure-expert) ejecuta:
  1. Read → AppDbContext.cs (lee DbSets y OnModelCreating actuales)
  2. Read → ServiceOrderRepository.cs (lee implementaciones existentes)
  3. Read → DependencyInjection.cs (lee qué está registrado)
  4. Escribe: cambios en OnModelCreating + nuevo DbSet + implementación de repositorio
  5. Bash → puede ejecutar dotnet ef migrations list para ver estado actual
  6. Da el comando exacto de migración si es necesario
```

#### Proceso: agente test-expert ejecutando (modo TDD)

```
Claude (como test-expert) ejecuta:
  1. Read → tests/CalSystem.Tests/ (lee tests existentes para mantener estilo)
  2. Read → el Handler a testear (lee qué parámetros acepta, qué retorna)
  3. Escribe el test PRIMERO (fase roja) — antes de que exista el handler
  4. Bash → dotnet test (confirma que el test falla con el error correcto)
  5. Espera que el handler sea implementado por application-expert
  6. Bash → dotnet test (confirma que el test pasa)
```

#### Proceso: agente architecture-expert ejecutando

```
Claude (como architecture-expert) ejecuta:
  1. Glob → *.csproj (lee todos los archivos de proyecto)
  2. Grep → busca <ProjectReference> en cada .csproj
  3. Construye el grafo de dependencias actual
  4. Compara contra la jerarquía permitida: Domain ← App ← Infra ← Api
  5. Read → archivos específicos involucrados en la tarea
  6. Grep → busca using statements sospechosos (Infrastructure en Application, etc.)
  7. Emite veredicto con evidencia concreta y corrección si aplica
```

**Nota sobre tools restringidas:** architecture-expert tiene solo Read, Glob, Grep (sin Bash) porque su trabajo es análisis estático — leer archivos y referencias. No necesita ejecutar comandos, y restringirlo lo hace más predecible y rápido.

---

## 6. Hooks en settings.json

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

## 7. Flujos de trabajo de validación

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

### Flujo de Pull Request

```
/crear-pr [título opcional]
    │
    PASO 1: detecta archivos pendientes (git status)
    │   └── sin cambios → avisa y para
    │
    PASO 2: valida build + tests
    │   ├── FAIL → ❌ BLOQUEADO — no se crea nada
    │   └── PASS → continúa
    │
    PASO 3: determina rama
    │   ├── ya en feature/* → usa la rama actual
    │   └── en main → crea feature/<slug-del-titulo>
    │
    PASO 4: git add -A + git commit (con título + Co-Authored-By)
    PASO 5: git push -u origin <rama>
    PASO 6: gh pr create --title ... --base main --body ...
    │
    └── ✅ PR CREADO — devuelve URL de GitHub
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

## 8. Cómo crear los tuyos

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

## 9. Referencia rápida

### ¿Qué usar cuándo?

#### Validación de calidad

| Situación | Herramienta |
|-----------|-------------|
| Acabo de implementar un cambio pequeño | `/validate` |
| Hice un cambio grande o crítico | `/validate-agent` |
| Quiero decidir qué hacer con cada problema | `/validate-smart` |
| Quiero que todo se valide solo sin hacer nada | Hooks automáticos (ya configurados) |
| El build falló al editar un .cs | Hook Op1 lo detecta → ejecuta `/validate` para el plan |
| El commit fue bloqueado | Hook Op4 detectó build/test error → ejecuta `/validate` |
| Claude despertó con un mensaje de validate-agent | Hook Op3 detectó problema → sigue las instrucciones |

#### Consulta a expertos

| Situación | Herramienta |
|-----------|-------------|
| No sé qué capas afecta mi tarea | `/consult [descripción]` — enruta automáticamente |
| Acabo de editar archivos, quiero revisión experta | `/consult` (sin args, lee git diff) |
| Solo quiero opinión de dominio DDD | `/domain-expert [descripción]` |
| Quiero diseñar un nuevo caso de uso CQRS | `/app-expert [descripción]` |
| Tengo dudas sobre EF Core o migraciones | `/infra-expert [descripción]` |
| Necesito diseñar un endpoint o revisar status codes | `/api-expert [descripción]` |
| Quiero escribir tests o seguir TDD | `/test-expert [descripción]` |
| Quiero verificar que no violé Clean Architecture | `/arch-expert [descripción]` |
| Cambio en UI Blazor o componente Razor | `/frontend-expert [descripción]` |
| Tarea que toca domain + infra (nueva entidad) | `/consult @domain @infra [descripción]` |
| Tarea que toca API + frontend | `/consult @api @frontend [descripción]` |
| Nueva funcionalidad completa (todas las capas) | `/consult [descripción]` — enruta a todos |

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

### Mapa del sistema de expertos

```
Tu tarea
   │
   ▼
/consult [descripción o vacío]
   │
   ├── ¿Contiene @domain/@app/@infra/@api/@test/@arch?
   │   └── SÍ → modo manual: invoca SOLO esos expertos
   │
   └── NO → modo automático
       ├── Sin args → lee git diff → enruta por archivos modificados
       └── Con args → analiza keywords → enruta por tecnología mencionada
           │
           ▼
   Lista de expertos a invocar
           │
   ┌───────┼────────┬────────┬──────────┬──────────┐
   ▼       ▼        ▼        ▼          ▼          ▼
domain  app     infra    api        test      arch      frontend
expert  expert  expert   expert     expert    expert    expert
   │       │        │        │          │          │
   └───────┴────────┴────────┴──────────┴──────────┘
                           │
                           ▼
              Respuestas por sección etiquetada
                           │
                           ▼
              ✅ Plan de acción consolidado
              (orden: Domain → App → Infra → Api → Tests)
```

### Comparativa de expertos

| | domain | application | infrastructure | api | test | architecture | frontend |
|--|:------:|:-----------:|:--------------:|:---:|:----:|:------------:|:--------:|
| **Capa** | Domain | Application | Infrastructure | Api | Tests | Transversal | CalSystem.Web |
| **Tools** | R,G,Grep | R,G,Grep,Bash | R,G,Grep,Bash | R,G,Grep,Bash | R,G,Grep,Bash | R,G,Grep | R,G,Grep,Bash |
| **Escribe código** | ✅ | ✅ | ✅ | ✅ | ✅ | Sugerencias | ✅ |
| **Ejecuta comandos** | ❌ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| **Emite veredicto** | Diseño DDD | Diseño CQRS | Config EF Core | REST semántico | Cobertura | CUMPLE/VIOLA | Componente Blazor |

*Guía generada con Claude Code · Proyecto CalSystem · Phoenix Calibration DR*

---

## 10. Sistema de Gestión de Contexto

El sistema de contexto permite a Claude recordar el historial del proyecto entre sesiones
sin releer todos los archivos desde cero. Ahorra tokens y acelera la resolución de problemas.

### Estructura de archivos

```
.claude/context/
  INDEX.md          ← índice maestro ultra-compacto (auto-cargado en SessionStart)
  active/           ← contextos activos (< 6 meses)
    ctx-NNN-slug.md
  archive/          ← backlog de contextos viejos (> 6 meses)
    ctx-NNN-slug.md
```

### Cómo funciona

**Al iniciar sesión** — el hook `SessionStart` carga `INDEX.md` automáticamente.
Claude ve de inmediato qué existe en el proyecto sin ningún prompt manual.

**Cuando necesitas contexto específico** — usa `/ctx-search` para cargar solo el archivo relevante.
No se lee el proyecto completo: INDEX.md (< 50 tokens) → archivo específico (~100 tokens).

**Al terminar trabajo importante** — usa `/ctx-save` para que el agente `context-manager`
cree un mini-contexto con resumen, archivos clave y decisiones. El INDEX.md se actualiza automáticamente.

**Mantenimiento mensual** — `/ctx-cleanup` archiva contextos > 6 meses (respetando un grace
period para los que se consultan frecuentemente: `accessed >= 3`).

### Comandos de contexto

| Comando | Acción |
|---------|--------|
| `/ctx-save` | Auto-detecta trabajo desde git, crea mini-contexto |
| `/ctx-save @tooling Nuevo agente X` | Fuerza el tipo `tooling` y el título |
| `/ctx-search @frontend` | Filtra por tag, carga solo esos archivos |
| `/ctx-search EF Core` | Búsqueda por keyword en títulos y tags |
| `/ctx-cleanup` | Archiva los viejos, mantiene los frecuentes |

### Formato del INDEX.md

```markdown
# CalSystem — Índice de Contexto
> Actualizado: YYYY-MM-DD | Activos: N | Archivados: N

## Activos
| ID | Título | Tags | Tipo | Fecha |
|----|--------|------|------|-------|
| [ctx-001](active/ctx-001-slug.md) | Título corto | tag1,tag2 | setup | 2026-06 |
```

**Regla de diseño:** el INDEX.md completo debe ser legible en menos de 50 tokens.
Una fila por contexto — no se añade detalle aquí, solo el título y los tags.

### Formato de un archivo de contexto

```markdown
---
id: ctx-NNN
title: Título descriptivo
date: YYYY-MM-DD
tags: [tag1, tag2]
type: setup | feature | bugfix | decision | tooling
accessed: 0
---

## Resumen
[Párrafo suficiente para entender el contexto sin abrir otro archivo]

## Archivos clave
- `ruta/archivo.cs` — descripción breve

## Decisiones tomadas
- Por qué se eligió X sobre Y

## Problemas resueltos
- Problema → Solución

## Relacionado
- [[ctx-NNN]] — contexto relacionado
```

### Agente `context-manager`

El agente especializado que crea y actualiza los archivos de contexto.
Usa `Bash, Read, Glob, Grep, Write, Edit`.

**Flujo interno del agente:**
1. Lee `git log` y `git diff` para entender qué cambió (sin leer todo el proyecto)
2. Lee máx 5 archivos clave identificados por el diff
3. Determina el siguiente ID desde INDEX.md
4. Escribe `ctx-NNN-slug.md` en `active/`
5. Actualiza la tabla del INDEX.md

**Tags disponibles:** `domain`, `app`, `api`, `infra`, `frontend`, `test`, `tooling`, `git`, `arch`

### Hook SessionStart

```json
"SessionStart": [
  {
    "hooks": [
      {
        "type": "command",
        "shell": "powershell",
        "command": "if (Test-Path '.claude/context/INDEX.md') { Get-Content '.claude/context/INDEX.md' | Select-Object -First 25 }"
      }
    ]
  }
]
```

Carga las primeras 25 líneas del INDEX.md al iniciar la sesión.
Si el directorio no existe, el hook es silencioso.

### Flujo de contexto completo

```
Inicio de sesión
    │
    └── [AUTO SessionStart] lee INDEX.md → Claude ve el índice

Trabajo de desarrollo
    │
    ├── /ctx-search @tooling  → filtra INDEX.md → carga ctx específico
    └── ... editar, crear, corregir código ...

Al terminar trabajo
    │
    └── /ctx-save "Título de lo que hice"
            │
            └── context-manager:
                  git log + git diff → entiende cambios
                  lee 5 archivos clave
                  crea ctx-NNN-slug.md
                  actualiza INDEX.md

Mantenimiento (mensual)
    │
    └── /ctx-cleanup
            │
            ├── ctx viejo + poco usado → archive/
            ├── ctx viejo + muy usado  → mantener (grace period)
            └── ctx reciente           → mantener activo
```

### Ahorro de tokens

| Escenario | Sin sistema | Con sistema |
|-----------|-------------|-------------|
| Entender qué existe | Leer 20+ archivos | INDEX.md (1 archivo) |
| Recordar la capa Domain | Releer entidades e interfaces | `/ctx-search @domain` |
| Saber qué hooks hay | Leer settings.json + GUIDE.md | `/ctx-search @tooling` |
| Inicio de sesión | Sin contexto | SessionStart auto-carga índice |
