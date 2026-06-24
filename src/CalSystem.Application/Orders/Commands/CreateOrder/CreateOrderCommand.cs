using MediatR;

namespace CalSystem.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string CustomerName,
    string Equipment,
    string ProblemDescription
) : IRequest<Guid>;
