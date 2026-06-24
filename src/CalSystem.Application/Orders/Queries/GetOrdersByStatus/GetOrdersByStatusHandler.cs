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
