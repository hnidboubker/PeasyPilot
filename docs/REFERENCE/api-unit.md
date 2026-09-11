# PeasyPilot.Unit API Reference

## Overview

`PeasyPilot.Unit` provides builder-oriented utilities and shared fixtures designed specifically for unit testing workflows. It streamlines the creation of test objects, manages common test data, and provides a consistent base for unit test fixtures across xUnit, NUnit, and TUnit frameworks.

**Key Responsibilities:**
- Builder pattern implementation for test object creation
- Unit test fixture base class
- Test data generation and validation helpers
- Builder extension methods
- Fluent API for constructing test scenarios

**Targets:** .NET 8.0, 9.0, 10.0

---

## Main Abstractions

### BuilderBase<T>

Base class for implementing the builder pattern for test object creation.

```csharp
namespace PeasyPilot.Unit.Builders;

/// <summary>
/// Base class for building test objects using the builder pattern.
/// </summary>
/// <typeparam name="T">The type of object being built.</typeparam>
public abstract class BuilderBase<T> where T : class
{
    /// <summary>
    /// Gets or sets the instance being built.
    /// </summary>
    protected T Instance { get; set; } = Activator.CreateInstance<T>()!;

    /// <summary>
    /// Builds and returns the instance.
    /// </summary>
    /// <returns>The built instance.</returns>
    public virtual T Build() => Instance;

    /// <summary>
    /// Resets the builder to a fresh state.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public virtual BuilderBase<T> Reset()
    {
        Instance = Activator.CreateInstance<T>()!;
        return this;
    }
}
```

**Purpose:** Provides a fluent builder pattern for test object creation with reset capability.

**Example: Creating a Custom Builder**
```csharp
using PeasyPilot.Unit.Builders;

public class User
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UserBuilder : BuilderBase<User>
{
    public UserBuilder WithName(string name)
    {
        Instance.Name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        Instance.Email = email;
        return this;
    }

    public UserBuilder WithAge(int age)
    {
        Instance.Age = age;
        return this;
    }

    public UserBuilder AsInactive()
    {
        Instance.IsActive = false;
        return this;
    }

    public new UserBuilder Reset()
    {
        base.Reset();
        return this;
    }
}

// Usage:
[Fact]
public void TestUserBuilder()
{
    var builder = new UserBuilder();
    
    var user1 = builder
        .WithName("John Doe")
        .WithEmail("john@example.com")
        .WithAge(30)
        .Build();

    Assert.Equal("John Doe", user1.Name);

    // Reset and build a different user
    var user2 = builder
        .Reset()
        .WithName("Jane Smith")
        .WithEmail("jane@example.com")
        .Build();

    Assert.Equal("Jane Smith", user2.Name);
}
```

---

## Core Classes

### UnitTestFixture

Base class for unit tests providing common test setup and utilities.

```csharp
namespace PeasyPilot.Unit.Fixtures;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Context;

/// <summary>
/// Base fixture for unit tests providing common test setup and utilities.
/// </summary>
public abstract class UnitTestFixture
{
    /// <summary>
    /// Gets the test context for this fixture.
    /// </summary>
    protected ITestContext TestContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitTestFixture"/> class.
    /// </summary>
    protected UnitTestFixture()
    {
        TestContext = new TestContext();
    }

    /// <summary>
    /// Gets or creates a value in the test context.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value.</param>
    /// <returns>The cached or newly created value.</returns>
    protected T GetOrCreateTestData<T>(string key, Func<T> factory) where T : class
    {
        return TestContext.GetOrAdd(key, factory);
    }
}
```

**Purpose:** Provides thread-safe test context and data sharing across unit test methods.

**Example: Creating a Unit Test Fixture**
```csharp
using PeasyPilot.Unit.Fixtures;
using PeasyPilot.Core.Abstractions;

public class CalculatorTests : UnitTestFixture
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        // Get or create a calculator instance
        var calculator = GetOrCreateTestData("calc", () => new Calculator());

        var result = calculator.Add(2, 3);

        Assert.Equal(5, result);
    }

    [Fact]
    public void Subtract_TwoNumbers_ReturnsDifference()
    {
        // Reuses the same calculator instance
        var calculator = GetOrCreateTestData("calc", () => new Calculator());

        var result = calculator.Subtract(5, 3);

        Assert.Equal(2, result);
    }
}

public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
}
```

---

## Helper Classes

