namespace PPOHomeTask.Services;

public class OrderService(
    IOrderRepository orderRepository,
    INotificationService notificationService)
{
    public async Task<bool> NotifyIfPaidAsync(Guid orderId)
    {
        var order = await orderRepository.GetByIdAsync(orderId);

        if (order is null || !order.IsPaid)
        {
            return false;
        }

        await notificationService.NotifyAsync(order);
        return true;
    }
}
