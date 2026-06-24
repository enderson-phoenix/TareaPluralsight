using CalSystem.Domain.Entities;
using CalSystem.Domain.Interfaces;
using MediatR;

namespace CalSystem.Application.Technicians.Commands.CreateTechnician;

public class CreateTechnicianHandler : IRequestHandler<CreateTechnicianCommand, Guid>
{
    private readonly ITechnicianRepository _repository;

    public CreateTechnicianHandler(ITechnicianRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateTechnicianCommand request, CancellationToken cancellationToken)
    {
        var technician = Technician.Create(request.Name, request.Email);
        await _repository.AddAsync(technician);
        return technician.Id;
    }
}
