using MediatR;

namespace CalSystem.Application.Technicians.Queries.GetAllTechnicians;

public record GetAllTechniciansQuery : IRequest<IEnumerable<TechnicianDto>>;
