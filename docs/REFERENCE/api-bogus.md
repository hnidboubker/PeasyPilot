# PeasyPilot.Bogus API Reference

## Overview

`PeasyPilot.Bogus` provides a streamlined fake data generation abstraction built on top of the popular Bogus library. It simplifies test data creation by offering a framework-agnostic interface that abstracts away Bogus API complexity and enables seamless integration with unit tests, integration tests, and BDD scenarios.

**Key Responsibilities:**
- Fake data generation abstraction via ITestDataFactory
- Bogus library integration without tight coupling
- Single-instance and collection data generation
- Framework-agnostic test data creation patterns
- Support for all Bogus faker capabilities
- Seamless DI container integration
- Test data customization and seeding

**Targets:** .NET 8.0, 9.0, 10.0

**Dependencies:** Bogus 35.x+ (abstracted)

---

## Main Abstractions

### ITestDataFactory

Core interface for generating fake test data in a framework-agnostic way.

```csharp
namespace PeasyPilot.Core.Abstractions;

/// <summary>
/// Factory for creating fake test data.
/// </summary>
public interface ITestDataFactory
{
    /// <summary>
    /// Creates a single fake instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to generate.</typeparam>
    /// <returns>A fake instance.</returns>
    T Create<T>() where T : class;

    /// <summary>
    /// Creates multiple fake instances of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to generate.</typeparam>
    /// <param name="count">The number of instances to generate.</param>
    /// <returns>A collection of fake instances.</returns>
    IReadOnlyCollection<T> CreateMany<T>(int count) where T : class;
}
```

**Purpose:** Provides a single abstraction point for all fake data generation, enabling swappable implementations (Bogus, AutoFixture, etc.) without changing test code.

---

### TestDataFactory

Concrete implementation using Bogus 35.x+.

```csharp
namespace PeasyPilot.Bogus;

/// <summary>
/// Default test data factory using Bogus for generating random test data.
///
/// Important: Bogus only populates properties that are explicitly configured via RuleFor().
/// Properties without rules remain at their default values (empty string, 0, null, false).
///
/// To populate all properties, configure rules explicitly when creating instances.
/// </summary>
public class TestDataFactory : ITestDataFactory
{
    /// <summary>
    /// Creates a single fake instance of the specified type.
    /// </summary>
    public T Create<T>() where T : class => new Faker<T>().Generate();

    /// <summary>
    /// Creates multiple fake instances of the specified type.
    /// </summary>
    public IReadOnlyCollection<T> CreateMany<T>(int count) where T : class
    {
        var faker = new Faker<T>();
        return Enumerable.Range(0, count).Select(_ => faker.Generate()).ToList();
    }
}
```

**Purpose:** Uses Bogus `Faker<T>` to generate realistic fake data with minimal configuration, perfect for unit and integration tests.

---

## Core Patterns

### Basic Bogus Integration

The TestDataFactory wraps Bogus's `Faker<T>` to provide a simple, stateless interface:

```csharp
// Basic usage - all unconfigured properties get defaults
var faker = new Faker<User>();
var user = faker.Generate();

// Custom configuration - RuleFor() to populate specific properties
var configuredFaker = new Faker<User>()
    .RuleFor(u => u.Name, f => f.Person.FullName())
    .RuleFor(u => u.Email, f => f.Internet.Email())
    .RuleFor(u => u.Age, f => f.Random.Int(18, 65));
var customUser = configuredFaker.Generate();
```

### Faker Localization

Bogus supports multiple locales for realistic data:

```csharp
// Generate French names and addresses
var frFaker = new Faker<User>("fr_FR");

// Generate German data
var deFaker = new Faker<User>("de_DE");

// Generate Japanese data
var jpFaker = new Faker<User>("ja_JP");
```

### Integration with DI Containers

```csharp
public static class TestDataFactoryServiceExtensions
{
    /// <summary>
    /// Registers ITestDataFactory implementation.
    /// </summary>
    public static IServiceCollection AddTestDataFactory(
        this IServiceCollection services)
    {
        services.AddSingleton<ITestDataFactory, TestDataFactory>();
        return services;
    }
}
```

---

## Working Examples

