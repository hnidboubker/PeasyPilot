# PeasyPilot.Moq API Reference

## Overview

`PeasyPilot.Moq` provides a unified mock factory abstraction for creating mock objects using the popular Moq mocking library. It simplifies mock creation in unit tests by providing a framework-agnostic interface that abstracts away Moq's API complexity and enables seamless integration with dependency injection and test fixtures.

**Key Responsibilities:**
- Mock object creation abstraction via IMockFactory
- Moq integration without tight coupling
- Type-safe generic mock support
- Seamless DI container integration
- Framework-agnostic mock creation patterns
- Support for all common Moq scenarios (setup, verification, behavior)

**Targets:** .NET 8.0, 9.0, 10.0

**Dependencies:** Moq 4.x (abstracted)

---

## Main Abstractions

### IMockFactory

Core interface for creating mock objects in a framework-agnostic way.

```csharp
namespace PeasyPilot.Core.Abstractions;

/// <summary>
/// Factory for creating mock objects.
/// </summary>
public interface IMockFactory
{
    /// <summary>
    /// Creates a mock instance of the specified type.
    /// </summary>
    /// <param name="type">The type to mock.</param>
    /// <returns>A mock object.</returns>
    object Create(Type type);
}
```

**Purpose:** Provides a single abstraction point for all mock creation, enabling swappable implementations (Moq, NSubstitute, etc.) without changing test code.

---

### MockFactory

Concrete implementation using Moq 4.x.

```csharp
namespace PeasyPilot.Moq;

/// <summary>
/// Factory for creating mock objects using Moq.
/// </summary>
public class MockFactory : IMockFactory
{
    /// <summary>
    /// Creates a mock instance of the specified type.
    /// </summary>
    /// <param name="type">The type to mock.</param>
    /// <returns>A mock object.</returns>
    public object Create(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var mockType = typeof(Mock<>).MakeGenericType(type);
        var mockInstance = Activator.CreateInstance(mockType);
        var objectProperty = mockType.GetProperty("Object");
        return objectProperty!.GetValue(mockInstance)!;
    }
}
```

**Purpose:** Uses reflection to dynamically create `Mock<T>` instances and extract their `.Object` property, enabling type-agnostic mock creation.

---

## Core Patterns

### Mock Creation Strategy

The MockFactory uses a reflection-based strategy to support generic type creation:

1. **Type-Safe Generic Creation:** `Mock<T>` is created via reflection
2. **Object Extraction:** The `.Object` property returns the mockable instance
3. **Casting at Use:** Callers cast to the expected interface/class
4. **Setup and Verification:** Standard Moq patterns apply post-creation

### Integration with DI Containers

```csharp
public static class MockFactoryServiceExtensions
{
    /// <summary>
    /// Registers IMockFactory implementation.
    /// </summary>
    public static IServiceCollection AddMockFactory(
        this IServiceCollection services)
    {
        services.AddSingleton<IMockFactory, MockFactory>();
        return services;
    }
}
```

---

## Working Examples

### Example 1: Basic Mock Creation

```csharp
using PeasyPilot.Moq;
using Moq;

public class BasicMockCreationTest
{
    [Fact]
    public void TestCreateSimpleMock()
    {
        var factory = new MockFactory();

        // Create mock of an interface
        var userService = (IUserService)factory.Create(typeof(IUserService));

        Assert.NotNull(userService);
    }

    [Fact]
    public void TestCreateMockWithMultipleInterfaces()
    {
        var factory = new MockFactory();

        var repository = (IUserRepository)factory.Create(typeof(IUserRepository));
        var logger = (ILogger)factory.Create(typeof(ILogger));

        Assert.NotNull(repository);
        Assert.NotNull(logger);
    }
}

public interface IUserService
{
    Task<User> GetUserAsync(int id);
}

public interface IUserRepository
{
    Task<User?> FindByIdAsync(int id);
}

public interface ILogger
{
    void Log(string message);
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

### Example 2: Mock Setup and Verification

```csharp
using PeasyPilot.Moq;
using Moq;

public class MockSetupAndVerificationTest
{
    [Fact]
    public async Task TestMockSetupAndVerification()
    {
        var factory = new MockFactory();
        
        // Create the mock
        var userServiceMock = factory.Create(typeof(IUserService));
        var userService = (IUserService)userServiceMock;

        // Setup behavior using Moq's extension methods
        // Note: You need to work with the Mock<T> instance for setup
        var moqInstance = new Mock<IUserService>();
        moqInstance
            .Setup(x => x.GetUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new User { Id = 1, Name = "John Doe" });

        var result = await moqInstance.Object.GetUserAsync(1);

        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);

