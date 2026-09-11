# Troubleshooting & Debugging Guide

## Overview

When tests fail, finding the root cause quickly is essential. This guide teaches you systematic debugging techniques to diagnose and resolve test failures effectively.

**Prerequisites:** Complete [Unit Testing Guide](../GUIDES/unit-testing-guide.md) and [Integration Testing Guide](../GUIDES/integration-testing-guide.md)  
**Time estimate:** 45 minutes  
**Frameworks:** xUnit, NUnit, TUnit  
**Code examples:** 10 working examples

In this guide you'll learn:
- How to identify the root cause of test failures
- Effective use of the debugger and breakpoints
- Strategic logging and diagnostics
- Analysis of stack traces and error messages
- Diagnosis of flaky and intermittent failures
- Debugging integration test issues
- CI/CD failure reproduction

---

## Part 1: Understanding Test Failures

### Types of Test Failures

Tests can fail in several distinct ways, each requiring different debugging approaches:

1. **Assertion Failures** – Expected vs actual value mismatch
2. **Exception Failures** – Unexpected exception thrown
3. **Timeout Failures** – Test exceeds timeout duration
4. **Setup/Teardown Failures** – Error in test initialization or cleanup
5. **Flaky Failures** – Passes sometimes, fails other times
6. **Environment Failures** – Dependent on local configuration

### The Debugging Workflow

```
├─ Read the failure message carefully
├─ Examine the stack trace
├─ Reproduce the failure locally
├─ Add diagnostic logging
├─ Use the debugger (breakpoints, watches)
├─ Isolate the problem area
└─ Implement the fix
```

---

## Part 2: Debugging Test Failures

### 2.1 Reading Error Messages

The error message is your first clue. Parse it systematically:

**Example: Assertion Failure**
```
Expected: "John Smith"
Actual:   "John "
```

This tells you:
- The assertion is comparing strings
- The actual value is missing "Smith"
- Likely issue: Trimming or parsing logic error

**Example: Null Reference Exception**
```
System.NullReferenceException: Object reference not set to an instance of an object.
  at UserService.GetUser (id=5) in UserService.cs:line 42
```

This tells you:
- A null object is being dereferenced
- The issue is in GetUser method
- Likely issue: Missing null check or invalid ID

### 2.2 Stack Trace Analysis

Stack traces show the call sequence from failure point to entry point:

```csharp
// Test code
[Fact]
public async Task CreateUser_WithValidData_SavesSuccessfully()
{
    var service = new UserService(_repository); // Line 15
    var user = await service.CreateUserAsync("John", "john@example.com"); // Line 16
    Assert.NotNull(user.Id);
}

// Stack trace indicates:
// at UserService.CreateUserAsync (UserService.cs:42)
// at UserRepositoryTests.CreateUser_WithValidData_SavesSuccessfully (UserRepositoryTests.cs:16)
```

**Key insights:**
- Line 42 in UserService is where the actual failure occurs
- The test called CreateUserAsync at line 16
- Trace shows: UserService.CreateUserAsync → Repository.SaveAsync → DbContext.SaveChangesAsync

**Reading the trace:**
1. Find the innermost frame in your code (ignore framework frames)
2. Look at the file and line number
3. Check what operation was happening there
4. Move up the stack to see what triggered it

### 2.3 Using the Debugger Effectively

**Setting Breakpoints:**

```csharp
[Fact]
public async Task UpdateUser_WithValidId_UpdatesName()
{
    // Arrange
    var service = new UserService(_repository);
    var user = await service.CreateUserAsync("John", "john@example.com");
    
    // Act – Place breakpoint on next line
    var updated = await service.UpdateUserAsync(user.Id, "Jane");
    
    // Assert
    Assert.Equal("Jane", updated.Name);
}
```

**Breakpoint types:**
- **Simple breakpoint** – Pause execution at line
- **Conditional breakpoint** – Pause only if condition is true: `id == 5`
- **Log breakpoint** – Print message without pausing

**Using Watches:**
```
Watch: user.Id = 0 (should be > 0)
Watch: repository.GetUser(id) = null (should return User)
Watch: service._logger = null (dependency not injected)
```

---

## Part 3: Logging Strategies

### 3.1 Structured Logging

Use ILogger for diagnostics that persist when tests run in CI:

