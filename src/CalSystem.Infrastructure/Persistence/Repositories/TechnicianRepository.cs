using CalSystem.Domain.Entities;
using CalSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CalSystem.Infrastructure.Persistence.Repositories;

public class TechnicianRepository : ITechnicianRepository
{
    private readonly AppDbContext _context;

    public TechnicianRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Technician technician)
    {
        await _context.Technicians.AddAsync(technician);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Technician>> GetAllAsync()
    {
        return await _context.Technicians
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}
