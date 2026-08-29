namespace PeasyPilot.TUnit.Samples;

using TUnitCore = global::TUnit.Core;
using PeasyPilot.Core.Assertions;
using PeasyPilot.TUnit;

/// <summary>
/// Sample domain model for testing.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Order item details.
/// </summary>
public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal Total => Quantity * UnitPrice;
}

/// <summary>
/// Order status enumeration.
/// </summary>
public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// Sample order service.
/// </summary>
public class OrderService
{
    private readonly Dictionary<int, Order> _orders = new();

    public Order CreateOrder(string customerId)
    {
        var order = new Order
        {
            Id = _orders.Count + 1,
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _orders[order.Id] = order;
        return order;
    }

    public void AddItem(int orderId, int productId, int quantity, decimal unitPrice)
    {
        if (_orders.ContainsKey(orderId))
        {
            _orders[orderId].Items.Add(new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            });
        }
    }

    public decimal GetOrderTotal(int orderId)
    {
        if (!_orders.ContainsKey(orderId)) return 0;
        return _orders[orderId].Items.Sum(i => i.Total);
    }

    public bool UpdateStatus(int orderId, OrderStatus status)
    {
        if (!_orders.ContainsKey(orderId)) return false;
        _orders[orderId].Status = status;
        return true;
    }

    public Order? GetOrder(int id) => _orders.ContainsKey(id) ? _orders[id] : null;
}

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

    [TUnitCore.Test]
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

    [TUnitCore.Test]
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

    [TUnitCore.Test]
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

    [TUnitCore.Test]
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

    [TUnitCore.Test]
    [TUnitCore.Arguments("CUST005")]
    [TUnitCore.Arguments("CUST006")]
    [TUnitCore.Arguments("CUST007")]
    public async Task CreateOrder_WithDifferentCustomers_AllSucceed(string customerId)
    {
        // Arrange & Act
        var order = _service.CreateOrder(customerId);

        // Assert
        Assert.That(order.CustomerId).IsEqualTo(customerId);
        await Task.CompletedTask;
    }

    [TUnitCore.Test]
    public async Task GetOrder_WithInvalidId_ReturnsNull()
    {
        // Act
        var order = _service.GetOrder(999);

        // Assert
        Assert.That(order).IsNull();
        await Task.CompletedTask;
    }

    [TUnitCore.Test]
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
