# EVIDENCE.md — CalSystem
## Evidencia de uso de Claude Code (8 temas)

> Proyecto: Sistema de Órdenes de Servicio Técnico — Phoenix Calibration DR  
> Stack: .NET 10 · Clean Architecture · EF Core · SQLite · xUnit

---

## T-01 — Plan Mode

**Cuándo:** Antes de implementar la capa Application (Fase 3).

**Prompt usado en Claude Code:**
```
Voy a implementar la capa de Application con CQRS usando MediatR.
Necesito: CreateOrderCommand, AssignTechnicianCommand y GetOrdersByStatusQuery,
cada uno con su handler. ¿Puedes generar un plan de implementación antes de empezar?
```

**Plan generado por Claude (extracto):**

Claude entró en Plan Mode y propuso:

1. Crear `DependencyInjection.cs` con `AddApplication()` registrando todos los handlers vía `RegisterServicesFromAssembly`.
2. Para cada caso de uso, estructura: `Command.cs` (record con `IRequest<T>`) + `Handler.cs` (clase con `IRequestHandler<TRequest, TResponse>`).
3. Orden de implementación: UC-01 (CreateOrder) → UC-02 (AssignTechnician) → UC-03 (GetOrdersByStatus), cada uno en su carpeta.
4. Para UC-03, crear también el `OrderDto.cs` en la carpeta de la Query.

**Ajuste al plan antes de aprobar:**
- Se cambió el tipo de retorno de `AssignTechnicianCommand` de `Unit` a `bool` para dar feedback explícito al controller sobre si la orden existía.

**Resultado:** Plan aprobado e implementado. 7 tests pasando (base UC-01/02/03; 11 en total tras UC-04 + gestión de técnicos).

---

## T-02 — CLAUDE.md

**Cuándo:** Inicio del proyecto, fase de setup.

**Acción:** Se ejecutó `/init` en la raíz del proyecto. Claude generó una estructura base que fue personalizada con:
- Stack completo (.NET 10, EF Core, MediatR, xUnit)
- Convenciones del proyecto (private set, records inmutables, handlers en mismo directorio)
- Rutas importantes de cada capa
- Restricciones arquitecturales (no lógica fuera del Domain, no referenciar Infrastructure desde Application)
- Comandos frecuentes de CLI

**Archivo resultante:** `CLAUDE.md` en la raíz del repositorio.

---

## T-03 — Test-Driven Development (Ciclo Rojo → Verde)

### Caso de uso: UC-01 CreateOrderHandler

### 1. Test escrito ANTES del handler

```csharp
[Fact]
public async Task Handle_ValidCommand_ReturnsNewOrderId()
{
    // Arrange
    var command = new CreateOrderCommand(
        CustomerName: "Ana García",
        Equipment: "Termómetro PT-100",
        ProblemDescription: "Lectura errática"
    );

    _repositoryMock
        .Setup(r => r.AddAsync(It.IsAny<ServiceOrder>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBe(Guid.Empty);
    _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ServiceOrder>()), Times.Once);
}
```

### 2. Resultado del test en ROJO (falla esperada)

Al ejecutar `dotnet test` con el test pero sin el handler:

```
error CS0246: The type or namespace name 'CreateOrderHandler' could not be found
Build FAILED.
```

El test ni compiló — comportamiento correcto en TDD. El handler no existía aún.

### 3. Implementación mínima del handler

```csharp
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IServiceOrderRepository _repository;

    public CreateOrderHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = ServiceOrder.Create(
            request.CustomerName,
            request.Equipment,
            request.ProblemDescription
        );

        await _repository.AddAsync(order);

        return order.Id;
    }
}
```

### 4. Resultado del test en VERDE

```
Passed! - Failed: 0, Passed: 7, Skipped: 0, Total: 7, Duration: 932 ms
```

> **Estado actual:** 11 tests pasando tras la adición de CloseOrder (3 tests) y CreateTechnician (2 tests).

---

## T-04 — Documentation Guidelines

### Parte A: README.md generado con Claude

