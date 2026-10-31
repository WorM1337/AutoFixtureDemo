using PPOHomeTask.Models;

namespace PPOHomeTask.Services;

public interface INotificationService
{
    Task NotifyAsync(Order order);
}
