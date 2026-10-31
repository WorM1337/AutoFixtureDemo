namespace PPOHomeTask.Models;

public class Order
{
    public Guid Id { get; set; }

    public Customer Customer { get; set; } = new();

    public List<OrderItem> Items { get; set; } = new();

    public bool IsPaid { get; set; }
}
