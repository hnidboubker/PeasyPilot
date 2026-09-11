# Advanced Testing Patterns

## Overview

In this guide you'll learn advanced testing strategies and patterns to write more effective, maintainable, and comprehensive tests:
- Master the Arrange-Act-Assert (AAA) pattern and its advanced variations
- Apply Given-When-Then patterns for BDD-style testing
- Build test objects with fluent Builder patterns
- Manage test lifecycle with Fixture patterns
- Validate test quality with Mutation testing
- Generate test cases with Property-Based testing
- Use Test Doubles (Mocks, Stubs, Fakes) effectively

**Prerequisites:** Complete [Unit Testing Guide](../GUIDES/unit-testing-guide.md)  
**Time estimate:** 60 minutes  
**Frameworks:** xUnit, NUnit, TUnit  
**Code examples:** 15+ working examples

---

## 1. Arrange-Act-Assert (AAA) Pattern — Advanced Variations

### Core Pattern (Review)

The AAA pattern structures tests in three distinct phases. While you've seen the basics, advanced variations help with complex scenarios.

#### Variation 1: Arrange-Act-Assert-Cleanup (AAA+C)

For tests that require resource cleanup:

```csharp
[Fact]
public void FileProcessor_CreatesLogFile_WhenProcessingDocument()
{
    // ARRANGE
    var logPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    var processor = new DocumentProcessor(logPath);
    var testDoc = new Document { Content = "Test content" };
    
    try
    {
        // ACT
        processor.Process(testDoc);
        
        // ASSERT
        Assert.True(File.Exists(logPath));
        var logContent = File.ReadAllText(logPath);
        Assert.Contains("Process completed", logContent);
    }
    finally
    {
        // CLEANUP
        if (File.Exists(logPath))
            File.Delete(logPath);
    }
}
```

#### Variation 2: Multiple Assertions with Context

When testing complex objects, group assertions by context:

```csharp
[Fact]
public void UserRepository_CreateUser_PersistsAllProperties()
{
    // ARRANGE
    var repository = new UserRepository(_fixture.DbContext);
    var user = new User
    {
        FirstName = "Alice",
        LastName = "Smith",
        Email = "alice@example.com",
        CreatedDate = DateTime.UtcNow
    };
    
    // ACT
    var createdUser = repository.Create(user);
    
    // ASSERT - Basic Properties
    Assert.NotNull(createdUser.Id);
    Assert.Equal(user.FirstName, createdUser.FirstName);
    Assert.Equal(user.LastName, createdUser.LastName);
    
    // ASSERT - Email Properties
    Assert.Equal(user.Email, createdUser.Email);
    Assert.True(createdUser.EmailConfirmed == false);
    
    // ASSERT - Audit Properties
    Assert.Equal(DateTime.UtcNow.Date, createdUser.CreatedDate.Date);
    Assert.Null(createdUser.ModifiedDate);
}
```

#### Variation 3: Assert-Verify Pattern (for state-based vs interaction-based testing)

```csharp
[Fact]
public void OrderProcessor_ProcessesOrder_UpdatesInventoryAndSendsNotification()
{
    // ARRANGE
    var inventoryMock = new Mock<IInventoryService>();
    var notificationMock = new Mock<INotificationService>();
    var processor = new OrderProcessor(inventoryMock.Object, notificationMock.Object);
    
    var order = new Order { OrderId = 1, Quantity = 5 };
    
    // ACT
    processor.ProcessOrder(order);
    
    // ASSERT - State
    inventoryMock.Verify(x => x.DecrementStock(1, 5), Times.Once);
    
    // ASSERT - Interactions
    notificationMock.Verify(
        x => x.SendOrderConfirmation(It.Is<Order>(o => o.OrderId == 1)),
        Times.Once);
}
```

---

## 2. Given-When-Then (GWT) Pattern — BDD Style

The GWT pattern aligns tests with business requirements using BDD principles. This integrates naturally with PeasyPilot's BDD framework.

#### Example: User Registration Scenario

