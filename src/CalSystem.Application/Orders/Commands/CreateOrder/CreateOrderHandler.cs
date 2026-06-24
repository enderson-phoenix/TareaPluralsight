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
