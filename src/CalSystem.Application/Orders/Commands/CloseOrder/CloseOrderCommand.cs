using MediatR;

namespace CalSystem.Application.Orders.Commands.CloseOrder;

public record CloseOrderCommand(
    Guid OrderId,
    string? Notes
) : IRequest<bool>;
