namespace PeasyPilot.XUnit.Samples.StepDefinitions;

using PeasyPilot.BDD.StepDefinitions;

/// <summary>
/// Step definitions for order processing scenarios.
/// </summary>
public class OrderSteps : BddStepDefinition
{
    private class Order
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
        public string Status { get; set; } = "Pending";
        public decimal Total { get; set; }
    }

    private class OrderItem
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }

    private List<Order> _orders = new();
    private Order? _currentOrder;

    [Given("the order database is empty")]
    public void OrderDatabaseEmpty()
    {
        _orders.Clear();
    }

    [When("I create an order for customer {customerId}")]
    public void CreateOrder(string customerId)
    {
        var order = new Order
        {
            Id = _orders.Count + 1,
            CustomerId = customerId,
            Status = "Pending"
        };
        _orders.Add(order);
        _currentOrder = order;
    }

    [Given("an order for customer {customerId}")]
    public void OrderExists(string customerId)
    {
        CreateOrder(customerId);
    }

    [Then("the order should be created")]
    public bool OrderCreated()
    {
        return _currentOrder != null && _orders.Contains(_currentOrder);
    }

    [Then("the order status should be {status}")]
    public bool OrderStatusIs(string status)
    {
        return _currentOrder?.Status == status;
    }

    [When("I add {quantity:int} units? of product {productId} at ${unitPrice:decimal} each")]
    public void AddItemToOrder(int quantity, string productId, decimal unitPrice)
    {
        if (_currentOrder != null)
        {
            _currentOrder.Items.Add(new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            });
            CalculateOrderTotal();
        }
    }

    [Then("the order should have {itemCount:int} items?")]
    public bool OrderItemCountIs(int itemCount)
    {
        return _currentOrder?.Items.Count == itemCount;
    }

    [Then("the order total should be ${total:decimal}")]
    public bool OrderTotalIs(decimal total)
    {
        return _currentOrder?.Total == total;
    }

    [Given("an order for customer {customerId} with status {status}")]
    public void OrderWithStatus(string customerId, string status)
    {
        CreateOrder(customerId);
        if (_currentOrder != null)
        {
            _currentOrder.Status = status;
        }
    }

    [When("I update the order status to {status}")]
    public void UpdateOrderStatus(string status)
    {
        if (_currentOrder != null)
        {
            _currentOrder.Status = status;
        }
    }

    [Given("an order for customer {customerId} with items")]
    public void OrderWithItems(string customerId)
    {
        CreateOrder(customerId);
        if (_currentOrder != null)
        {
            _currentOrder.Items.Add(new OrderItem
            {
                ProductId = "Widget",
                Quantity = 1,
                UnitPrice = 10.00m
            });
            CalculateOrderTotal();
        }
    }

    [When("I cancel the order")]
    public void CancelOrder()
    {
        if (_currentOrder != null)
        {
            _currentOrder.Status = "Cancelled";
        }
    }

    [Then("the order should not be active")]
    public bool OrderNotActive()
    {
        return _currentOrder?.Status == "Cancelled";
    }

    private void CalculateOrderTotal()
    {
        if (_currentOrder != null)
        {
            _currentOrder.Total = _currentOrder.Items.Sum(item => item.Total);
        }
    }
}
