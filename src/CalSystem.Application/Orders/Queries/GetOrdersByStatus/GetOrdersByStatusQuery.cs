using CalSystem.Domain.Enums;
using MediatR;

namespace CalSystem.Application.Orders.Queries.GetOrdersByStatus;

public record GetOrdersByStatusQuery(OrderStatus Status) : IRequest<IEnumerable<OrderDto>>;