### TestDataHelper

Static helper methods for test data operations.

```csharp
namespace PeasyPilot.Unit.Helpers;

/// <summary>
/// Helper methods for unit testing operations.
/// </summary>
public static class TestDataHelper
{
    /// <summary>
    /// Validates that an object is not null and meets basic criteria.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValidTestObject<T>(T? obj) where T : class
    {
        return obj != null;
    }

    /// <summary>
    /// Creates a shallow copy of the given object using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to copy.</param>
    /// <returns>A shallow copy of the object.</returns>
    public static T ShallowCopy<T>(T obj) where T : class
    {
        var type = obj.GetType();
        if (type.IsValueType)
            return obj;

        var copy = Activator.CreateInstance(type) as T;
        if (copy == null)
            throw new InvalidOperationException($"Failed to create copy of {typeof(T).Name}");

        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                var value = property.GetValue(obj);
                property.SetValue(copy, value);
            }
        }

        return copy;
    }
}
```

**Purpose:** Common utility methods for test data manipulation and validation.

**Example: Using TestDataHelper**
```csharp
using PeasyPilot.Unit.Helpers;

public class TestDataHelperTests
{
    [Fact]
    public void IsValidTestObject_WithValidObject_ReturnsTrue()
    {
        var user = new User { Name = "John" };

        bool isValid = TestDataHelper.IsValidTestObject(user);

        Assert.True(isValid);
    }

    [Fact]
    public void IsValidTestObject_WithNull_ReturnsFalse()
    {
        User? user = null;

        bool isValid = TestDataHelper.IsValidTestObject(user);

        Assert.False(isValid);
    }

    [Fact]
    public void ShallowCopy_CopiesObject_WithoutModifyingOriginal()
    {
        var original = new User { Name = "John", Age = 30 };

        var copy = TestDataHelper.ShallowCopy(original);
        copy.Name = "Jane";

        Assert.Equal("John", original.Name);
        Assert.Equal("Jane", copy.Name);
    }
}

public class User
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
```

---

## Extension Methods

### BuilderExtensions

Extension methods for builder fluency and chainability.

```csharp
namespace PeasyPilot.Unit.Extensions;

/// <summary>
/// Extension methods for builders.
/// </summary>
public static class BuilderExtensions
{
    /// <summary>
    /// Creates a new instance of a builder.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <returns>A new builder instance.</returns>
    public static TBuilder Create<TBuilder>() where TBuilder : class, new()
    {
        return new TBuilder();
    }

    /// <summary>
    /// Chains multiple builder configurations together.
    /// </summary>
    /// <typeparam name="T">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The configured builder.</returns>
    public static T Configure<T>(this T builder, Action<T> configure) where T : class
    {
        configure(builder);
        return builder;
    }
}
```

**Example: Using BuilderExtensions**
```csharp
using PeasyPilot.Unit.Extensions;

public class OrderBuilder : BuilderBase<Order>
{
    public OrderBuilder WithItems(int count)
    {
        Instance.ItemCount = count;
        return this;
    }

    public OrderBuilder WithTotal(decimal amount)
    {
        Instance.Total = amount;
        return this;
    }

    public new OrderBuilder Reset()
    {
        base.Reset();
        return this;
    }
}

public class Order
{
    public int ItemCount { get; set; }
    public decimal Total { get; set; }
}

// Usage with extensions:
[Fact]
public void BuildOrder_WithExtensions()
{
    var builder = BuilderExtensions.Create<OrderBuilder>();
    
    var order = builder
        .Configure(b => b.WithItems(5).WithTotal(99.99m))
        .Build();

    Assert.Equal(5, order.ItemCount);
    Assert.Equal(99.99m, order.Total);
}
```

---

## Common Patterns

### Pattern 1: Builder with Default Values

```csharp
using PeasyPilot.Unit.Builders;

public class ProductBuilder : BuilderBase<Product>
{
    public override Product Build()
    {
        // Set defaults if not provided
        if (Instance.Name == string.Empty)
            Instance.Name = "Default Product";
        
        if (Instance.Price <= 0)
            Instance.Price = 9.99m;

        return base.Build();
    }

    public ProductBuilder WithName(string name)
    {
        Instance.Name = name;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        Instance.Price = price;
        return this;
    }
}

public class Product
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

[Fact]
public void ProductBuilder_AppliesDefaults()
{
    var product = new ProductBuilder().Build();

    Assert.Equal("Default Product", product.Name);
    Assert.Equal(9.99m, product.Price);
}
```