```csharp
public class UserRegistrationBddTests : PeasyPilotBddTestBase
{
    private UserRepository _userRepository;
    private UserService _userService;
    private User _newUser;
    private Exception _registrationException;
    
    [Fact]
    public void UserRegistration_ValidEmail_SuccessfullyCreatesAccount()
    {
        // GIVEN: A new user with valid credentials
        GivenNewUserWithValidCredentials();
        
        // WHEN: The user is registered
        WhenUserIsRegistered();
        
        // THEN: The account is created and user can login
        ThenUserAccountIsCreated();
        ThenUserCanLogin();
    }
    
    [Fact]
    public void UserRegistration_DuplicateEmail_FailsWithConflict()
    {
        // GIVEN: An existing user with email alice@example.com
        GivenExistingUserWithEmail("alice@example.com");
        
        // WHEN: Attempting to register with the same email
        WhenUserIsRegisteredWithEmail("alice@example.com");
        
        // THEN: Registration fails with duplicate email error
        ThenRegistrationFailsWithError("Duplicate email");
    }
    
    // Given-When-Then helper methods
    private void GivenNewUserWithValidCredentials()
    {
        _newUser = new User
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "SecurePassword123!"
        };
    }
    
    private void GivenExistingUserWithEmail(string email)
    {
        var user = new User
        {
            Email = email,
            FirstName = "Existing",
            LastName = "User"
        };
        _userRepository.Create(user);
    }
    
    private void WhenUserIsRegistered()
    {
        try
        {
            _userService.Register(_newUser);
            _registrationException = null;
        }
        catch (Exception ex)
        {
            _registrationException = ex;
        }
    }
    
    private void WhenUserIsRegisteredWithEmail(string email)
    {
        _newUser.Email = email;
        WhenUserIsRegistered();
    }
    
    private void ThenUserAccountIsCreated()
    {
        Assert.Null(_registrationException);
        var savedUser = _userRepository.GetByEmail(_newUser.Email);
        Assert.NotNull(savedUser);
    }
    
    private void ThenUserCanLogin()
    {
        var loginResult = _userService.Login(_newUser.Email, _newUser.Password);
        Assert.NotNull(loginResult.Token);
    }
    
    private void ThenRegistrationFailsWithError(string expectedError)
    {
        Assert.NotNull(_registrationException);
        Assert.Contains(expectedError, _registrationException.Message);
    }
}
```

---

## 3. Builder Pattern for Tests

The Builder pattern reduces test setup complexity and improves readability through fluent APIs.

#### Example 1: TestDataBuilder

```csharp
public class OrderBuilder
{
    private string _customerId = "CUST001";
    private decimal _totalAmount = 100m;
    private List<OrderItem> _items = new();
    private OrderStatus _status = OrderStatus.Pending;
    private DateTime _createdDate = DateTime.UtcNow;
    
    public OrderBuilder WithCustomerId(string customerId)
    {
        _customerId = customerId;
        return this;
    }
    
    public OrderBuilder WithAmount(decimal amount)
    {
        _totalAmount = amount;
        return this;
    }
    
    public OrderBuilder WithItem(string productId, int quantity, decimal price)
    {
        _items.Add(new OrderItem
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = price
        });
        return this;
    }
    
    public OrderBuilder WithStatus(OrderStatus status)
    {
        _status = status;
        return this;
    }
    
    public OrderBuilder CreatedOn(DateTime date)
    {
        _createdDate = date;
        return this;
    }
    
    public Order Build()
    {
        return new Order
        {
            CustomerId = _customerId,
            TotalAmount = _totalAmount,
            Items = _items,
            Status = _status,
            CreatedDate = _createdDate
        };
    }
}

// Usage in tests
[Fact]
public void OrderProcessor_CalculateDiscount_AppliesToOrders()
{
    // ARRANGE: Build a complex order with builder
    var order = new OrderBuilder()
        .WithCustomerId("CUST001")
        .WithItem("PROD001", 2, 50m)
        .WithItem("PROD002", 1, 100m)
        .WithStatus(OrderStatus.Confirmed)
        .Build();
    
    var processor = new OrderProcessor();
    
    // ACT
    var discountedTotal = processor.CalculateDiscount(order);
    
    // ASSERT
    Assert.True(discountedTotal < order.TotalAmount);
}
```

#### Example 2: AnonymousBuilder Pattern

For when you only care about specific properties:

