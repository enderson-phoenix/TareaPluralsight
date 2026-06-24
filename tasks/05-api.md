# Fase 5 — Capa de API (Endpoints)

**Prerequisito:** Fases 3 y 4 completadas (handlers y repositorios listos).  
**Resultado esperado:** 3 endpoints HTTP funcionales, probables desde Swagger.

---

## 5.1 Crear el controlador de órdenes

**Archivo:** `src/CalSystem.Api/Controllers/ServiceOrdersController.cs`

```csharp
using CalSystem.Application.Orders.Commands.AssignTechnician;
using CalSystem.Application.Orders.Commands.CreateOrder;
using CalSystem.Application.Orders.Queries.GetOrdersByStatus;
using CalSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CalSystem.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class ServiceOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // UC-01: Crear orden de servicio
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(
            request.CustomerName,
            request.Equipment,
            request.ProblemDescription
        );

        var orderId = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetOrdersByStatus), new { status = "Pending" }, new { id = orderId });
    }

    // UC-02: Asignar técnico a una orden
    [HttpPut("{id:guid}/assign")]
    public async Task<IActionResult> AssignTechnician(Guid id, [FromBody] AssignTechnicianRequest request)
    {
        var command = new AssignTechnicianCommand(id, request.TechnicianId);
        var success = await _mediator.Send(command);

        if (!success) return NotFound($"Order {id} not found.");

        return Ok();
    }

    // UC-03: Consultar órdenes por estado
    [HttpGet]
    public async Task<IActionResult> GetOrdersByStatus([FromQuery] string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var orderStatus))
            return BadRequest($"Invalid status '{status}'. Valid values: Pending, InProgress, Closed.");

        var query = new GetOrdersByStatusQuery(orderStatus);
        var orders = await _mediator.Send(query);

        return Ok(orders);
    }
}
```

---

## 5.2 Crear los request DTOs

Estos objetos reciben los datos del body de las peticiones HTTP.

**Archivo:** `src/CalSystem.Api/Models/CreateOrderRequest.cs`

```csharp
namespace CalSystem.Api.Models;

public record CreateOrderRequest(
    string CustomerName,
    string Equipment,
    string ProblemDescription
);
```

**Archivo:** `src/CalSystem.Api/Models/AssignTechnicianRequest.cs`

```csharp
namespace CalSystem.Api.Models;

public record AssignTechnicianRequest(Guid TechnicianId);
```

> Actualiza los `using` en el controlador si mueves estos a un namespace diferente.

---

## 5.3 Configurar Program.cs

Asegúrate de que `Program.cs` quede así (agrega lo que falte):

```csharp
using CalSystem.Application;
using CalSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
```

---

## 5.4 Probar los endpoints con Swagger

```bash
dotnet run --project src/CalSystem.Api
```

Abre el navegador en `https://localhost:{puerto}/swagger`

**Flujo de prueba manual:**

1. **Crear una orden** — `POST /api/orders`
   ```json
   {
     "customerName": "Juan Pérez",
     "equipment": "Balanza Analítica XR-200",
     "problemDescription": "No enciende después de caída de tensión"
   }
   ```
   Respuesta esperada: `201 Created` con el `id` de la orden.

2. **Consultar órdenes pendientes** — `GET /api/orders?status=Pending`  
   Respuesta esperada: `200 OK` con la lista (debería aparecer la orden recién creada).

3. **Asignar un técnico** — `PUT /api/orders/{id}/assign`
   ```json
   {
     "technicianId": "un-guid-cualquiera"
   }
   ```
   Respuesta esperada: `200 OK`

4. **Consultar órdenes en progreso** — `GET /api/orders?status=InProgress`  
   La orden anterior debería aparecer ahora aquí.

---

## Checklist

- [ ] `ServiceOrdersController` con 3 action methods
- [ ] `POST /api/orders` retorna `201 Created`
- [ ] `PUT /api/orders/{id}/assign` retorna `200 OK` o `404 Not Found`
- [ ] `GET /api/orders?status=...` retorna `200 OK` o `400 Bad Request` si el status no existe
- [ ] Swagger disponible y funcional
- [ ] Los 3 endpoints probados manualmente

**Siguiente:** [`06-tests.md`](06-tests.md)