        // Verify the call
        moqInstance.Verify(
            x => x.GetUserAsync(1),
            Times.Once);
    }

    [Fact]
    public void TestMockThrowsException()
    {
        var userServiceMock = new Mock<IUserService>();
        
        userServiceMock
            .Setup(x => x.GetUserAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        var userService = userServiceMock.Object;

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await userService.GetUserAsync(999));
        
        Assert.NotNull(ex);
    }
}
```

### Example 3: Factory with Dependency Injection

```csharp
using PeasyPilot.Moq;
using PeasyPilot.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public class MockFactoryDiTest
{
    [Fact]
    public void TestMockFactoryWithDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMockFactory, MockFactory>();
        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<IMockFactory>();

        var userRepository = (IUserRepository)factory.Create(typeof(IUserRepository));

        Assert.NotNull(userRepository);
    }

    [Fact]
    public void TestInjectMockFactoryIntoService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMockFactory, MockFactory>();
        services.AddScoped<TestableService>();
        var serviceProvider = services.BuildServiceProvider();

        var testableService = serviceProvider.GetRequiredService<TestableService>();

        Assert.NotNull(testableService.UserService);
    }
}

public class TestableService
{
    private readonly IUserService _userService;

    public TestableService(IMockFactory mockFactory)
    {
        _userService = (IUserService)mockFactory.Create(typeof(IUserService));
    }

    public IUserService UserService => _userService;
}
```

### Example 4: Mock Repository Pattern

```csharp
using PeasyPilot.Moq;
using Moq;

public class RepositoryPatternTest
{
    [Fact]
    public async Task TestMockRepository()
    {
        var repositoryMock = new Mock<IUserRepository>();

        var testUsers = new List<User>
        {
            new User { Id = 1, Name = "Alice" },
            new User { Id = 2, Name = "Bob" }
        };

        repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(testUsers);

        repositoryMock
            .Setup(r => r.FindByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => testUsers.FirstOrDefault(u => u.Id == id));

        var repository = repositoryMock.Object;

        var all = await repository.GetAllAsync();
        Assert.Equal(2, all.Count);

        var user = await repository.FindByIdAsync(1);
        Assert.Equal("Alice", user?.Name);

        repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> FindByIdAsync(int id);
    Task SaveAsync(User user);
}
```

### Example 5: Multiple Mocks in Service Test

```csharp
using PeasyPilot.Moq;
using Moq;

public class MultipleLocksServiceTest
{
    [Fact]
    public async Task TestServiceWithMultipleMocks()
    {
        // Setup mocks
        var repositoryMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<ILogger>();
        var emailServiceMock = new Mock<IEmailService>();

        repositoryMock
            .Setup(r => r.FindByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User { Id = 1, Name = "John Doe", Email = "john@example.com" });

        emailServiceMock
            .Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Create service with mocks
        var userService = new UserService(
            repositoryMock.Object,
            loggerMock.Object,
            emailServiceMock.Object);

        // Execute
        var result = await userService.NotifyUserAsync(1, "Welcome!");

        // Verify
        Assert.True(result);
        repositoryMock.Verify(r => r.FindByIdAsync(1), Times.Once);
        loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.AtLeast(1));
        emailServiceMock.Verify(e => e.SendAsync("john@example.com", "Welcome!"), Times.Once);
    }
}

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger _logger;
    private readonly IEmailService _emailService;

    public UserService(IUserRepository repository, ILogger logger, IEmailService emailService)
    {
        _repository = repository;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<bool> NotifyUserAsync(int userId, string message)
    {
        _logger.Log($"Notifying user {userId}");
        
        var user = await _repository.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _emailService.SendAsync(user.Email, message);
        _logger.Log($"Notification {'sent' if result else 'failed'}");
        
        return result;
    }
}

public interface IEmailService
{
    Task<bool> SendAsync(string to, string message);
}
```

### Example 6: Mock with It.IsAny Patterns

```csharp
using PeasyPilot.Moq;
using Moq;

public class ItIsAnyPatternsTest
{
    [Fact]
    public void TestMockWithWildcardMatching()
    {
        var processorMock = new Mock<IDataProcessor>();

        // Setup for any string input
        processorMock
            .Setup(p => p.Process(It.IsAny<string>()))
            .Returns("Processed");

        // Setup for specific range
        processorMock
            .Setup(p => p.Calculate(It.IsInRange<int>(0, 100, Moq.Range.Inclusive)))
            .Returns(true);

        var processor = processorMock.Object;

        Assert.Equal("Processed", processor.Process("anything"));
        Assert.Equal("Processed", processor.Process("foo"));
        Assert.True(processor.Calculate(50));

        processorMock.Verify(p => p.Process(It.IsAny<string>()), Times.Exactly(2));
    }
}

public interface IDataProcessor
{
    string Process(string input);
    bool Calculate(int value);
}
```

### Example 7: Mock Callbacks and Return Values

```csharp
using PeasyPilot.Moq;
using Moq;

public class CallbacksAndReturnTest
{
    [Fact]
    public void TestMockCallbacks()
    {
        var serviceMock = new Mock<IOrderService>();

        var callCount = 0;

        serviceMock
            .Setup(s => s.ProcessOrder(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                callCount++;
                order.Status = "Processing";
            })
            .Returns("OrderProcessed");

        var service = serviceMock.Object;
        var order = new Order { Id = 1 };

        var result = service.ProcessOrder(order);

        Assert.Equal("OrderProcessed", result);
        Assert.Equal(1, callCount);
        Assert.Equal("Processing", order.Status);
    }

