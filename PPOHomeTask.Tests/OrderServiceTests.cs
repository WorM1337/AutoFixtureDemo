using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using PPOHomeTask.Models;
using PPOHomeTask.Services;
using Xunit;

namespace PPOHomeTask.Tests;

public class OrderServiceTests
{
    [Fact(DisplayName = "Сервис уведомляет об оплаченном заказе")]
    public async Task OrderService__PaidOrder__ReturnsTrueAndNotifies()
    {
        // Arrange
        var fixture = CreateMockingFixture();
        var repository = fixture.Freeze<Mock<IOrderRepository>>();
        var notificationService = fixture.Freeze<Mock<INotificationService>>();

        var orderId = fixture.Create<Guid>();
        var customer = fixture.Create<Customer>();
        var item = fixture.Create<OrderItem>();
        var order = fixture
            .Build<Order>()
            .With(x => x.Id, orderId)
            .With(x => x.Customer, customer)
            .With(x => x.Items, [item])
            .With(x => x.IsPaid, true)
            .Create();

        repository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        var service = fixture.Create<OrderService>();

        // Action
        var result = await service.NotifyIfPaidAsync(orderId);

        // Assert
        Assert.True(result);
        notificationService.Verify(x => x.NotifyAsync(order), Times.Once);
    }

    [Fact(DisplayName = "Сервис не уведомляет о неоплаченном заказе")]
    public async Task OrderService__UnpaidOrder__ReturnsFalseAndDoesNotNotify()
    {
        // Arrange
        var fixture = CreateMockingFixture();
        var repository = fixture.Freeze<Mock<IOrderRepository>>();
        var notificationService = fixture.Freeze<Mock<INotificationService>>();
        var order = fixture.Build<Order>().With(x => x.IsPaid, false).Create();

        repository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = fixture.Create<OrderService>();

        // Action
        var result = await service.NotifyIfPaidAsync(order.Id);

        // Assert
        Assert.False(result);
        notificationService.Verify(x => x.NotifyAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact(DisplayName = "Сервис не уведомляет, если заказ не найден")]
    public async Task OrderService__MissingOrder__ReturnsFalseAndDoesNotNotify()
    {
        // Arrange
        var fixture = CreateMockingFixture();
        var repository = fixture.Freeze<Mock<IOrderRepository>>();
        var notificationService = fixture.Freeze<Mock<INotificationService>>();
        var orderId = fixture.Create<Guid>();

        repository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        var service = fixture.Create<OrderService>();

        // Action
        var result = await service.NotifyIfPaidAsync(orderId);

        // Assert
        Assert.False(result);
        notificationService.Verify(x => x.NotifyAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact(DisplayName = "Fixture создаёт сервис с mock-зависимостями")]
    public void OrderService__FixtureWithMockDependencies__CreatesService()
    {
        // Arrange
        var fixture = CreateMockingFixture();

        var repository = fixture.Freeze<Mock<IOrderRepository>>();
        var notificationService = fixture.Freeze<Mock<INotificationService>>();

        // Action
        var service = fixture.Create<OrderService>();

        // Assert
        Assert.NotNull(service);
        Assert.NotNull(repository.Object);
        Assert.NotNull(notificationService.Object);
    }

    private static Fixture CreateMockingFixture()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());
        return fixture;
    }
}