```csharp
public class AnonymousUserBuilder
{
    private readonly User _user = new();
    
    public AnonymousUserBuilder()
    {
        _user.Id = Guid.NewGuid();
        _user.FirstName = "John";
        _user.LastName = "Doe";
        _user.Email = $"user_{Guid.NewGuid()}@example.com";
        _user.IsActive = true;
    }
    
    public AnonymousUserBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }
    
    public AnonymousUserBuilder WithoutEmail()
    {
        _user.Email = null;
        return this;
    }
    
    public AnonymousUserBuilder AsInactive()
    {
        _user.IsActive = false;
        return this;
    }
    
    public User Build() => _user;
}

[Fact]
public void UserValidator_ValidatesEmail_WhenPresent()
{
    // Arrange: Only customize what matters for this test
    var user = new AnonymousUserBuilder()
        .WithEmail("invalid-email")
        .Build();
    
    // Act & Assert
    Assert.False(UserValidator.IsValid(user));
}
```

---

## 4. Fixture Patterns

Fixtures manage test setup, teardown, and resource lifecycle across multiple tests.

#### Example 1: Shared Fixture (One instance per test class)

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly IHost _host;
    public DbContext DbContext { get; private set; }
    
    public DatabaseFixture()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<DbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            })
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _host.StartAsync();
        DbContext = _host.Services.GetRequiredService<DbContext>();
        await DbContext.Database.EnsureCreatedAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
    }
}

public class UserRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = new();
    
    public Task InitializeAsync() => _fixture.InitializeAsync();
    public Task DisposeAsync() => _fixture.DisposeAsync();
    
    [Fact]
    public void Repository_GetUser_ReturnsPersistedUser()
    {
        // ARRANGE
        var user = new User { Email = "test@example.com" };
        _fixture.DbContext.Users.Add(user);
        _fixture.DbContext.SaveChanges();
        
        var repository = new UserRepository(_fixture.DbContext);
        
        // ACT
        var result = repository.GetByEmail("test@example.com");
        
        // ASSERT
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }
}
```

#### Example 2: Isolated Fixture (Fresh instance per test)

```csharp
public class IsolatedDatabaseFixture : IAsyncLifetime
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    public DbContext DbContext { get; private set; }
    
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;
        
        DbContext = new DbContext(options);
        await DbContext.Database.EnsureCreatedAsync();
    }
    
    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        DbContext.Dispose();
    }
}

public class OrderProcessingTests
{
    [Fact]
    public async Task ProcessOrder_WithValidData_CreatesOrderRecord()
    {
        // Use fixture - fresh database for this test
        await using var fixture = new IsolatedDatabaseFixture();
        await fixture.InitializeAsync();
        
        var repository = new OrderRepository(fixture.DbContext);
        var order = new Order { /* ... */ };
        
        repository.Create(order);
        
        var retrieved = repository.GetById(order.Id);
        Assert.NotNull(retrieved);
        
        await fixture.DisposeAsync();
    }
}
```

---

## 5. Mutation Testing

Mutation testing validates that your tests actually catch bugs by injecting small code changes (mutations).

### How Mutation Testing Works

A mutation tool modifies your code (e.g., `>` to `>=`, `true` to `false`) and re-runs tests. If tests pass despite the mutation, your tests have a gap.

#### Example: Writing Tests for Mutation Testing

```csharp
public class PriceCalculator
{
    public decimal CalculateDiscount(decimal price, int quantityPurchased)
    {
        // Business rule: 10% discount if quantity >= 10
        if (quantityPurchased >= 10)
            return price * 0.9m;
        
        return price;
    }
}

// ❌ WEAK TEST: Doesn't catch mutation of >= to >
[Fact]
public void CalculateDiscount_Quantity10_AppliesDiscount()
{
    var calc = new PriceCalculator();
    Assert.Equal(90m, calc.CalculateDiscount(100m, 10));
}

// ✅ STRONG TESTS: Catches mutations
[Fact]
public void CalculateDiscount_Quantity10_AppliesDiscount()
{
    var calc = new PriceCalculator();
    Assert.Equal(90m, calc.CalculateDiscount(100m, 10));
}

[Fact]
public void CalculateDiscount_Quantity9_NoDiscount()
{
    var calc = new PriceCalculator();
    Assert.Equal(100m, calc.CalculateDiscount(100m, 9));
}