```csharp
using Microsoft.Extensions.Logging;

public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _repository;
    
    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<User> CreateUserAsync(string name, string email)
    {
        _logger.LogInformation("Creating user: {Name} ({Email})", name, email);
        
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Email is empty for user: {Name}", name);
            throw new ArgumentException("Email is required");
        }
        
        try
        {
            var user = new User { Name = name, Email = email };
            var saved = await _repository.AddAsync(user);
            
            _logger.LogInformation("User created successfully: {UserId}", saved.Id);
            return saved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user: {Name} ({Email})", name, email);
            throw;
        }
    }
}
```

### 3.2 Log Levels and Filtering

**Log Levels (ordered by severity):**
- **Trace** – Most detailed, internal framework details
- **Debug** – Development diagnostics
- **Information** – Significant events (startup, creation, completion)
- **Warning** – Unusual conditions that don't prevent execution
- **Error** – Recoverable errors
- **Critical** – System-level failures

**Configure logging in tests:**

```csharp
public class UserServiceTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureLogging(ILoggingBuilder logging)
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
        
        // Filter specific namespaces
        logging.AddFilter("UserService", LogLevel.Debug);
        logging.AddFilter("Repository", LogLevel.Information);
    }
    
    [Fact]
    public async Task CreateUser_WithLogging_TracesExecution()
    {
        var service = GetService<UserService>();
        
        // Debug output will show:
        // [Information] Creating user: John (john@example.com)
        // [Debug] Validating email format
        // [Information] User created successfully: 123
        
        var user = await service.CreateUserAsync("John", "john@example.com");
        Assert.NotNull(user.Id);
    }
}
```

### 3.3 Capturing Context

Log relevant context to speed up debugging:

```csharp
public async Task<Order> CreateOrderAsync(int customerId, List<LineItem> items)
{
    var context = new Dictionary<string, object>
    {
        { "CustomerId", customerId },
        { "ItemCount", items.Count },
        { "TotalAmount", items.Sum(i => i.Price * i.Quantity) },
        { "Timestamp", DateTime.UtcNow }
    };
    
    using (_logger.BeginScope(context))
    {
        _logger.LogInformation("Processing order for customer");
        
        if (items.Count == 0)
        {
            _logger.LogWarning("Order has no items");
            throw new InvalidOperationException("Order must contain at least one item");
        }
        
        try
        {
            var order = new Order { CustomerId = customerId };
            return await _repository.SaveOrderAsync(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order");
            throw;
        }
    }
}
```

---

## Part 4: Common Test Failures

### 4.1 Flaky Tests (Timing Issues)

**Problem:** Test passes sometimes, fails other times.

**Common causes:**
- Async operations not properly awaited
- Race conditions in multithreaded code
- External service timeouts
- System time dependencies

**Example of flaky test:**

```csharp
// ❌ BAD: Flaky – doesn't wait for async operation
[Fact]
public async Task ProcessOrder_CompletesQuickly()
{
    var service = new OrderProcessingService();
    
    service.ProcessOrderAsync(orderId: 1); // Fire and forget
    
    var status = await service.GetStatusAsync(1);
    Assert.Equal("Completed", status); // May fail if not ready yet
}

// ✅ GOOD: Wait for completion
[Fact]
public async Task ProcessOrder_CompletesQuickly()
{
    var service = new OrderProcessingService();
    
    await service.ProcessOrderAsync(orderId: 1); // Wait for completion
    
    var status = await service.GetStatusAsync(1);
    Assert.Equal("Completed", status);
}
```

**Another common source – timing expectations:**

```csharp
// ❌ BAD: Hardcoded delays
[Fact]
public async Task BackgroundJob_ExecutesWithinTimeout()
{
    var job = new BackgroundJob();
    job.Start();
    
    await Task.Delay(100); // May not be enough on slow machines
    
    Assert.True(job.IsComplete);
}

// ✅ GOOD: Wait with timeout assertion
[Fact]
public async Task BackgroundJob_ExecutesWithinTimeout()
{
    var job = new BackgroundJob();
    job.Start();
    
    var task = job.WaitForCompletionAsync();
    var completed = await task.ConfigureAwait(false);
    var timeout = TimeSpan.FromSeconds(5);
    
    Assert.True(completed.IsCompletedSuccessfully, 
        "Job should complete within timeout");
}
```

### 4.2 Data Isolation Problems