**Prompt exacto usado:**
```
Genera el README.md principal del proyecto. El proyecto se llama "CalSystem"
(Sistema de Órdenes de Servicio Técnico para Phoenix Calibration DR).
Stack: .NET 10, Clean Architecture, EF Core SQLite, MediatR, xUnit.
Incluye: descripción, requisitos previos, cómo instalar y ejecutar,
los 3 endpoints disponibles con ejemplos de request/response, y cómo correr los tests.
```

**Resultado:** `README.md` en la raíz del proyecto con documentación completa de los 3 endpoints.

### Parte B: XML comments en ServiceOrder

**Prompt exacto usado:**
```
Agrega XML documentation comments a la clase ServiceOrder en
src/CalSystem.Domain/Entities/ServiceOrder.cs.
Documenta la clase, todas las propiedades y los métodos Create, AssignTechnician y Close.
```

**Resultado:** La clase `ServiceOrder` tiene XML comments en clase, propiedades y los 3 métodos. Visible en IntelliSense de VS/Rider.

---

## T-05 — Security Review

**Endpoint revisado:** `POST /api/orders` en `ServiceOrdersController.cs`

**Prompt usado:**
```
Revisa el endpoint POST /api/orders en ServiceOrdersController.cs desde una perspectiva de seguridad.
Considera: validación de input, inyección, exposición de datos sensibles,
autenticación/autorización, manejo de errores.
¿Cuáles son los riesgos actuales y cómo los mitigarías?
```

**Hallazgos de Claude:**

1. **Falta de validación de input** — Actualmente el endpoint acepta campos vacíos o nulos. Mitigación: agregar FluentValidation con `CreateOrderCommandValidator` que valide `CustomerName.NotEmpty()`, `Equipment.NotEmpty()`, `ProblemDescription.NotEmpty().MaximumLength(1000)`.

2. **Sin autenticación/autorización** — Cualquier cliente puede crear órdenes. Para producción: agregar JWT Bearer authentication (`AddAuthentication().AddJwtBearer()`). Aceptable para MVP de demostración.

3. **Sin rate limiting** — Endpoint expuesto a flood de requests. Mitigación: `builder.Services.AddRateLimiter(...)` con política fija por IP.

4. **Exposición de stack traces** — En desarrollo los errores no controlados exponen el stack. Mitigación: middleware global de manejo de excepciones que retorne `ProblemDetails` sin stack en producción.

5. **No hay riesgos de SQL injection** — EF Core usa queries parametrizadas automáticamente. ✅

**Corrección implementada:** Se documentó que los campos son requeridos. La validación de FluentValidation se deja como deuda técnica ya que está fuera del alcance del MVP.

---

## T-06 — GitHub MCP Integration

**Acción realizada:** Creación de Issue en el repositorio GitHub via GitHub MCP.

**Prompt usado:**
```
Usa GitHub MCP para crear un issue en el repositorio con el título:
"feat: agregar validación de campos vacíos en CreateOrderCommand"
y descripción: "Actualmente el endpoint acepta campos vacíos.
Se debe agregar validación de FluentValidation para CustomerName, Equipment y ProblemDescription."
```

**Resultado:** Issue #1 creado en GitHub via GitHub CLI (gh).  
**URL del recurso:** https://github.com/enderson-phoenix/TareaPluralsight/issues/1

---

## T-07 — Custom Skill

**Archivo creado:** `.claude/skills/new-entity.md` (también disponible en `.claude/commands/new-entity.md`)

**Descripción:** Genera una entidad de dominio DDD completa con propiedades tipadas, constructor privado, método `Create(...)` estático, y XML documentation comments.

**Ejemplo de uso:**
```
/new-entity Equipment serialNumber:string brand:string calibrationDate:DateTime
```

**Output generado por Claude al ejecutar el skill:**

