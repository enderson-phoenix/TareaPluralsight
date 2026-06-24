using CalSystem.Domain.Interfaces;
using MediatR;

namespace CalSystem.Application.Orders.Commands.CloseOrder;

public class CloseOrderHandler : IRequestHandler<CloseOrderCommand, bool>
{
    private readonly IServiceOrderRepository _repository;

    public CloseOrderHandler(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(CloseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId);
        if (order is null) return false;

        order.Close(request.Notes);
        await _repository.UpdateAsync(order);

        return true;
    }
}
