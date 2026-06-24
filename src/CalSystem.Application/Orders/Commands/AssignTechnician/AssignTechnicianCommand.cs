using MediatR;

namespace CalSystem.Application.Orders.Commands.AssignTechnician;

public record AssignTechnicianCommand(
    Guid OrderId,
    Guid TechnicianId
) : IRequest<bool>;