### Pattern 2: Fixture with Shared Setup

```csharp
using PeasyPilot.Unit.Fixtures;

public class UserServiceTests : UnitTestFixture
{
    private IUserService _service;
    private List<User> _testUsers;

    public UserServiceTests()
    {
        SetupSharedData();
    }

    private void SetupSharedData()
    {
        _service = GetOrCreateTestData("service", () =>
            new UserService());

        _testUsers = GetOrCreateTestData("users", () =>
            new List<User>
            {
                new User { Id = 1, Name = "Alice" },
                new User { Id = 2, Name = "Bob" }
            });
    }

    [Fact]
    public void GetUser_ReturnsExistingUser()
    {
        var user = _service.GetUser(1);

        Assert.NotNull(user);
        Assert.Equal("Alice", user.Name);
    }

    [Fact]
    public void CreateUser_AddsNewUser()
    {
        var newUser = new User { Id = 3, Name = "Charlie" };
        _service.AddUser(newUser);

        Assert.Contains(_testUsers, u => u.Name == "Charlie");
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public interface IUserService
{
    User? GetUser(int id);
    void AddUser(User user);
}
```

### Pattern 3: Builder for Complex Objects

```csharp
using PeasyPilot.Unit.Builders;

public class RequestBuilder : BuilderBase<HttpRequest>
{
    public RequestBuilder WithMethod(string method)
    {
        Instance.Method = method;
        return this;
    }

    public RequestBuilder WithPath(string path)
    {
        Instance.Path = path;
        return this;
    }

    public RequestBuilder WithHeader(string key, string value)
    {
        Instance.Headers ??= new Dictionary<string, string>();
        Instance.Headers[key] = value;
        return this;
    }

    public RequestBuilder WithBody(string body)
    {
        Instance.Body = body;
        return this;
    }

    public new RequestBuilder Reset()
    {
        base.Reset();
        Instance.Headers = null;
        return this;
    }
}

public class HttpRequest
{
    public string Method { get; set; } = "GET";
    public string Path { get; set; } = "/";
    public Dictionary<string, string>? Headers { get; set; }
    public string Body { get; set; } = string.Empty;
}

[Fact]
public void RequestBuilder_BuildsComplexRequest()
{
    var request = new RequestBuilder()
        .WithMethod("POST")
        .WithPath("/api/users")
        .WithHeader("Content-Type", "application/json")
        .WithHeader("Authorization", "Bearer token123")
        .WithBody("{\"name\": \"John\"}")
        .Build();

    Assert.Equal("POST", request.Method);
    Assert.Equal("/api/users", request.Path);
    Assert.Equal(2, request.Headers?.Count);
    Assert.NotEmpty(request.Body);
}
```

---

## Configuration

### Dependency Injection Setup

```csharp
using Microsoft.Extensions.DependencyInjection;

public class UnitTestConfiguration
{
    public static IServiceCollection AddUnitTestSupport(
        this IServiceCollection services)
    {
        // No explicit registration needed - fixtures and builders 
        // are used directly without DI container
        return services;
    }
}
```

---

## Reference Summary

| Component | Purpose | Usage |
|-----------|---------|-------|
| BuilderBase<T> | Base class for builder pattern | Inheritance for custom builders |
| UnitTestFixture | Base fixture for unit tests | Inheritance for test classes |
| TestDataHelper | Utility methods for test data | Static method calls |
| BuilderExtensions | Extension methods for builders | Extension method calls |

---

## Quick Reference: Common Builder Pattern

```csharp
// 1. Create a custom builder
public class MyObjectBuilder : BuilderBase<MyObject>
{
    public MyObjectBuilder WithProperty(string value)
    {
        Instance.Property = value;
        return this;
    }
}

// 2. Use fluent API
var obj = new MyObjectBuilder()
    .WithProperty("value1")
    .WithProperty("value2")
    .Build();

// 3. Reset and reuse
var obj2 = new MyObjectBuilder()
    .Reset()
    .WithProperty("different")
    .Build();
```

---

## See Also

- **GETTING-STARTED.md** — Quick start guide
- **unit-testing-guide.md** — Comprehensive unit testing guide
- **api-core.md** — PeasyPilot.Core API
- **api-integration.md** — PeasyPilot.Integration API