```csharp
namespace CalSystem.Domain.Entities;

/// <summary>
/// Represents a piece of equipment registered for calibration or service.
/// </summary>
public class Equipment
{
    /// <summary>Gets the unique identifier of the equipment.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the manufacturer serial number of the equipment.</summary>
    public string SerialNumber { get; private set; } = default!;

    /// <summary>Gets the brand or manufacturer name.</summary>
    public string Brand { get; private set; } = default!;

    /// <summary>Gets the date of the last calibration.</summary>
    public DateTime CalibrationDate { get; private set; }

    private Equipment() { }

    /// <summary>
    /// Creates a new Equipment instance.
    /// </summary>
    public static Equipment Create(string serialNumber, string brand, DateTime calibrationDate)
    {
        return new Equipment
        {
            Id = Guid.NewGuid(),
            SerialNumber = serialNumber,
            Brand = brand,
            CalibrationDate = calibrationDate
        };
    }
}
```

**Archivo:** `src/CalSystem.Domain/Entities/Equipment.cs`  
**Migración necesaria:**
```bash
dotnet ef migrations add AddEquipmentTable \
  --project src/CalSystem.Infrastructure \
  --startup-project src/CalSystem.Api
```

---

## T-08 — Custom Hook

**Hook implementado en `.claude/settings.json`:**

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
            "statusMessage": "change-validator: checking build...",
            "command": "try { $p = ($env:CLAUDE_TOOL_INPUT | ConvertFrom-Json -EA Stop).file_path; if ($p -match '\\.cs$') { $r = dotnet build --no-incremental -v q 2>&1; $e = ($r | Select-String ' error ').Count; if ($e -gt 0) { Write-Host ('[FAIL] ' + $e + ' error(s)') } else { Write-Host '[PASS] Build OK' } } } catch {}"
          }
        ]
      }
    ]
  }
}
```

**Qué hace:** Cada vez que Claude edita o escribe un archivo `.cs`, el hook ejecuta `dotnet build` automáticamente en background. Si hay errores de compilación, muestra `[FAIL] N error(s)`. Si compila bien, muestra `[PASS] Build OK`.

**Cuándo se activa:** Evento `PostToolUse` con matcher `Edit|Write`, filtrado a archivos `.cs` dentro del comando.

**Hooks adicionales implementados:**
- `PreToolUse` Op4: cancela `git commit` si build o tests fallan
- `PreToolUse` Op5: análisis de severidad (agente) antes de cada commit
- `Stop` Op3: validación en background al terminar turno de Claude

Ver `.claude/settings.json` y `.claude/GUIDE.md` para el código completo.

---

## UC-04 — Cerrar Orden de Servicio (CloseOrder)

**Cuándo:** Tras completar UC-01/02/03 y los 7 tests base, se extendió el sistema con el flujo de cierre de órdenes incluyendo notas del técnico.

### Plan Mode — diseño de la feature

**Prompt usado en Claude Code:**
```
Quiero agregar el campo Notes a ServiceOrder para que el técnico pueda
dejar observaciones al cerrar una orden. Necesito el plan completo:
dominio, application, infraestructura, endpoint y tests.
```

**Plan generado por Claude:**

| Capa | Cambios |
|------|---------|
| Domain | Agregar `Notes { get; private set; }` a `ServiceOrder`; actualizar `Close()` para aceptar `string? notes` |
| Application | Crear `CloseOrderCommand(Guid OrderId, string? Notes)` + `CloseOrderHandler`; agregar `Notes` al `OrderDto` |
| Infrastructure | Agregar `entity.Property(e => e.Notes).HasMaxLength(2000)` en `AppDbContext` |
| Api | Nuevo endpoint `PUT /api/orders/{id}/close` con `CloseOrderRequest(string? Notes)` |
| Tests | 3 casos: cierre con notes, cierre sin notes, orden inexistente |

### TDD — ciclo Rojo → Verde

**Test escrito ANTES de implementar el handler:**

```csharp
[Fact]
public async Task Handle_ExistingOrderWithNotes_ReturnsTrueAndPersistsNotes()
{
    // Arrange
    var order = ServiceOrder.Create("Ana García", "Termómetro PT-100", "Lectura errática");
    var command = new CloseOrderCommand(order.Id, "Sensor calibrado. Lectura corregida a ±0.1°C.");

    _repositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
    _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ServiceOrder>())).Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeTrue();
    order.Status.Should().Be(OrderStatus.Closed);
    order.Notes.Should().Be("Sensor calibrado. Lectura corregida a ±0.1°C.");
    _repositoryMock.Verify(r => r.UpdateAsync(order), Times.Once);
}
```

**Resultado en ROJO:** `error CS0246: 'CloseOrderHandler' could not be found. Build FAILED.`

**Implementación del handler (mínima para pasar el test):**

```csharp
public class CloseOrderHandler : IRequestHandler<CloseOrderCommand, bool>
{
    private readonly IServiceOrderRepository _repository;
    public CloseOrderHandler(IServiceOrderRepository repository) => _repository = repository;

