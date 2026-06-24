using CalSystem.Domain.Interfaces;
using MediatR;

namespace CalSystem.Application.Technicians.Queries.GetAllTechnicians;

public class GetAllTechniciansHandler : IRequestHandler<GetAllTechniciansQuery, IEnumerable<TechnicianDto>>
{
    private readonly ITechnicianRepository _repository;

    public GetAllTechniciansHandler(ITechnicianRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TechnicianDto>> Handle(GetAllTechniciansQuery request, CancellationToken cancellationToken)
    {
        var technicians = await _repository.GetAllAsync();
        return technicians.Select(t => new TechnicianDto(t.Id, t.Name, t.Email));
    }
}
