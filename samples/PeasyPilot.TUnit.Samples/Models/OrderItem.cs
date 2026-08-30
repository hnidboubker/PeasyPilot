namespace PeasyPilot.TUnit.Samples.Models;

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
