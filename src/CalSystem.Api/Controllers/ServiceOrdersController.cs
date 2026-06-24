using CalSystem.Application.Orders.Commands.AssignTechnician;
using CalSystem.Application.Orders.Commands.CloseOrder;
using CalSystem.Application.Orders.Commands.CreateOrder;
using CalSystem.Application.Orders.Queries.GetOrdersByStatus;
using CalSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CalSystem.Api.Controllers;

[ApiController]
[Route("api/service-orders")]
public class ServiceOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Creates a new service order.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>Assigns a technician to an existing service order.</summary>
    [HttpPut("{id:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTechnician(Guid id, [FromBody] AssignTechnicianRequest request)
    {
        var command = new AssignTechnicianCommand(id, request.TechnicianId);
        var success = await _mediator.Send(command);

        if (!success) return NotFound($"Order {id} not found.");

        return Ok();
    }

    /// <summary>Closes a service order with optional technician notes.</summary>
    [HttpPut("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseOrder(Guid id, [FromBody] CloseOrderRequest request)
    {
        var command = new CloseOrderCommand(id, request.Notes);
        var success = await _mediator.Send(command);

        if (!success) return NotFound($"Order {id} not found.");

        return Ok();
    }

    /// <summary>Returns all service orders filtered by status.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrdersByStatus([FromQuery] string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var orderStatus))
            return BadRequest($"Invalid status '{status}'. Valid values: Pending, InProgress, Closed.");

        var query = new GetOrdersByStatusQuery(orderStatus);
        var orders = await _mediator.Send(query);

        return Ok(orders);
    }
}

public record CreateOrderRequest(
    string CustomerName,
    string Equipment,
    string ProblemDescription
);

public record AssignTechnicianRequest(Guid TechnicianId);

public record CloseOrderRequest(string? Notes);
