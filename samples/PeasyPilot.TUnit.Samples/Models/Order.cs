using PeasyPilot.TUnit.Samples.Enums;

namespace PeasyPilot.TUnit.Samples.Models;

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