**Problem:** Tests interfere with each other, sharing database state.

**Example of data isolation failure:**

```csharp
// ❌ BAD: Tests share data
[Collection("Database collection")]
public class UserRepositoryTests
{
    private static readonly List<User> _users = new(); // Shared state!
    
    [Fact]
    public void Test1_CreateUser_Succeeds()
    {
        _users.Add(new User { Id = 1, Name = "John" });
        Assert.Single(_users); // Passes
    }
    
    [Fact]
    public void Test2_CreateAnotherUser_Succeeds()
    {
        _users.Add(new User { Id = 2, Name = "Jane" });
        Assert.Single(_users); // Fails if Test1 runs first!
    }
}

// ✅ GOOD: Isolated test fixtures
public class UserRepositoryTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    }
    
    [Fact]
    public async Task Test1_CreateUser_Succeeds()
    {
        var repo = GetService<IUserRepository>();
        await repo.AddAsync(new User { Name = "John" });
        
        var users = await repo.GetAllAsync();
        Assert.Single(users);
    }
    
    [Fact]
    public async Task Test2_CreateAnotherUser_Succeeds()
    {
        var repo = GetService<IUserRepository>();
        await repo.AddAsync(new User { Name = "Jane" });
        
        var users = await repo.GetAllAsync();
        Assert.Single(users); // Fresh state for each test
    }
}
```

### 4.3 Async/Await Issues

**Problem:** Synchronous code blocking async operations.

```csharp
// ❌ BAD: Blocking async call with .Result
[Fact]
public void GetUser_WithValidId_ReturnsUser()
{
    var service = new UserService();
    var user = service.GetUserAsync(1).Result; // Causes deadlock
    Assert.NotNull(user);
}

// ✅ GOOD: Await properly
[Fact]
public async Task GetUser_WithValidId_ReturnsUser()
{
    var service = new UserService();
    var user = await service.GetUserAsync(1);
    Assert.NotNull(user);
}
```

**Example: Missing ConfigureAwait**

```csharp
// ❌ Potential issue in library code
public async Task<User> GetUserAsync(int id)
{
    var user = await _repository.GetAsync(id); // Don't capture context
    return user;
}

// ✅ GOOD: Use ConfigureAwait(false) in library code
public async Task<User> GetUserAsync(int id)
{
    var user = await _repository.GetAsync(id).ConfigureAwait(false);
    return user;
}
```

---

## Part 5: Diagnostic Tools and Techniques

### 5.1 Assertion Helpers

Create helpers to provide better failure messages:

```csharp
public static class AssertionHelpers
{
    public static void AssertUserEqual(User expected, User actual, string? message = null)
    {
        var failures = new List<string>();
        
        if (expected.Id != actual.Id)
            failures.Add($"Id: expected {expected.Id}, got {actual.Id}");
        if (expected.Name != actual.Name)
            failures.Add($"Name: expected '{expected.Name}', got '{actual.Name}'");
        if (expected.Email != actual.Email)
            failures.Add($"Email: expected '{expected.Email}', got '{actual.Email}'");
        
        if (failures.Any())
        {
            var details = string.Join(Environment.NewLine, failures);
            var fullMessage = message == null
                ? details
                : $"{message}:{Environment.NewLine}{details}";
            throw new Xunit.Sdk.XunitException(fullMessage);
        }
    }
}

// Usage
[Fact]
public async Task CreateUser_StoresAllFields()
{
    var service = new UserService(_repository);
    var expected = new User { Id = 1, Name = "John", Email = "john@example.com" };
    
    var actual = await service.CreateUserAsync("John", "john@example.com");
    
    AssertionHelpers.AssertUserEqual(expected, actual, "User data mismatch");
}
```

### 5.2 Memory Profiling for Tests

Detect memory leaks in tests:

```csharp
public class MemoryLeakDetectionTests
{
    [Fact]
    public void Service_DoesNotLeakMemory()
    {
        var initialMemory = GC.GetTotalMemory(true);
        
        // Allocate and dispose many objects
        for (int i = 0; i < 1000; i++)
        {
            var service = new UserService(_repository);
            service.ProcessUser(i);
            // Service should be garbage collected
        }
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(false);
        var increase = finalMemory - initialMemory;
        
        // Memory should not increase significantly (< 10MB for 1000 iterations)
        Assert.True(increase < 10_000_000, 
            $"Memory leak detected: {increase / 1024.0 / 1024.0:F2} MB increase");
    }
}
```

