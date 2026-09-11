# Integration Testing Guide

## Overview

Integration tests verify that multiple components work together correctly. Unlike unit tests that isolate code, integration tests exercise real databases, APIs, and external services.

**Prerequisites:** Complete [Unit Testing Guide](./unit-testing-guide.md)  
**Time estimate:** 30 minutes  
**Frameworks:** xUnit, NUnit, TUnit  

---

## Why Integration Testing Matters

Unit tests catch logic errors. Integration tests catch configuration errors, database issues, and interactions between components.

**When to use integration tests:**
- Testing database access layers
- Testing API endpoints
- Testing multi-component workflows
- Testing configuration-dependent code

---

## Core Concepts

### 1. Test Fixtures

Fixtures manage test infrastructure (databases, services, DI containers):

```csharp
using PeasyPilot.Integration.Fixtures;
using Xunit;

public class UserRepositoryIntegrationTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Register services for integration testing
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    }
    
    [Fact]
    public async Task AddUser_WithValidUser_Succeeds()
    {
        var repository = GetService<IUserRepository>();
        var user = new User { Name = "John", Email = "john@example.com" };
        
        await repository.AddAsync(user);
        
        var users = await repository.GetAllAsync();
        Assert.Single(users);
    }
}
```

### 2. In-Memory vs Real Databases

**In-Memory** (fast, for testing):
```csharp
services.AddSingleton<ITestDatabase, InMemoryTestDatabase>();
```

**Real Database** (production-like):
```csharp
services.AddSingleton<ITestDatabase>(provider =>
    new SqliteTestDatabase("Data Source=:memory:")
);
```

### 3. Automatic Reset

The `IResettable` interface resets singleton services between tests:

```csharp
public class InMemoryUserRepository : IUserRepository, IResettable
{
    private List<User> _users = new();
    
    public async Task ResetAsync()
    {
        _users.Clear();
        await Task.CompletedTask;
    }
}
```

---

## Examples

### Example 1: Basic Integration Test

```csharp
public class UserServiceIntegrationTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task CreateAndRetrieveUser_Succeeds()
    {
        // Arrange
        var service = GetService<IUserService>();
        var user = new User { Name = "Alice", Email = "alice@example.com" };
        
        // Act
        await service.CreateUserAsync(user);
        var retrieved = await service.GetUserAsync(user.Id);
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Alice", retrieved.Name);
    }
}
```

### Example 2: Multiple Tests with Shared Setup

```csharp
public class OrderServiceIntegrationTests : XUnitIntegrationTestFixture
{
    private IOrderService _orderService = null!;
    private IInventoryService _inventoryService = null!;
    
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOrderService, OrderService>();
        services.AddSingleton<IInventoryService, InventoryService>();
    }
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _orderService = GetService<IOrderService>();
        _inventoryService = GetService<IInventoryService>();
    }
    
    [Fact]
    public async Task CreateOrder_ChecksInventory()
    {
        await _inventoryService.AddStockAsync(productId: 1, quantity: 100);
        
        var order = await _orderService.CreateOrderAsync(
            productId: 1, 
            quantity: 10
        );
        
        Assert.NotNull(order);
        var remaining = await _inventoryService.GetStockAsync(productId: 1);
        Assert.Equal(90, remaining);
    }
}
```

### Example 3: Testing Database Transactions

```csharp
[Fact]
public async Task TransferMoney_RollsBackOnFailure()
{
    var service = GetService<IAccountService>();
    var from = new Account { Balance = 100 };
    var to = new Account { Balance = 50 };
    
    // This should fail (insufficient funds after transfer)
    var exception = await Assert.ThrowsAsync<InsufficientFundsException>(
        () => service.TransferAsync(from, to, amount: 150)
    );
    
    // Verify rollback: balances unchanged
    Assert.Equal(100, from.Balance);
    Assert.Equal(50, to.Balance);
}
```

### Example 4: Web API Integration Test

```csharp
public class UserApiIntegrationTests : XUnitIntegrationTestFixture
{
    private HttpClient _httpClient = null!;
    
    protected override void ConfigureServices(IServiceCollection services)
    {
        var factory = new WebApplicationFactory<Program>();
        _httpClient = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetUser_WithValidId_Returns200()
    {
        var response = await _httpClient.GetAsync("/api/users/1");
        
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("John", content);
    }
}
```

### Example 5: Reset Between Tests

```csharp
public class UserRepositoryResetTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task FirstTest_AddsUser()
    {
        var repository = GetService<IUserRepository>();
        await repository.AddAsync(new User { Name = "Alice" });
        
        var users = await repository.GetAllAsync();
        Assert.Single(users);
    }
    
    [Fact]
    public async Task SecondTest_StartsClean()
    {
        var repository = GetService<IUserRepository>();
        
        // Automatically reset between tests
        var users = await repository.GetAllAsync();
        Assert.Empty(users);
    }
}
```

---

## Best Practices

✅ **DO**
- Use in-memory databases for speed
- Reset state between tests automatically
- Test real configurations
- Keep integration tests focused
- Mock external APIs (third-party services)

❌ **DON'T**
- Connect to production databases
- Skip cleanup between tests
- Test multiple workflows in one test
- Ignore setup failures
- Use hardcoded timeouts

---

## Next Steps

📖 **[BDD Testing Guide](./bdd-testing-guide.md)** – Test business behavior  
📖 **[Unit Testing Guide](./unit-testing-guide.md)** – Review unit testing patterns  
📖 **[Test Generation Guide](./test-generation-guide.md)** – Auto-generate tests  
📖 **[Framework Adapters](./framework-adapters-guide.md)** – xUnit vs NUnit vs TUnit  

Integration tests + unit tests = comprehensive coverage. 🎯

---

**[← Back to Learning Guides](./README.md)** | **[← Back to Documentation Hub](../README.md)**

**Version:** English | **[Français](./integration-testing-guide-FR.md)**
