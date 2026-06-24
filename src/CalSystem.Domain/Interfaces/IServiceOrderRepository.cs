using CalSystem.Domain.Entities;
using CalSystem.Domain.Enums;

namespace CalSystem.Domain.Interfaces;

public interface IServiceOrderRepository
{
    Task AddAsync(ServiceOrder order);
    Task<ServiceOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServiceOrder>> GetByStatusAsync(OrderStatus status);
    Task UpdateAsync(ServiceOrder order);
}