### 5.3 Thread Safety Analysis

Check for thread safety issues:

```csharp
public class ThreadSafetyTests
{
    [Fact]
    public void Service_IsThreadSafe()
    {
        var service = new UserService(_repository);
        var errors = new List<Exception>();
        var tasks = new List<Task>();
        
        // Simulate concurrent access from 10 threads
        for (int i = 0; i < 10; i++)
        {
            int userId = i;
            var task = Task.Run(async () =>
            {
                try
                {
                    await service.CreateUserAsync($"User{userId}", $"user{userId}@example.com");
                    await service.UpdateUserAsync(userId, $"Updated{userId}");
                    await service.DeleteUserAsync(userId);
                }
                catch (Exception ex)
                {
                    lock (errors)
                    {
                        errors.Add(ex);
                    }
                }
            });
            
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        Assert.Empty(errors); // No thread safety exceptions
    }
}
```

---

## Part 6: Integration Test Debugging

### 6.1 Database State Problems

**Problem:** Database state persists between tests.

```csharp
public class OrderIntegrationTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITestDatabase, InMemoryTestDatabase>();
        services.AddSingleton<IOrderRepository, OrderRepository>();
    }
    
    [Fact]
    public async Task CreateOrder_WithExistingData_Succeeds()
    {
        var db = GetService<ITestDatabase>();
        var repo = GetService<IOrderRepository>();
        
        // Setup: Create initial data
        await db.InitializeAsync();
        
        // Verify clean state
        var initialOrders = await repo.GetAllAsync();
        Assert.Empty(initialOrders);
        
        // Act
        var order = new Order { CustomerId = 1, Total = 100 };
        await repo.AddAsync(order);
        
        // Assert
        var allOrders = await repo.GetAllAsync();
        Assert.Single(allOrders);
    }
    
    // Each test gets fresh database state
    [Fact]
    public async Task CreateMultipleOrders_Succeeds()
    {
        var repo = GetService<IOrderRepository>();
        
        // Fresh state – previous test's data is gone
        var initialOrders = await repo.GetAllAsync();
        Assert.Empty(initialOrders);
        
        // Act
        for (int i = 1; i <= 3; i++)
        {
            await repo.AddAsync(new Order { CustomerId = i, Total = i * 100 });
        }
        
        // Assert
        var allOrders = await repo.GetAllAsync();
        Assert.Equal(3, allOrders.Count);
    }
}
```

### 6.2 Resource Cleanup Issues

**Problem:** Resources not cleaned up properly, blocking subsequent tests.

```csharp
// ❌ BAD: Resource not cleaned up
[Fact]
public async Task Service_AccessesFile()
{
    var filePath = "test.txt";
    var fileWriter = new FileWriter(filePath);
    
    await fileWriter.WriteAsync("data");
    // File is locked, next test may fail
}

// ✅ GOOD: Explicit cleanup
[Fact]
public async Task Service_AccessesFile()
{
    var filePath = "test.txt";
    var fileWriter = new FileWriter(filePath);
    
    try
    {
        await fileWriter.WriteAsync("data");
    }
    finally
    {
        fileWriter.Dispose(); // Ensure cleanup
    }
}

// ✅ BEST: Use IAsyncDisposable
[Fact]
public async Task Service_AccessesFile()
{
    var filePath = "test.txt";
    await using var fileWriter = new FileWriter(filePath);
    
    await fileWriter.WriteAsync("data");
    // Automatically disposed
}
```

---

## Part 7: CI/CD Failure Reproduction

### 7.1 Reproducing CI Failures Locally

When a test fails in CI but passes locally:

**Step 1: Check environment variables**
```csharp
[Fact]
public void Test_ReadsConfiguration()
{
    var apiUrl = Environment.GetEnvironmentVariable("API_URL");
    var apiKey = Environment.GetEnvironmentVariable("API_KEY");
    
    Assert.NotNull(apiUrl);
    Assert.NotNull(apiKey);
}
```

**Step 2: Use same test filter as CI**
```bash
# If CI runs: dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Integration"

# Match exact framework versions
dotnet test --framework net8.0
```

