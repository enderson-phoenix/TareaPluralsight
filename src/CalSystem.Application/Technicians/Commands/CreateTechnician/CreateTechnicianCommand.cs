using MediatR;

namespace CalSystem.Application.Technicians.Commands.CreateTechnician;

public record CreateTechnicianCommand(string Name, string Email) : IRequest<Guid>;