[Fact]
public void CalculateDiscount_Quantity100_AppliesDiscount()
{
    var calc = new PriceCalculator();
    Assert.Equal(90m, calc.CalculateDiscount(100m, 100));
}
```

**Why strong tests matter:** The weak test passes even if someone changes `>=` to `>`. The strong tests catch this mutation.

### Best Practices for Mutation Testing

1. **Test boundary conditions** – `=, <, >, <=, >=`
2. **Test both branches** – `if/else`, `true/false`
3. **Test operator changes** – `+` to `-`, `*` to `/`
4. **Use assertion libraries** – More specific assertions catch more mutations

```csharp
// Better mutation detection with FluentAssertions
[Fact]
public void CalculateDiscount_EdgeCases_WorkCorrectly()
{
    var calc = new PriceCalculator();
    
    calc.CalculateDiscount(100m, 9).Should().Be(100m);
    calc.CalculateDiscount(100m, 10).Should().Be(90m);
    calc.CalculateDiscount(100m, 0).Should().Be(100m);
}
```

---

## 6. Property-Based Testing

Property-based testing generates many test cases automatically, catching edge cases you might miss.

### Using Bogus for Data Generation

PeasyPilot includes Bogus support for generating realistic test data:

```csharp
using Bogus;
using Xunit;

public class PasswordValidationTests
{
    private readonly Faker<User> _userFaker = new Faker<User>()
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.Password, f => f.Internet.Password(8));
    
    [Fact]
    public void PasswordValidator_100GeneratedPasswords_AllValidateCorrectly()
    {
        var validator = new PasswordValidator();
        
        // Generate 100 random users
        var users = _userFaker.Generate(100);
        
        foreach (var user in users)
        {
            var result = validator.IsPasswordValid(user.Password);
            Assert.True(result, $"Password '{user.Password}' should be valid");
        }
    }
    
    [Fact]
    public void PasswordValidator_WeakPasswords_Rejected()
    {
        var validator = new PasswordValidator();
        var weakPasswords = new[] { "123", "abc", "pass", "12345", "" };
        
        foreach (var password in weakPasswords)
        {
            var result = validator.IsPasswordValid(password);
            Assert.False(result, $"Password '{password}' should be invalid");
        }
    }
}
```

### Property-Based Testing Patterns

```csharp
public class MathOperationsPropertyTests
{
    private readonly Faker _faker = new();
    
    [Fact]
    public void Addition_Commutative_Property()
    {
        // Property: a + b == b + a (for any a, b)
        for (int i = 0; i < 100; i++)
        {
            decimal a = (decimal)_faker.Random.Double();
            decimal b = (decimal)_faker.Random.Double();
            
            var calc = new Calculator();
            Assert.Equal(calc.Add(a, b), calc.Add(b, a));
        }
    }
    
    [Fact]
    public void Multiplication_IdentityProperty()
    {
        // Property: a * 1 == a (for any a)
        for (int i = 0; i < 100; i++)
        {
            decimal a = (decimal)_faker.Random.Double();
            
            var calc = new Calculator();
            Assert.Equal(a, calc.Multiply(a, 1));
        }
    }
}
```

---

## 7. Test Doubles — Mocks, Stubs, and Fakes

### Understanding Test Doubles

| Test Double | Purpose | When to Use |
|-------------|---------|------------|
| **Mock** | Verify interactions | Testing that methods are called |
| **Stub** | Provide canned responses | Simulating external service behavior |
| **Fake** | Working implementation | Testing domain logic |
| **Spy** | Record interactions | Partial mocking with real behavior |

### Example 1: Mock (Verify Interactions)

```csharp
[Fact]
public void OrderProcessor_SendsConfirmationEmail_WhenOrderConfirmed()
{
    // ARRANGE
    var emailServiceMock = new Mock<IEmailService>();
    var processor = new OrderProcessor(emailServiceMock.Object);
    var order = new Order { OrderId = 123, CustomerEmail = "test@example.com" };
    
    // ACT
    processor.ConfirmOrder(order);
    
    // ASSERT: Verify the email service was called correctly
    emailServiceMock.Verify(
        x => x.SendConfirmation(
            It.Is<Order>(o => o.OrderId == 123),
            It.Is<string>(s => s == "test@example.com")),
        Times.Once,
        "Email should be sent exactly once");
}
```

### Example 2: Stub (Provide Responses)

```csharp
[Fact]
public void UserService_GetUser_ReturnsStubData()
{
    // ARRANGE: Create stub
    var userRepositoryStub = new Mock<IUserRepository>();
    userRepositoryStub
        .Setup(x => x.GetById(1))
        .Returns(new User { Id = 1, FirstName = "Alice" });
    
    var service = new UserService(userRepositoryStub.Object);
    
    // ACT
    var user = service.GetUser(1);
    
    // ASSERT
    Assert.Equal("Alice", user.FirstName);
}
```

### Example 3: Fake (Working Implementation)

```csharp
public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<int, User> _users = new();
    
    public User GetById(int id)
    {
        return _users.TryGetValue(id, out var user) ? user : null;
    }
    
    public void Save(User user)
    {
        _users[user.Id] = user;
    }
}