### Example 1: Basic Fake Data Generation

```csharp
using PeasyPilot.Bogus;
using PeasyPilot.Core.Abstractions;

public class BasicFakeDataTest
{
    private readonly ITestDataFactory _factory = new TestDataFactory();

    [Fact]
    public void TestGenerateSingleUser()
    {
        var user = _factory.Create<User>();

        Assert.NotNull(user);
        // Properties without explicit RuleFor will have default values
    }

    [Fact]
    public void TestGenerateMultipleUsers()
    {
        var users = _factory.CreateMany<User>(5);

        Assert.Equal(5, users.Count);
        Assert.All(users, u => Assert.NotNull(u));
    }

    [Fact]
    public void TestGenerateDifferentTypes()
    {
        var user = _factory.Create<User>();
        var product = _factory.Create<Product>();
        var order = _factory.Create<Order>();

        Assert.NotNull(user);
        Assert.NotNull(product);
        Assert.NotNull(order);
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<int> ProductIds { get; set; } = new();
    public decimal Total { get; set; }
}
```

### Example 2: Custom Faker Configuration

```csharp
using Bogus;

public class CustomFakerConfigurationTest
{
    [Fact]
    public void TestConfiguredUserData()
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => f.IndexFaker)
            .RuleFor(u => u.Name, f => f.Person.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Age, f => f.Random.Int(18, 80));

        var user = userFaker.Generate();

        Assert.NotEmpty(user.Name);
        Assert.NotEmpty(user.Email);
        Assert.InRange(user.Age, 18, 80);
    }

    [Fact]
    public void TestConfiguredProductData()
    {
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.IndexFaker)
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(1, 1000)));

        var product = productFaker.Generate();

        Assert.NotEmpty(product.Name);
        Assert.True(product.Price > 0);
    }

    [Fact]
    public void TestConfiguredOrderData()
    {
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker)
            .RuleFor(o => o.UserId, f => f.Random.Int(1, 1000))
            .RuleFor(o => o.ProductIds, f => 
                f.Make(f.Random.Int(1, 5), () => f.Random.Int(1, 500)).ToList())
            .RuleFor(o => o.Total, f => decimal.Parse(f.Commerce.Price(10, 10000)));

        var order = orderFaker.Generate();

        Assert.NotEmpty(order.ProductIds);
        Assert.True(order.Total > 0);
    }
}
```

### Example 3: Faker with Dependency Injection

```csharp
using PeasyPilot.Bogus;
using PeasyPilot.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public class FakerDiTest
{
    [Fact]
    public void TestTestDataFactoryWithDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestDataFactory, TestDataFactory>();
        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<ITestDataFactory>();

        var user = factory.Create<User>();
        var users = factory.CreateMany<User>(3);

        Assert.NotNull(user);
        Assert.Equal(3, users.Count);
    }

    [Fact]
    public void TestInjectTestDataFactoryIntoService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestDataFactory, TestDataFactory>();
        services.AddScoped<UserTestDataService>();
        var serviceProvider = services.BuildServiceProvider();

        var userService = serviceProvider.GetRequiredService<UserTestDataService>();

        var user = userService.GenerateTestUser();

        Assert.NotNull(user);
    }
}

public class UserTestDataService
{
    private readonly ITestDataFactory _factory;

    public UserTestDataService(ITestDataFactory factory)
    {
        _factory = factory;
    }

    public User GenerateTestUser() => _factory.Create<User>();

    public IReadOnlyCollection<User> GenerateTestUsers(int count) 
        => _factory.CreateMany<User>(count);
}
```

### Example 4: Internet Data Generation

