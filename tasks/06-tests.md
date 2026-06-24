# Fase 6 — Pruebas (xUnit + TDD)

**Prerequisito:** Fases 2 y 3 completadas (entidades y handlers listos).  
**Resultado esperado:** Tests unitarios para los 3 handlers pasando. Ciclo TDD documentado para UC-01.

> **Importante para T-03:** El ciclo TDD de UC-01 debe quedar documentado en `EVIDENCE.md`.
> La secuencia es: escribir test → ver que falla → implementar → ver que pasa.

---

## 6.1 Preparar el proyecto de tests

Verifica que `CalSystem.Tests.csproj` tiene las referencias necesarias:

```bash
dotnet add tests/CalSystem.Tests reference src/CalSystem.Application
dotnet add tests/CalSystem.Tests reference src/CalSystem.Domain
dotnet add tests/CalSystem.Tests package Moq
dotnet add tests/CalSystem.Tests package FluentAssertions
```

Crea la estructura de carpetas:
```
tests/CalSystem.Tests/
  Orders/
    Commands/
      CreateOrderHandlerTests.cs
      AssignTechnicianHandlerTests.cs
    Queries/
      GetOrdersByStatusHandlerTests.cs
```

---

## 6.2 Ciclo TDD — UC-01: Crear Orden (documenta esto en EVIDENCE.md)

### Paso 1: Escribir el test ANTES de implementar el handler

**Archivo:** `tests/CalSystem.Tests/Orders/Commands/CreateOrderHandlerTests.cs`

```csharp
using CalSystem.Application.Orders.Commands.CreateOrder;
using CalSystem.Domain.Entities;
using CalSystem.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CalSystem.Tests.Orders.Commands;

public class CreateOrderHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _repositoryMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _repositoryMock = new Mock<IServiceOrderRepository>();
        _handler = new CreateOrderHandler(_repositoryMock.Object);
    }

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

    [Fact]
    public async Task Handle_ValidCommand_CreatesOrderWithPendingStatus()
    {
        // Arrange
        ServiceOrder? capturedOrder = null;
        var command = new CreateOrderCommand("Ana García", "Termómetro PT-100", "Lectura errática");

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ServiceOrder>()))
            .Callback<ServiceOrder>(order => capturedOrder = order)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Status.Should().Be(CalSystem.Domain.Enums.OrderStatus.Pending);
        capturedOrder.CustomerName.Should().Be("Ana García");
    }
}
```

### Paso 2: Ejecutar el test — debe FALLAR (rojo)

```bash
dotnet test tests/CalSystem.Tests --filter "CreateOrderHandlerTests"
```

Si el handler no existe todavía, el test ni compilará. **Esto es correcto.** Documenta este momento en `EVIDENCE.md`.

### Paso 3: Implementar el handler mínimo

Crea `CreateOrderHandler.cs` como se describe en [`03-application.md`](03-application.md).

### Paso 4: Ejecutar el test — debe PASAR (verde)

```bash
dotnet test tests/CalSystem.Tests --filter "CreateOrderHandlerTests"
```

Documenta la salida con el resultado en verde en `EVIDENCE.md`.

---

## 6.3 Test para UC-02: Asignar Técnico

**Archivo:** `tests/CalSystem.Tests/Orders/Commands/AssignTechnicianHandlerTests.cs`

```csharp
using CalSystem.Application.Orders.Commands.AssignTechnician;
using CalSystem.Domain.Entities;
using CalSystem.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CalSystem.Tests.Orders.Commands;

public class AssignTechnicianHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _repositoryMock;
    private readonly AssignTechnicianHandler _handler;

    public AssignTechnicianHandlerTests()
    {
        _repositoryMock = new Mock<IServiceOrderRepository>();
        _handler = new AssignTechnicianHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingOrder_ReturnsTrueAndUpdatesStatus()
    {
        // Arrange
        var order = ServiceOrder.Create("Cliente", "Equipo", "Problema");
        var technicianId = Guid.NewGuid();
        var command = new AssignTechnicianCommand(order.Id, technicianId);

        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ServiceOrder>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.TechnicianId.Should().Be(technicianId);
        order.Status.Should().Be(CalSystem.Domain.Enums.OrderStatus.InProgress);
    }

    [Fact]
    public async Task Handle_NonExistingOrder_ReturnsFalse()
    {
        // Arrange
        var command = new AssignTechnicianCommand(Guid.NewGuid(), Guid.NewGuid());
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ServiceOrder?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
```

---

## 6.4 Test para UC-03: Consultar por Estado

**Archivo:** `tests/CalSystem.Tests/Orders/Queries/GetOrdersByStatusHandlerTests.cs`

```csharp
using CalSystem.Application.Orders.Queries.GetOrdersByStatus;
using CalSystem.Domain.Entities;
using CalSystem.Domain.Enums;
using CalSystem.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CalSystem.Tests.Orders.Queries;

public class GetOrdersByStatusHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _repositoryMock;
    private readonly GetOrdersByStatusHandler _handler;

    public GetOrdersByStatusHandlerTests()
    {
        _repositoryMock = new Mock<IServiceOrderRepository>();
        _handler = new GetOrdersByStatusHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsOrdersMappedToDtos()
    {
        // Arrange
        var orders = new List<ServiceOrder>
        {
            ServiceOrder.Create("Cliente A", "Equipo 1", "Problema 1"),
            ServiceOrder.Create("Cliente B", "Equipo 2", "Problema 2")
        };

        _repositoryMock
            .Setup(r => r.GetByStatusAsync(OrderStatus.Pending))
            .ReturnsAsync(orders);

        var query = new GetOrdersByStatusQuery(OrderStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().CustomerName.Should().Be("Cliente A");
        result.All(o => o.Status == "Pending").Should().BeTrue();
    }
}
```

---

## 6.5 Ejecutar todos los tests

```bash
dotnet test
```

Resultado esperado:
```
Passed! - Failed: 0, Passed: 5, Skipped: 0, Total: 5
```

---

## Checklist

- [ ] Ciclo TDD de UC-01 documentado en `EVIDENCE.md` (rojo → verde)
- [ ] `CreateOrderHandlerTests` con al menos 2 tests pasando
- [ ] `AssignTechnicianHandlerTests` con al menos 2 tests pasando
- [ ] `GetOrdersByStatusHandlerTests` con al menos 1 test pasando
- [ ] `dotnet test` retorna 0 errores

**Siguiente:** [`07-claude-topics.md`](07-claude-topics.md)
