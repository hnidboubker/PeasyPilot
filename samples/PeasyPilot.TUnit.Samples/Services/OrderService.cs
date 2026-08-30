namespace PeasyPilot.TUnit.Samples.Services;

using PeasyPilot.TUnit.Samples.Models;
using PeasyPilot.TUnit.Samples.Enums;


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
