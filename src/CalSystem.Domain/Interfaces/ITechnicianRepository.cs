using CalSystem.Domain.Entities;

namespace CalSystem.Domain.Interfaces;

public interface ITechnicianRepository
{
    Task AddAsync(Technician technician);
    Task<IEnumerable<Technician>> GetAllAsync();
}
