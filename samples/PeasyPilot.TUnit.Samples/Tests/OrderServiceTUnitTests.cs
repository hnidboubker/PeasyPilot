namespace PeasyPilot.TUnit.Samples.Tests;

using global::TUnit;
using PeasyPilot.Core;
using PeasyPilot.Core.Extensions;
using PeasyPilot.TUnit;
using PeasyPilot.TUnit.Samples.Enums;
using PeasyPilot.TUnit.Samples.Services;



/// <summary>
/// Sample TUnit tests demonstrating PeasyPilot usage.
/// </summary>
public class OrderServiceTUnitTests : PeasyPilotTUnitTestBase
{
    private OrderService _service = null!;

    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _service = new OrderService();
    }

    [Test]
    public async Task CreateOrder_WithValidCustomerId_CreatesOrder()
    {
        // Arrange & Act
        var order = _service.CreateOrder("CUST001");

        // Assert
         Assert.That(order).IsNotNull();
         Assert.That(order.CustomerId).IsEqualTo("CUST001");
         Assert.That(order.Status).IsEqualTo(OrderStatus.Pending);
        await Task.CompletedTask;
    }

    [Test]
    public async Task AddItem_ToOrder_IncreasesItemCount()
    {
        // Arrange
        var order = _service.CreateOrder("CUST002");

        // Act
        _service.AddItem(order.Id, 1001, 2, 50m);
        _service.AddItem(order.Id, 1002, 3, 30m);

        // Assert
        var updated = _service.GetOrder(order.Id);
        Assert.That(updated!.Items.Count).IsEqualTo(2);
        await Task.CompletedTask;
    }

    [Test]
    public async Task GetOrderTotal_WithMultipleItems_CalculatesCorrectly()
    {
        // Arrange
        var order = _service.CreateOrder("CUST003");
        _service.AddItem(order.Id, 1, 2, 100m); // 200
        _service.AddItem(order.Id, 2, 3, 50m);  // 150
        // Total should be 350

        // Act
        var total = _service.GetOrderTotal(order.Id);

        // Assert
        Assert.That(total).IsEqualTo(350m);
        await Task.CompletedTask;
    }

    [Test]
    public async Task UpdateStatus_WithValidStatus_UpdatesSuccessfully()
    {
        // Arrange
        var order = _service.CreateOrder("CUST004");

        // Act
        var updated = _service.UpdateStatus(order.Id, OrderStatus.Confirmed);

        // Assert
        Assert.That(updated).IsEqualTo(true);
        var result = _service.GetOrder(order.Id);
        Assert.That(result!.Status).IsEqualTo(OrderStatus.Confirmed);
        await Task.CompletedTask;
    }

    [Test]
    [Arguments("CUST005")]
    [Arguments("CUST006")]
    [Arguments("CUST007")]
    public async Task CreateOrder_WithDifferentCustomers_AllSucceed(string customerId)
    {
        // Arrange & Act
        var order = _service.CreateOrder(customerId);

        // Assert
        Assert.That(order.CustomerId).IsEqualTo(customerId);
        await Task.CompletedTask;
    }

    [Test]
    public async Task GetOrder_WithInvalidId_ReturnsNull()
    {
        // Act
        var order = _service.GetOrder(999);

        // Assert
        Assert.That(order).IsNull();
        await Task.CompletedTask;
    }

    [Test]
    public async Task OrderWorkflow_CompleteFlow_FollowsExpectedStates()
    {
        // Arrange & Act
        var order = _service.CreateOrder("CUST_WORKFLOW");
        Assert.That(order.Status).IsEqualTo(OrderStatus.Pending);

        _service.UpdateStatus(order.Id, OrderStatus.Confirmed);
        var confirmed = _service.GetOrder(order.Id);
        Assert.That(confirmed!.Status).IsEqualTo(OrderStatus.Confirmed);

        _service.UpdateStatus(order.Id, OrderStatus.Shipped);
        var shipped = _service.GetOrder(order.Id);
        Assert.That(shipped!.Status).IsEqualTo(OrderStatus.Shipped);

        _service.UpdateStatus(order.Id, OrderStatus.Delivered);
        var delivered = _service.GetOrder(order.Id);

        // Assert
        Assert.That(delivered!.Status).IsEqualTo(OrderStatus.Delivered);
        await Task.CompletedTask;
    }
}