```csharp
using Bogus;

public class InternetDataTest
{
    [Fact]
    public void TestInternetFakerData()
    {
        var faker = new Faker();

        var email = faker.Internet.Email();
        var username = faker.Internet.UserName();
        var password = faker.Internet.Password();
        var ipAddress = faker.Internet.IpAddress();
        var url = faker.Internet.Url();
        var domain = faker.Internet.DomainName();

        Assert.NotEmpty(email);
        Assert.Contains("@", email);
        Assert.NotEmpty(username);
        Assert.NotEmpty(password);
        Assert.NotEmpty(ipAddress);
        Assert.NotEmpty(url);
        Assert.Contains(".", domain);
    }

    [Fact]
    public void TestPersonFakerData()
    {
        var faker = new Faker();

        var firstName = faker.Person.FirstName();
        var lastName = faker.Person.LastName();
        var fullName = faker.Person.FullName();
        var email = faker.Person.Email;
        var phone = faker.Person.Phone;

        Assert.NotEmpty(firstName);
        Assert.NotEmpty(lastName);
        Assert.NotEmpty(fullName);
        Assert.NotEmpty(email);
        Assert.NotEmpty(phone);
    }
}
```

### Example 5: Commerce Data Generation

```csharp
using Bogus;

public class CommerceDataTest
{
    [Fact]
    public void TestCommerceData()
    {
        var faker = new Faker();

        var productName = faker.Commerce.ProductName();
        var productDescription = faker.Commerce.ProductDescription();
        var department = faker.Commerce.Department();
        var price = faker.Commerce.Price(10, 500);
        var category = faker.Commerce.Categories(1).First();

        Assert.NotEmpty(productName);
        Assert.NotEmpty(productDescription);
        Assert.NotEmpty(department);
        Assert.NotEmpty(price);
        Assert.NotEmpty(category);
    }

    [Fact]
    public void TestGenerateProductCatalog()
    {
        var faker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(5, 500)));

        var products = Enumerable.Range(0, 10)
            .Select(_ => faker.Generate())
            .ToList();

        Assert.Equal(10, products.Count);
        Assert.All(products, p => 
        {
            Assert.NotEmpty(p.Name);
            Assert.True(p.Price > 0);
        });
    }
}
```

### Example 6: Date and Time Data

```csharp
using Bogus;

public class DateTimeDataTest
{
    [Fact]
    public void TestDateTimeGeneration()
    {
        var faker = new Faker();

        var pastDate = faker.Date.Past();
        var futureDate = faker.Date.Future();
        var recentDate = faker.Date.Recent();
        var birthdayDate = faker.Date.Between(DateTime.Now.AddYears(-80), DateTime.Now.AddYears(-18));

        Assert.True(pastDate < DateTime.Now);
        Assert.True(futureDate > DateTime.Now);
        Assert.True(recentDate >= DateTime.Now.AddDays(-7));
        Assert.InRange(birthdayDate.Year, DateTime.Now.Year - 80, DateTime.Now.Year - 18);
    }

    [Fact]
    public void TestTimeDataGeneration()
    {
        var faker = new Faker();

        var randomTime = faker.Date.Soon().TimeOfDay;
        var randomTimespan = faker.Date.Timespan();

        Assert.NotEqual(default(TimeSpan), randomTime);
        Assert.NotEqual(default(TimeSpan), randomTimespan);
    }
}
```

### Example 7: Seeded Faker for Reproducibility

```csharp
using Bogus;

public class SeededFakerTest
{
    [Fact]
    public void TestSeededFakerReproducibility()
    {
        Randomizer.Seed(12345);
        var faker1 = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email());
        var user1 = faker1.Generate();

        Randomizer.Seed(12345);
        var faker2 = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email());
        var user2 = faker2.Generate();

        // Same seed produces same data
        Assert.Equal(user1.Name, user2.Name);
        Assert.Equal(user1.Email, user2.Email);
    }

    [Fact]
    public void TestDifferentSeedsProduceDifferentData()
    {
        Randomizer.Seed(111);
        var faker1 = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName());
        var user1 = faker1.Generate();

        Randomizer.Seed(222);
        var faker2 = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName());
        var user2 = faker2.Generate();

        // Different seeds produce different data
        Assert.NotEqual(user1.Name, user2.Name);
    }
}
```

### Example 8: Localized Faker

