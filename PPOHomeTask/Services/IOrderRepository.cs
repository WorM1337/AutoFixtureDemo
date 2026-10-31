using PPOHomeTask.Models;

namespace PPOHomeTask.Services;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
}