    public async Task<bool> Handle(CloseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId);
        if (order is null) return false;
        order.Close(request.Notes);
        await _repository.UpdateAsync(order);
        return true;
    }
}
```

**Resultado en VERDE:**
```
Passed! - Failed: 0, Passed: 10, Skipped: 0, Total: 10, Duration: 1 043 ms
```

> **Estado actual:** 11 tests tras agregar `CreateTechnicianHandlerTests` (2 tests).

### Archivos modificados/creados

| Archivo | Cambio |
|---------|--------|
| `src/CalSystem.Domain/Entities/ServiceOrder.cs` | `Notes { get; private set; }` + `Close(string? notes)` |
| `src/CalSystem.Application/Orders/Commands/CloseOrder/CloseOrderCommand.cs` | Nuevo — record `IRequest<bool>` |
| `src/CalSystem.Application/Orders/Commands/CloseOrder/CloseOrderHandler.cs` | Nuevo — handler |
| `src/CalSystem.Application/Orders/Queries/GetOrdersByStatus/OrderDto.cs` | Campo `Notes` agregado |
| `src/CalSystem.Infrastructure/Persistence/AppDbContext.cs` | `HasMaxLength(2000)` para Notes |
| `src/CalSystem.Api/Controllers/ServiceOrdersController.cs` | Endpoint `PUT {id}/close` |
| `tests/CalSystem.Tests/Orders/Commands/CloseOrderHandlerTests.cs` | Nuevo — 3 tests |

---

## Bonus — Sistema de Agentes Expertos

**Qué es:** Un sistema de 6 agentes especializados por capa + un orquestador con routing automático, creado para asistir el desarrollo de CalSystem con expertise específico de cada tecnología.

### Agentes creados en `.claude/agents/`

| Agente | Especialidad |
|--------|-------------|
| `domain-expert` | Entidades DDD, enums, interfaces de repositorio, invariantes |
| `application-expert` | Commands, Queries, Handlers, DTOs, MediatR 12 |
| `infrastructure-expert` | EF Core 9, SQLite, repositorios, migraciones |
| `api-expert` | ASP.NET Core controllers, routing, Swagger, status codes |
| `test-expert` | xUnit, Moq, FluentAssertions, ciclo TDD |
| `architecture-expert` | Clean Architecture, límites entre capas, SOLID |

### Orquestador `/consult`

**Archivo:** `.claude/commands/consult.md`

**Dos modos de uso:**
- **Automático:** detecta capas afectadas por keywords o `git diff` y enruta a los expertos correspondientes
- **Manual:** el usuario prefija con `@domain`, `@app`, `@infra`, `@api`, `@test` o `@arch`

**Demo real — usado para diseñar UC-04:**

```
/consult agregar campo Notes a ServiceOrder para que el técnico pueda
         dejar observaciones al cerrar una orden
