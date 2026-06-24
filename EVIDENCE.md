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

**Resultado:** Plan aprobado e implementado. 7 tests pasando (base UC-01/02/03; 10 en total tras UC-04).

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
