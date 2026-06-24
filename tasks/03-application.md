# Fase 3 — Capa de Aplicación (Casos de Uso)

**Prerequisito:** Fase 2 completada.  
**Resultado esperado:** Los 3 casos de uso implementados como Commands/Queries con sus handlers usando MediatR.

> **CQRS con MediatR:** cada operación es un objeto (Command o Query) que tiene un Handler.
> La API solo envía el objeto a MediatR (`_mediator.Send(command)`) y él se encarga del resto.

---

## 3.1 Configurar MediatR en el proyecto Application

**Archivo:** `src/CalSystem.Application/DependencyInjection.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CalSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
```

Esto se llamará desde `Program.cs` de la Api: `builder.Services.AddApplication();`

---

## 3.2 UC-01 — Crear Orden de Servicio

### Command
**Archivo:** `src/CalSystem.Application/Orders/Commands/CreateOrder/CreateOrderCommand.cs`

```csharp
using MediatR;

namespace CalSystem.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string CustomerName,
    string Equipment,
    string ProblemDescription
) : IRequest<Guid>;
```

> `record` es ideal para commands: inmutable por defecto y con igualdad por valor.

### Handler
**Archivo:** `src/CalSystem.Application/Orders/Commands/CreateOrder/CreateOrderHandler.cs`

```csharp
using CalSystem.Domain.Entities;
using CalSystem.Domain.Interfaces;
using MediatR;

namespace CalSystem.Application.Orders.Commands.CreateOrder;

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

---

## 3.3 UC-02 — Asignar Técnico a una Orden

### Command
**Archivo:** `src/CalSystem.Application/Orders/Commands/AssignTechnician/AssignTechnicianCommand.cs`

```csharp
using MediatR;

namespace CalSystem.Application.Orders.Commands.AssignTechnician;

public record AssignTechnicianCommand(
    Guid OrderId,
    Guid TechnicianId
) : IRequest<bool>;
```

### Handler
**Archivo:** `src/CalSystem.Application/Orders/Commands/AssignTechnician/AssignTechnicianHandler.cs`

```csharp
using CalSystem.Domain.Interfaces;
using MediatR;

namespace CalSystem.Application.Orders.Commands.AssignTechnician;

public class AssignTechnicianHandler : IRequestHandler<AssignTechnicianCommand, bool>
{
    private readonly IServiceOrderRepository _repository;

    public AssignTechnicianHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(AssignTechnicianCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId);
        if (order is null) return false;

        order.AssignTechnician(request.TechnicianId);
        await _repository.UpdateAsync(order);

        return true;
    }
}
```

---

## 3.4 UC-03 — Consultar Órdenes por Estado

### DTO de respuesta
**Archivo:** `src/CalSystem.Application/Orders/Queries/GetOrdersByStatus/OrderDto.cs`

```csharp
namespace CalSystem.Application.Orders.Queries.GetOrdersByStatus;

public record OrderDto(
    Guid Id,
    string CustomerName,
    string Equipment,
    string ProblemDescription,
    string Status,
    Guid? TechnicianId,
    DateTime CreatedAt
);
```

### Query
**Archivo:** `src/CalSystem.Application/Orders/Queries/GetOrdersByStatus/GetOrdersByStatusQuery.cs`

```csharp
using CalSystem.Domain.Enums;
using MediatR;

namespace CalSystem.Application.Orders.Queries.GetOrdersByStatus;

public record GetOrdersByStatusQuery(OrderStatus Status) : IRequest<IEnumerable<OrderDto>>;
```

### Handler
**Archivo:** `src/CalSystem.Application/Orders/Queries/GetOrdersByStatus/GetOrdersByStatusHandler.cs`

```csharp
using CalSystem.Domain.Interfaces;
using MediatR;

namespace CalSystem.Application.Orders.Queries.GetOrdersByStatus;

public class GetOrdersByStatusHandler : IRequestHandler<GetOrdersByStatusQuery, IEnumerable<OrderDto>>
{
    private readonly IServiceOrderRepository _repository;

    public GetOrdersByStatusHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetByStatusAsync(request.Status);

        return orders.Select(o => new OrderDto(
            o.Id,
            o.CustomerName,
            o.Equipment,
            o.ProblemDescription,
            o.Status.ToString(),
            o.TechnicianId,
            o.CreatedAt
        ));
    }
}
```

---

## 3.5 Estructura final de la capa

```
src/CalSystem.Application/
  Orders/
    Commands/
      CreateOrder/
        CreateOrderCommand.cs
        CreateOrderHandler.cs
      AssignTechnician/
        AssignTechnicianCommand.cs
        AssignTechnicianHandler.cs
    Queries/
      GetOrdersByStatus/
        GetOrdersByStatusQuery.cs
        GetOrdersByStatusHandler.cs
        OrderDto.cs
  DependencyInjection.cs
```

```bash
dotnet build src/CalSystem.Application
```

---

## Checklist

- [ ] `DependencyInjection.cs` con `AddApplication()` creado
- [ ] UC-01: `CreateOrderCommand` + `CreateOrderHandler` implementados
- [ ] UC-02: `AssignTechnicianCommand` + `AssignTechnicianHandler` implementados
- [ ] UC-03: `GetOrdersByStatusQuery` + `GetOrdersByStatusHandler` + `OrderDto` implementados
- [ ] `dotnet build` sin errores

**Siguiente:** [`04-infrastructure.md`](04-infrastructure.md)