    [Fact]
    public void TestMockReturnsSequence()
    {
        var counterMock = new Mock<ICounter>();

        counterMock
            .SetupSequence(c => c.GetNext())
            .Returns(1)
            .Returns(2)
            .Returns(3)
            .Throws(new InvalidOperationException("Exhausted"));

        var counter = counterMock.Object;

        Assert.Equal(1, counter.GetNext());
        Assert.Equal(2, counter.GetNext());
        Assert.Equal(3, counter.GetNext());
        Assert.Throws<InvalidOperationException>(() => counter.GetNext());
    }
}

public class Order
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
}

public interface IOrderService
{
    string ProcessOrder(Order order);
}

public interface ICounter
{
    int GetNext();
}
```

### Example 8: Property Mocking

```csharp
using PeasyPilot.Moq;
using Moq;

public class PropertyMockingTest
{
    [Fact]
    public void TestMockProperties()
    {
        var configMock = new Mock<IConfiguration>();

        configMock
            .Setup(c => c.DatabaseConnectionString)
            .Returns("Server=localhost;Database=TestDb");

        configMock
            .Setup(c => c.LogLevel)
            .Returns("Debug");

        var config = configMock.Object;

        Assert.Equal("Server=localhost;Database=TestDb", config.DatabaseConnectionString);
        Assert.Equal("Debug", config.LogLevel);
    }

    [Fact]
    public void TestPropertySetterTracking()
    {
        var cacheMock = new Mock<ICache>();

        cacheMock.SetupProperty(c => c.Ttl, TimeSpan.FromMinutes(5));

        var cache = cacheMock.Object;

        // Can set property
        cache.Ttl = TimeSpan.FromMinutes(10);

        // Can read property
        Assert.Equal(TimeSpan.FromMinutes(10), cache.Ttl);

        cacheMock.VerifySet(c => c.Ttl = TimeSpan.FromMinutes(10), Times.Once);
    }
}

public interface IConfiguration
{
    string DatabaseConnectionString { get; }
    string LogLevel { get; }
}

public interface ICache
{
    TimeSpan Ttl { get; set; }
}
```

### Example 9: Loose vs Strict Mocking

```csharp
using PeasyPilot.Moq;
using Moq;

public class LooseVsStrictMockingTest
{
    [Fact]
    public void TestLooseMock()
    {
        // Default behavior - returns default values for unmocked calls
        var serviceMock = new Mock<IService>(MockBehavior.Loose);
        
        var service = serviceMock.Object;

        // These calls don't throw, just return defaults
        var result = service.GetValue("unmocked");
        Assert.Null(result);
    }

    [Fact]
    public void TestStrictMock()
    {
        // Strict behavior - throws for unmocked calls
        var serviceMock = new Mock<IService>(MockBehavior.Strict);
        
        serviceMock
            .Setup(s => s.GetValue("valid"))
            .Returns("Value");

        var service = serviceMock.Object;

        Assert.Equal("Value", service.GetValue("valid"));

        // This throws because GetValue("unmocked") was not setup
        Assert.Throws<MockException>(() => service.GetValue("unmocked"));
    }
}

public interface IService
{
    string? GetValue(string key);
}
```

### Example 10: Integration with Test Fixtures

```csharp
using PeasyPilot.Moq;
using PeasyPilot.Unit.Fixtures;

public class FixtureIntegrationTest : UnitTestFixture
{
    private readonly MockFactory _mockFactory = new();

    [Fact]
    public void TestMockInFixture()
    {
        var userService = (IUserService)_mockFactory.Create(typeof(IUserService));

        Assert.NotNull(userService);
    }

    [Fact]
    public void TestMultipleMocksInFixture()
    {
        var repository = (IUserRepository)_mockFactory.Create(typeof(IUserRepository));
        var logger = (ILogger)_mockFactory.Create(typeof(ILogger));

        Assert.NotNull(repository);
        Assert.NotNull(logger);
    }
}
```

---

## Best Practices

1. **Use IMockFactory Interface:** Always inject `IMockFactory` to enable flexibility
2. **Setup Before Use:** Configure mock behavior before passing to code under test
3. **Verify Expectations:** Use Verify to ensure expected interactions occurred
4. **Keep Mocks Simple:** Mock only what's necessary; over-mocking creates brittle tests
5. **Use Strict Mode Carefully:** Loose mode is default; strict mode for critical contracts
6. **Avoid Mocking Concrete Types:** Mock interfaces/abstractions, not concrete implementations

---

## Performance Considerations

- **Mock Creation:** Reflection-based creation is fast (microseconds per mock)
- **Setup Overhead:** Minimal overhead for setup and verification
- **Memory:** Each mock uses minimal memory; scale to thousands without issues
- **Test Isolation:** Mocks provide excellent test isolation with no shared state

---

## See Also

- [PeasyPilot.Core API](api-core.md)
- [PeasyPilot.Unit API](api-unit.md)
- [Unit Testing Guide](../GUIDES/unit-testing-guide.md)
- [Moq Documentation](https://github.com/moq/moq4)