```csharp
using Bogus;

public class LocalizedFakerTest
{
    [Fact]
    public void TestFrenchFakerData()
    {
        var faker = new Faker("fr_FR");

        var firstName = faker.Person.FirstName();
        var lastName = faker.Person.LastName();
        var email = faker.Internet.Email();

        // French names and data
        Assert.NotEmpty(firstName);
        Assert.NotEmpty(lastName);
        Assert.NotEmpty(email);
    }

    [Fact]
    public void TestGermanFakerData()
    {
        var faker = new Faker("de_DE");

        var firstName = faker.Person.FirstName();
        var company = faker.Company.CompanyName();

        // German names and company data
        Assert.NotEmpty(firstName);
        Assert.NotEmpty(company);
    }

    [Fact]
    public void TestJapaneseFakerData()
    {
        var faker = new Faker("ja_JP");

        var firstName = faker.Person.FirstName();
        var lastName = faker.Person.LastName();

        // Japanese names
        Assert.NotEmpty(firstName);
        Assert.NotEmpty(lastName);
    }
}
```

### Example 9: Complex Object Generation

```csharp
using Bogus;

public class ComplexObjectGenerationTest
{
    [Fact]
    public void TestGenerateComplexOrder()
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Person.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email());

        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(1, 1000)));

        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => f.IndexFaker)
            .RuleFor(o => o.UserId, f => f.Random.Int(1, 1000))
            .RuleFor(o => o.ProductIds, f => 
                f.Make(f.Random.Int(1, 5), () => f.Random.Int(1, 500)).ToList())
            .RuleFor(o => o.Total, f => decimal.Parse(f.Commerce.Price(50, 10000)));

        var order = orderFaker.Generate();

        Assert.NotEmpty(order.ProductIds);
        Assert.True(order.Total > 0);
    }
}
```

### Example 10: Integration Test with Test Data Factory

```csharp
using PeasyPilot.Bogus;
using PeasyPilot.Integration;

public class IntegrationTestWithTestDataFactory
{
    [Fact]
    public async Task TestCreateUserWithFakedData()
    {
        var factory = new TestDataFactory();
        
        // Generate test data
        var testUser = factory.Create<User>();
        var moreUsers = factory.CreateMany<User>(5);

        // Use in integration test
        Assert.NotNull(testUser);
        Assert.Equal(5, moreUsers.Count);
    }

    [Fact]
    public async Task TestBulkCreateWithFakeData()
    {
        var factory = new TestDataFactory();
        
        var testUsers = factory.CreateMany<User>(100);

        Assert.Equal(100, testUsers.Count);
        Assert.All(testUsers, u => Assert.NotNull(u));
    }
}
```

---

## Best Practices

1. **Use ITestDataFactory Interface:** Always inject `ITestDataFactory` for flexibility
2. **Configure Rules Explicitly:** Use `RuleFor()` for properties you care about
3. **Keep Defaults Simple:** Rely on Bogus defaults for unconfigured properties
4. **Seed for Reproducibility:** Use `Randomizer.Seed()` when deterministic data is needed
5. **Leverage Locales:** Use different locales for testing international scenarios
6. **Avoid Over-Configuration:** Only configure what your test actually uses

---

## Performance Considerations

- **Object Creation:** Fast (microseconds per object for simple types)
- **Collection Generation:** Linear scaling with count parameter
- **Memory:** Minimal overhead; generates thousands of objects efficiently
- **Seeding:** One-time setup cost; no impact on subsequent generation
- **Localization:** Minimal performance difference between locales

---

## Common Faker Providers

```
faker.Person          # Names, emails, phone numbers, birthdays
faker.Internet        # URLs, usernames, passwords, IP addresses
faker.Commerce        # Products, prices, categories, departments
faker.Company         # Company names, catch phrases, BS statements
faker.Date            # Past/future/recent dates, timespan
faker.Random          # Random numbers, booleans, elements from arrays
faker.Finance         # Account numbers, credit cards, IBAN codes
faker.Address         # Streets, cities, countries, zip codes
faker.Phone           # Phone numbers (various formats)
faker.Email           # Emails, usernames
faker.Lorem           # Lorem ipsum words, sentences, paragraphs
faker.Hacker          # Hacker vocabulary (abbr, adjective, noun)
```

---

## See Also

- [PeasyPilot.Core API](api-core.md)
- [PeasyPilot.Unit API](api-unit.md)
- [Unit Testing Guide](../GUIDES/unit-testing-guide.md)
- [Bogus Documentation](https://github.com/bchavez/Bogus)

