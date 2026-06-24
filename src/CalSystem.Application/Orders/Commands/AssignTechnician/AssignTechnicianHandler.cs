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
