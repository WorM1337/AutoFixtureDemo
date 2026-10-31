using AutoFixture;
using AutoFixture.Kernel;
using PPOHomeTask.Models;
using Xunit;

namespace PPOHomeTask.Tests;

public class FixtureTests
{
    [Fact(DisplayName = "Fixture создаёт заказ с вложенными данными")]
    public void Fixture__OrderRequest__CreatesOrderWithNestedData()
    {
        // Arrange
        var fixture = new Fixture();

        // Action
        var order = fixture.Create<Order>();

        // Assert
        Assert.NotNull(order);
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.NotNull(order.Customer);
        Assert.NotEmpty(order.Customer.Name);
        Assert.NotNull(order.Items);
        Assert.NotEmpty(order.Items);
    }

    [Fact(DisplayName = "Кастомизация создаёт оплаченный заказ с положительной суммой")]
    public void OrderCustomization__OrderRequest__CreatesPaidOrderWithValidData()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customize(new OrderCustomization());

        // Action
        var order = fixture.Create<Order>();

        // Assert
        Assert.True(order.IsPaid);
        Assert.Equal("AutoFixture Customer", order.Customer.Name);
        Assert.NotEmpty(order.Items);
        Assert.All(order.Items, item =>
        {
            Assert.True(item.Quantity > 0);
            Assert.True(item.UnitPrice > 0);
        });
    }

    [Fact(DisplayName = "Пользовательский builder обрабатывает заказ и передаёт остальные типы Fixture")]
    public void CustomOrderBuilder__OrderAndOtherTypeRequests__CreatesCustomOrderAndUsesFixture()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customizations.Add(new CustomOrderBuilder());

        // Action
        var order = fixture.Create<Order>();
        var customer = fixture.Create<Customer>();

        // Assert
        Assert.Equal(CustomOrderBuilder.OrderId, order.Id);
        Assert.Equal("Order created by CustomOrderBuilder", order.Customer.Name);
        Assert.NotEmpty(order.Items);
        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.NotEmpty(customer.Name);
    }

    [Fact(DisplayName = "Build настраивает один экземпляр и не меняет Fixture")]
    public void Fixture__BuildConfiguration__CreatesLocalCustomizationWithoutGlobalChanges()
    {
        // Arrange
        var fixture = new Fixture();

        // Action
        var order = fixture
            .Build<Order>()
            .With(x => x.IsPaid, true)
            .Without(x => x.Items)
            .Create();

        var anotherOrder = fixture.Create<Order>();

        // Assert
        Assert.True(order.IsPaid);
        Assert.Empty(order.Items);
        Assert.NotEmpty(anotherOrder.Items);
    }
}

public sealed class OrderCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Customer>(composer =>
            composer.With(x => x.Name, "AutoFixture Customer"));

        fixture.Customize<OrderItem>(composer =>
            composer
                .With(x => x.Quantity, 1)
                .With(x => x.UnitPrice, 10m));

        fixture.Customize<Order>(composer =>
            composer
                .With(x => x.IsPaid, true)
                .With(x => x.Items, fixture.CreateMany<OrderItem>(1).ToList()));
    }
}

public sealed class CustomOrderBuilder : ISpecimenBuilder
{
    public static readonly Guid OrderId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public object Create(object request, ISpecimenContext context)
    {
        if (request is not Type type || type != typeof(Order))
        {
            return NoSpecimen.Instance;
        }

        var customer = (Customer)context.Resolve(typeof(Customer));
        customer.Name = "Order created by CustomOrderBuilder";

        var item = (OrderItem)context.Resolve(typeof(OrderItem));

        return new Order
        {
            Id = OrderId,
            Customer = customer,
            Items = new List<OrderItem> { item },
            IsPaid = true
        };
    }
}