[Fact]
public void UserService_SaveAndRetrieve_WorksWithFakeRepository()
{
    // ARRANGE: Use fake implementation
    var fakeRepository = new InMemoryUserRepository();
    var service = new UserService(fakeRepository);
    var user = new User { Id = 1, FirstName = "Bob" };
    
    // ACT
    service.SaveUser(user);
    var retrieved = service.GetUser(1);
    
    // ASSERT
    Assert.Equal("Bob", retrieved.FirstName);
}
```

### Example 4: Spy (Record + Verify)

```csharp
[Fact]
public void Logger_LogsAllMessages_WhenEnabled()
{
    // ARRANGE: Spy on real logger
    var realLogger = new ConsoleLogger();
    var loggerSpy = new Mock<ILogger>(MockBehavior.Default) { CallBase = true };
    loggerSpy.Setup(x => x.Log(It.IsAny<string>())).CallBase();
    
    var service = new ReportGenerator(loggerSpy.Object);
    
    // ACT
    service.GenerateReport();
    
    // ASSERT: Verify what was logged
    loggerSpy.Verify(x => x.Log(It.IsAny<string>()), Times.AtLeastOnce);
}
```

---

## 8. Testing Asynchronous Code

Asynchronous code introduces unique testing challenges. Understanding async patterns ensures your tests are reliable and don't hide race conditions.

### Pattern 1: Testing Async Methods

```csharp
[Fact]
public async Task UserService_GetUserAsync_ReturnsUser()
{
    // ARRANGE
    var repository = new Mock<IUserRepository>();
    repository
        .Setup(x => x.GetUserAsync(1))
        .ReturnsAsync(new User { Id = 1, FirstName = "Alice" });
    
    var service = new UserService(repository.Object);
    
    // ACT
    var result = await service.GetUserAsync(1);
    
    // ASSERT
    Assert.NotNull(result);
    Assert.Equal("Alice", result.FirstName);
}
```

### Pattern 2: Testing Task Cancellation

```csharp
[Fact]
public async Task LongRunningOperation_WithCancellation_StopsGracefully()
{
    // ARRANGE
    var cts = new CancellationTokenSource();
    var operation = new LongRunningOperation();
    
    // ACT: Start operation and cancel after 100ms
    var task = operation.ExecuteAsync(cts.Token);
    await Task.Delay(100);
    cts.Cancel();
    
    // ASSERT: Operation should be cancelled
    await Assert.ThrowsAsync<OperationCanceledException>(() => task);
}
```

### Pattern 3: Testing Timeout Behavior

```csharp
[Fact]
public async Task ApiClient_WithTimeout_ThrowsTimeoutException()
{
    // ARRANGE
    var client = new ApiClient(timeoutMs: 100);
    
    // ACT & ASSERT: Should timeout
    await Assert.ThrowsAsync<TimeoutException>(
        () => client.FetchDataAsync("http://httpbin.org/delay/5"));
}
```

### Pattern 4: Task Composition and Multiple Async Calls

```csharp
[Fact]
public async Task OrderProcessor_ProcessMultipleOrders_ExecutesInParallel()
{
    // ARRANGE
    var processor = new OrderProcessor();
    var orders = new[] 
    { 
        new Order { Id = 1 }, 
        new Order { Id = 2 }, 
        new Order { Id = 3 } 
    };
    
    var stopwatch = Stopwatch.StartNew();
    
    // ACT: Process orders in parallel
    var results = await Task.WhenAll(
        orders.Select(o => processor.ProcessAsync(o))
    );
    
    stopwatch.Stop();
    
    // ASSERT: All completed successfully and faster than sequential
    Assert.Equal(3, results.Length);
    Assert.True(stopwatch.ElapsedMilliseconds < 1000); // Fast because parallel
}
```

---

## 9. Advanced Testing Strategies

### Strategy 1: Test Pyramid

Structure tests by type and count:

```
        △ E2E Tests (10%)
       △△ Integration Tests (30%)
      △△△△△ Unit Tests (60%)
