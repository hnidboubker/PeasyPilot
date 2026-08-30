namespace PeasyPilot.NUnit.Samples.Models;

/// <summary>
/// Sample domain model for testing.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