```

El orquestador detectó keywords de 5 capas (`entity`, `campo`, `cerrar`, `orden`) e invocó
`domain-expert`, `application-expert`, `infrastructure-expert`, `api-expert` y `test-expert`.
Cada uno entregó su recomendación en su área. El plan consolidado fue aprobado e implementado
exitosamente: 3 nuevos archivos, 4 modificados, 10/10 tests verdes.

### Atajos manuales (6 comandos)

```
/domain-expert   → invoca domain-expert directamente
/app-expert      → invoca application-expert
/infra-expert    → invoca infrastructure-expert
/api-expert      → invoca api-expert
/test-expert     → invoca test-expert
/arch-expert     → invoca architecture-expert
```

Ver `.claude/GUIDE.md` para documentación completa del sistema de expertos.

---

## Bonus 4 — Sistema de Gestión de Contexto (`/ctx-save`, `/ctx-search`, `context-manager`)

**Qué es:** Sistema de historial de contexto por proyecto que permite a Claude recordar el trabajo previo sin releer todos los archivos desde cero en cada sesión. Implementa un índice compacto auto-cargado en `SessionStart`, archivos de mini-contexto estructurados y archivado automático por antigüedad.

### Componentes implementados

| Archivo | Descripción |
|---------|-------------|
| `.claude/context/INDEX.md` | Índice maestro ultra-compacto (< 50 tokens), auto-cargado en cada sesión |
| `.claude/context/active/ctx-00N-*.md` | 6 archivos de contexto semilla cubriendo todo el historial del proyecto |
| `.claude/agents/context-manager.md` | Agente que crea mini-contextos desde git diff y actualiza el índice |
| `.claude/commands/ctx-save.md` | `/ctx-save [título]` — guarda contexto del trabajo actual |
| `.claude/commands/ctx-search.md` | `/ctx-search [@tag | keyword]` — busca sin cargar archivos irrelevantes |
| `.claude/commands/ctx-cleanup.md` | Archiva contextos > 6 meses (grace period para los muy consultados) |

### Flujo del sistema

```
SessionStart hook → INDEX.md auto-cargado → Claude tiene visión del proyecto

/ctx-search @tooling → lee INDEX.md → carga solo archivos con tag "tooling"

/ctx-save "Feature X" → context-manager:
    git log + git diff → identifica cambios
    lee máx 5 archivos clave
    crea ctx-NNN-slug.md en active/
    actualiza INDEX.md (nueva fila + contador)

/ctx-cleanup → compara fechas → archiva viejos → grace period si accessed ≥ 3
```

### Regla de grace period

Contextos con más de 6 meses NO se archivan si `accessed >= 3`. El campo `accessed`
se incrementa cada vez que `/ctx-search` carga ese archivo, indicando que el equipo
lo consulta activamente. Conservar contextos frecuentes aunque sean viejos evita
perder conocimiento valioso.

### Ahorro de tokens estimado

| Escenario | Tokens sin sistema | Tokens con sistema |
|-----------|-------------------|-------------------|
| Entender qué existe en el proyecto | ~2000 (leer 20+ archivos) | ~50 (INDEX.md) |
| Recordar la capa Domain | ~800 (releer entidades) | ~150 (ctx-002) |
| Inicio de sesión con contexto | 0 (empieza ciego) | ~50 (SessionStart auto-carga) |

---

## Bonus 3 — Automatización de Pull Requests (`/crear-pr` + `pr-creator`)

**Qué es:** Comando y agente especializado que automatizan el flujo completo de Pull Request: valida build y tests, crea una rama `feature/`, hace commit de los cambios pendientes, push al remoto y abre el PR en GitHub via `gh` CLI. Nunca commitea directamente a `main`.

### Archivos creados

| Archivo | Descripción |
|---------|-------------|
| `.claude/commands/crear-pr.md` | Comando `/crear-pr [título]` — delegador al agente `pr-creator` |
| `.claude/agents/pr-creator.md` | Agente con flujo de 7 pasos: detectar → validar → rama → commit → push → PR → reporte |

### Flujo del agente `pr-creator`

```
/crear-pr [título opcional]
    │
    PASO 1: git status — detecta cambios pendientes
    PASO 2: dotnet build + dotnet test — ❌ BLOQUEADO si falla alguno
    PASO 3: genera feature/<slug-del-titulo> (o usa rama actual si ya es feature)
    PASO 4: git add -A + git commit con Co-Authored-By
    PASO 5: git push -u origin <rama>
    PASO 6: gh pr create --title --base main --body (Resumen + Plan de pruebas)
    PASO 7: tabla de estado + URL del PR creado
