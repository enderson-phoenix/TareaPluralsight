using CalSystem.Domain.Entities;
using CalSystem.Domain.Enums;
using CalSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CalSystem.Infrastructure.Persistence.Repositories;

public class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly AppDbContext _context;

    public ServiceOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ServiceOrder order)
    {
        await _context.ServiceOrders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id)
    {
        return await _context.ServiceOrders.FindAsync(id);
    }

    public async Task<IEnumerable<ServiceOrder>> GetByStatusAsync(OrderStatus status)
    {
        return await _context.ServiceOrders
            .AsNoTracking()
            .Where(o => o.Status == status)
            .ToListAsync();
    }

    public async Task UpdateAsync(ServiceOrder order)
    {
        _context.ServiceOrders.Update(order);
        await _context.SaveChangesAsync();
    }
}
