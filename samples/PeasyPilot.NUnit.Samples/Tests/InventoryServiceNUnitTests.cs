namespace PeasyPilot.NUnit.Samples;

using global::NUnit.Framework;
using PeasyPilot.NUnit;
using PeasyPilot.NUnit.Extensions;
using PeasyPilot.NUnit.Samples.Models;
using PeasyPilot.NUnit.Samples.Services;

/// <summary>
/// Sample NUnit tests demonstrating PeasyPilot usage.
/// </summary>
[TestFixture]
public class InventoryServiceNUnitTests : PeasyPilotNUnitTestBase
{
    private InventoryService _service = null!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _service = new InventoryService();
    }

    [Test]
    public void AddProduct_WithValidData_AddsProductSuccessfully()
    {
        // Arrange
        var product = new Product { Name = "Laptop", Price = 1200m, Quantity = 5 };

        // Act
        var result = _service.AddProduct(product);

        // Assert
        NAssert.That(result, Is.Not.Null);
        NAssert.That(result.Id, Is.EqualTo(1));
        NAssert.That(result.Name, Is.EqualTo("Laptop"));
        NAssert.That(result.Price, Is.EqualTo(1200m));
    }

    [Test]
    public void GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        var product = new Product { Name = "Mouse", Price = 25m, Quantity = 100 };
        _service.AddProduct(product);

        // Act
        var result = _service.GetProduct(1);

        // Assert
        NAssert.That(result, Is.Not.Null);
        NAssert.That(result!.Name, Is.EqualTo("Mouse"));
        NAssert.That(result.Price, Is.EqualTo(25m));
    }

    [Test]
    public void GetProduct_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = _service.GetProduct(999);

        // Assert
        NAssert.That(result, Is.Null);
    }

    [Test]
    public void UpdateQuantity_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        var product = new Product { Name = "Keyboard", Price = 75m, Quantity = 10 };
        _service.AddProduct(product);

        // Act
        var updated = _service.UpdateQuantity(1, 20);

        // Assert
        NAssert.That(updated, Is.True);
        var result = _service.GetProduct(1);
        NAssert.That(result!.Quantity, Is.EqualTo(20));
    }

    [Test]
    public void CalculateInventoryValue_WithMultipleProducts_ReturnsCorrectTotal()
    {
        // Arrange
        _service.AddProduct(new Product { Name = "Item1", Price = 100m, Quantity = 2 });
        _service.AddProduct(new Product { Name = "Item2", Price = 50m, Quantity = 4 });
        _service.AddProduct(new Product { Name = "Item3", Price = 25m, Quantity = 8 });

        // Act
        var value = _service.CalculateInventoryValue();

        // Assert - (100*2) + (50*4) + (25*8) = 200 + 200 + 200 = 600
        NAssert.That(value, Is.EqualTo(600m));
    }

    [Test]
    [TestCase(1, 5)]
    [TestCase(10, 20)]
    [TestCase(50, 100)]
    public void UpdateQuantity_WithDifferentValues_AllSucceed(int initialQty, int newQty)
    {
        // Arrange
        var product = new Product { Name = "Test Item", Price = 10m, Quantity = initialQty };
        _service.AddProduct(product);

        // Act
        var updated = _service.UpdateQuantity(1, newQty);

        // Assert
        NAssert.That(updated, Is.True);
        var result = _service.GetProduct(1);
        NAssert.That(result!.Quantity, Is.EqualTo(newQty));
    }

    [Test]
    public void RemoveProduct_WithValidId_RemovesSuccessfully()
    {
        // Arrange
        var product = new Product { Name = "To Remove", Price = 10m, Quantity = 1 };
        _service.AddProduct(product);

        // Act
        var removed = _service.RemoveProduct(1);

        // Assert
        NAssert.That(removed, Is.True);
        NAssert.That(_service.GetProduct(1), Is.Null);
    }
}
