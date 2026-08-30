using PeasyPilot.NUnit.Samples.Models;

namespace PeasyPilot.NUnit.Samples.Services;

/// <summary>
/// Sample inventory service.
/// </summary>
public class InventoryService
{
    private readonly Dictionary<int, Product> _products = new();

    public Product AddProduct(Product product)
    {
        product.Id = _products.Count + 1;
        _products[product.Id] = product;
        return product;
    }

    public Product? GetProduct(int id) => _products.ContainsKey(id) ? _products[id] : null;

    public bool UpdateQuantity(int id, int quantity)
    {
        if (!_products.ContainsKey(id)) return false;
        _products[id].Quantity = quantity;
        return true;
    }

    public decimal CalculateInventoryValue()
    {
        return _products.Values.Sum(p => p.Price * p.Quantity);
    }

    public bool RemoveProduct(int id) => _products.Remove(id);
}