```

### Ejemplos de uso

```
/crear-pr
/crear-pr Agregar campo Notes a ServiceOrder
/crear-pr Configurar MCP GitHub en el proyecto
```

### Configuración MCP relacionada

El archivo `.mcp.json` en la raíz define el servidor GitHub MCP disponible para el proyecto:

```json
{
  "mcpServers": {
    "github": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-github"],
      "env": { "GITHUB_TOKEN": "${GITHUB_PERSONAL_ACCESS_TOKEN}" }
    }
  }
}
```

Este archivo se commitea al repo para que todo el equipo comparta la misma configuración MCP al abrir el proyecto en Claude Code.

---

## Bonus 2 — Frontend Blazor WebAssembly + Gestión de Técnicos

**Qué es:** Interfaz visual completa que consume la API REST, implementada como Blazor WASM en `src/CalSystem.Web/`. Incluye gestión de técnicos end-to-end.

### Flujo visual del sistema

```
http://localhost:5200  →  Kanban de 3 columnas (Pending / InProgress / Closed)
                          ↓ botón "+ Nueva Orden"
                          Modal crear orden → POST /api/orders
                          ↓ botón "Asignar Técnico" (columna Pending)
                          Modal con dropdown de técnicos → PUT /api/orders/{id}/assign
                          ↓ botón "Cerrar Orden" (columna InProgress)
                          Modal con campo notas → PUT /api/orders/{id}/close

http://localhost:5200/technicians → Gestión de técnicos
                          Formulario nombre + email → POST /api/technicians
                          Tabla con técnicos registrados → GET /api/technicians
```

### Agente frontend-expert

**Archivo:** `.claude/agents/frontend-expert.md`

Experto en Blazor WASM añadido al sistema de agentes. Conoce la estructura de `CalSystem.Web/`, las convenciones de componentes Razor, el contrato de la API y el CSS del proyecto.

**Integración en el orquestador `/consult`:**
- Keywords detectados: `blazor`, `razor`, `componente`, `frontend`, `UI`, `HttpClient`, `modal`, `CSS`, `kanban`
- Flag manual: `@frontend`
- Patrones de archivos: `src/CalSystem.Web/**` → enruta a `frontend-expert` + `api-expert`

### Archivos del frontend

| Archivo | Descripción |
|---------|-------------|
| `src/CalSystem.Web/Pages/Home.razor` | Kanban de órdenes con 3 columnas y estadísticas |
| `src/CalSystem.Web/Pages/Technicians.razor` | CRUD de técnicos (formulario + tabla) |
| `src/CalSystem.Web/Components/OrderCard.razor` | Tarjeta de orden con acciones contextuales |
| `src/CalSystem.Web/Components/CreateOrderModal.razor` | Modal crear nueva orden |
| `src/CalSystem.Web/Components/AssignTechnicianModal.razor` | Modal con dropdown de técnicos reales |
| `src/CalSystem.Web/Components/CloseOrderModal.razor` | Modal cerrar orden con notas |
| `src/CalSystem.Web/Services/OrderApiService.cs` | Servicio HTTP centralizado para todos los endpoints |
| `src/CalSystem.Web/Layouts/MainLayout.razor` | Layout con barra de navegación (Órdenes / Técnicos) |
| `src/CalSystem.Web/wwwroot/css/app.css` | Estilos completos del sistema (dark mode, kanban, modales) |

### Mejoras de calidad aplicadas (change-validator)

Tras ejecutar `/validate-agent` completo sobre todo el código:

| Mejora | Detalle |
|--------|---------|
| `EnsureCreated()` → `Migrate()` | Base de datos ahora usa migraciones formales EF Core |
| Migración `InitialCreate` | Esquema completo versionado en `Infrastructure/Migrations/` |
| `Directory.Build.props` | Supresión auditada de SQLitePCLRaw advisory (sin versión corregida disponible) |
| Paquetes actualizados | OpenApi 10.0.9, Blazor WASM 10.0.9, Test SDK 18.7.0, xunit 3.1.5, coverlet 10.0.1 |
| Comentario `GetByIdAsync` | Documentado por qué `FindAsync` sin `AsNoTracking` es intencional |
| `UnitTest1.cs` eliminado | Placeholder sin aserciones removido — 11 tests reales |
