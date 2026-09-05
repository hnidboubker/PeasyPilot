namespace PeasyPilot.Bogus.Tests;

/// <summary>
/// Regression tests for <see cref="TestDataFactory"/>.
/// Tests for issue #3: TestDataFactory.Create<T>() always throws "Failed to generate {Type}"
/// </summary>
public class TestDataFactoryTests
{
    private readonly TestDataFactory _factory = new();

    /// <summary>
    /// Regression test: Create<T>() should generate an instance without throwing.
    /// Previously failed due to reflection signature mismatch on Faker<T>.Generate().
    /// </summary>
    [Fact]
    public void Create_WithSimpleClass_ReturnsNonNull()
    {
        // Act
        var result = _factory.Create<SimpleTestClass>();

        // Assert
        Assert.NotNull(result);
    }

    /// <summary>
    /// Regression test: Create<T>() should generate instances with populated properties.
    /// </summary>
    [Fact]
    public void Create_WithSimpleClass_PopulatesProperties()
    {
        // Act
        var result = _factory.Create<SimpleTestClass>();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Name);
        Assert.True(result.Age > 0);
    }

    /// <summary>
    /// Regression test: CreateMany<T>() should generate multiple instances without throwing.
    /// </summary>
    [Fact]
    public void CreateMany_WithSimpleClass_ReturnsNonEmptyCollection()
    {
        // Act
        var count = 5;
        var result = _factory.CreateMany<SimpleTestClass>(count);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count);
    }

    /// <summary>
    /// Regression test: CreateMany<T>() instances should all be valid.
    /// </summary>
    [Fact]
    public void CreateMany_WithSimpleClass_AllInstancesValid()
    {
        // Act
        var count = 3;
        var result = _factory.CreateMany<SimpleTestClass>(count);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, item =>
        {
            Assert.NotNull(item);
            Assert.NotEmpty(item.Name);
            Assert.True(item.Age > 0);
        });
    }

    /// <summary>
    /// Regression test: Create<T>() with complex nested properties.
    /// </summary>
    [Fact]
    public void Create_WithComplexClass_ReturnsNonNull()
    {
        // Act
        var result = _factory.Create<ComplexTestClass>();

        // Assert
        Assert.NotNull(result);
    }
}

// Test model classes
namespace PeasyPilot.Bogus.Tests;

public class SimpleTestClass
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class ComplexTestClass
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public SimpleTestClass? RelatedItem { get; set; }
    public List<string> Tags { get; set; } = [];
}