```

```csharp
// Unit Test (Fast, Isolated)
[Fact]
public void Validator_Email_WithValidEmail_ReturnsTrue()
{
    var validator = new EmailValidator();
    Assert.True(validator.IsValid("test@example.com"));
}

// Integration Test (Moderate speed, with dependencies)
[Fact]
public async Task UserRepository_SaveAndLoad_PersistsToDatabase()
{
    var dbContext = GetTestDbContext();
    var repository = new UserRepository(dbContext);
    var user = new User { Email = "test@example.com" };
    
    repository.Save(user);
    var loaded = repository.GetByEmail("test@example.com");
    
    Assert.NotNull(loaded);
}

// E2E Test (Slow, full system)
[Fact]
public async Task API_RegisterUser_CreateAccountViaHttpRequest()
{
    var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    var response = await client.PostAsync("/api/users/register", 
        new StringContent("{\"email\":\"test@example.com\"}"));
    
    Assert.True(response.IsSuccessStatusCode);
}
```

### Strategy 2: Test Isolation with Setup/Teardown

```csharp
public class UserServiceTests : IAsyncLifetime
{
    private DbContext _dbContext;
    private UserService _userService;
    
    public async Task InitializeAsync()
    {
        // Setup: Create clean database
        _dbContext = new TestDbContextFactory().CreateDbContext();
        await _dbContext.Database.EnsureCreatedAsync();
        _userService = new UserService(_dbContext);
    }
    
    public async Task DisposeAsync()
    {
        // Teardown: Clean up resources
        await _dbContext.Database.EnsureDeletedAsync();
        _dbContext.Dispose();
    }
    
    [Fact]
    public void CreateUser_WithValidData_SuccessfullyCreates()
    {
        var user = new User { Email = "test@example.com" };
        var result = _userService.Create(user);
        Assert.NotNull(result.Id);
    }
}
```

---

## Best Practices Summary

| Practice | Benefit |
|----------|---------|
| **Single Responsibility** | Tests are easier to maintain |
| **Clear Naming** | Failing tests are self-documenting |
| **AAA/GWT Structure** | Tests are predictable and readable |
| **Builders** | Reduces setup boilerplate |
| **Fixtures** | Manages resource lifecycle safely |
| **Test Doubles** | Isolates code under test |
| **Comprehensive Coverage** | Catches mutations and edge cases |
| **Property-Based Testing** | Finds edge cases automatically |

---

## Putting It All Together

Here's a complete example combining multiple patterns:

```csharp
public class CompleteOrderProcessingTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly OrderProcessor _processor;
    
    // Using fixture pattern
    public CompleteOrderProcessingTests()
    {
        var fixture = new DatabaseFixture();
        _orderRepository = new OrderRepository(fixture.DbContext);
        _processor = new OrderProcessor(_orderRepository, new Mock<IEmailService>().Object);
    }
    
    [Fact]
    public void ProcessOrder_ValidOrder_SuccessfullyProcesses()
    {
        // Using builder pattern
        var order = new OrderBuilder()
            .WithCustomerId("CUST001")
            .WithAmount(100m)
            .WithItem("PROD001", 10, 10m)
            .Build();
        
        // AAA pattern with GWT style steps
        // GIVEN
        _orderRepository.Save(order);
        
        // WHEN
        var result = _processor.Process(order);
        
        // THEN
        Assert.True(result.IsSuccessful);
        Assert.Equal(OrderStatus.Processed, result.Status);
    }
}
```

---

## Learning Path

1. **Foundation** – Master AAA and GWT patterns
2. **Efficiency** – Learn builders and fixtures
3. **Quality** – Implement mutation and property-based testing
4. **Isolation** – Understand test doubles thoroughly
5. **Integration** – Combine patterns for complex scenarios

---

## Additional Resources

- [Unit Testing Guide](../GUIDES/unit-testing-guide.md)
- [Integration Testing Guide](../GUIDES/integration-testing-guide.md)
- [BDD Testing Guide](../GUIDES/bdd-testing-guide.md)
- [Moq Documentation](../PeasyPilot-Moq.md)
- [Test Generation Guide](../GUIDES/test-generation-guide.md)

---

**[← Back to Advanced Topics](./README.md)** | **[← Back to Documentation Hub](../README.md)**

**Version:** English | **[Français](./testing-patterns-FR.md)**