**Step 3: Check logging output**
```csharp
protected override void ConfigureLogging(ILoggingBuilder logging)
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
}
```

### 7.2 Common CI-Specific Issues

**Issue: Timezone differences**
```csharp
// ❌ BAD: Assumes specific timezone
var now = DateTime.Now;
var tomorrow = now.AddDays(1);

// ✅ GOOD: Use UTC
var now = DateTime.UtcNow;
var tomorrow = now.AddDays(1);
```

**Issue: Parallel test execution conflicts**
```csharp
// Use collection names to serialize tests
[Collection("Database collection")]
public class Test1 { }

[Collection("Database collection")]
public class Test2 { }

// Tests in same collection run sequentially
```

**Issue: Missing dependencies**
```csharp
[Fact]
public async Task Service_WorksWithExternalApi()
{
    // Check required services are available
    var apiClient = GetService<IExternalApiClient>();
    Assert.NotNull(apiClient);
    
    // May fail in CI if external service is down
    try
    {
        var result = await apiClient.CallAsync();
        Assert.NotNull(result);
    }
    catch (HttpRequestException)
    {
        // Document external dependency
        Assert.Skip("External API unavailable");
    }
}
```

---

## Part 8: Systematic Debugging Workflow

### Complete Example: Debugging a Failed Test

**Scenario:** Test fails intermittently in CI.

**Step 1: Collect Information**
```csharp
[Fact]
public async Task ProcessOrder_WithLogging()
{
    var logger = GetService<ILogger<OrderService>>();
    var service = GetService<OrderService>();
    
    // Add context logging
    logger.LogInformation("Test start: ProcessOrder_WithLogging");
    logger.LogInformation("Environment: {Env}", 
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
    
    var order = new Order { CustomerId = 1, Items = new[] { "Item1", "Item2" } };
    logger.LogInformation("Order created with {ItemCount} items", order.Items.Length);
    
    var result = await service.ProcessAsync(order);
    
    logger.LogInformation("Result: {Status}", result.Status);
    Assert.Equal("Completed", result.Status);
}
```

**Step 2: Add Conditional Breakpoint**
```
Breakpoint on: ProcessAsync call
Condition: order.Items.Length == 0
```

**Step 3: Reproduce in Different Scenarios**
- Run single test multiple times
- Run with other tests (check isolation)
- Run on different machine (check environment)
- Run with specific framework version

**Step 4: Identify Pattern**
- Does it fail in CI only?
- Does it fail in specific order?
- Does it fail under load?

**Step 5: Implement Fix**
```csharp
public async Task<OrderResult> ProcessAsync(Order order)
{
    if (order.Items.Length == 0)
    {
        _logger.LogWarning("Order has no items");
        throw new InvalidOperationException("Order must have items");
    }
    
    // Rest of implementation...
}
```

**Step 6: Validate Fix**
- Run test 10x locally
- Run full test suite
- Run in CI
- Monitor for regression

---

## Best Practices

### Do's ✅

- **Do** make test names descriptive of what is being tested
- **Do** use structured logging with context
- **Do** isolate tests to prevent interference
- **Do** await async operations properly
- **Do** clean up resources in finally blocks
- **Do** test edge cases and error conditions
- **Do** use specific assertions with clear messages
- **Do** document external dependencies and timeouts

### Don'ts ❌

- **Don't** ignore test failures – investigate immediately
- **Don't** use hardcoded delays instead of proper waiting
- **Don't** share state between tests
- **Don't** block async code with .Result or .Wait()
- **Don't** catch and swallow exceptions
- **Don't** assume order of test execution
- **Don't** skip flaky tests – fix the root cause
- **Don't** commit code with skipped tests

---

## Summary

Effective debugging requires systematic thinking:

1. **Read carefully** – Error messages contain clues
2. **Isolate** – Reproduce in smallest possible case
3. **Log strategically** – Capture context and flow
4. **Use tools** – Debugger, watches, memory profiler
5. **Think systematically** – What changed? What's different in CI?
6. **Fix the root cause** – Not just the symptom
7. **Validate** – Ensure fix works consistently

---

## Next Steps

- Read [Performance & Optimization Guide](./performance-optimization.md)
- Learn [Advanced Testing Patterns](./testing-patterns.md)
- Explore [Migration & Upgrade Guide](./migration-upgrade.md)

[← Back to Documentation](../README.md)
