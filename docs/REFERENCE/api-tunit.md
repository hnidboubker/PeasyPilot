# PeasyPilot.TUnit API Reference

Complete API reference for the `PeasyPilot.TUnit` adapter package.

**Target Frameworks:** .NET 8, 9, 10  
**Package:** [PeasyPilot.TUnit on NuGet](https://www.nuget.org/packages/PeasyPilot.TUnit)  
**Repository:** [GitHub: PeasyPilot](https://github.com/houssinedev/PeasyPilot)

---

## Table of Contents

1. [Overview](#overview)
2. [Main Abstractions](#main-abstractions)
3. [Async Hook Lifecycle](#async-hook-lifecycle)
4. [Key Classes & Members](#key-classes--members)
5. [Attributes & Patterns](#attributes--patterns)
6. [Working Examples](#working-examples)
7. [Best Practices](#best-practices)

---

## Overview

The `PeasyPilot.TUnit` adapter integrates TUnit 1.x with PeasyPilot's test infrastructure. It provides:

- **Native async/await support** with `BeforeEachAsync()` and `AfterEachAsync()` hooks
- **Modern test design** optimized for async workflows
- **Test context management** for sharing state within a test
- **Mock and data factory integration** for dependency injection patterns
- **Seamless integration** with PeasyPilot.Core features
- **Ultra-fast test execution** with TUnit's parallel capabilities
- **No attributes needed** – pure C# hook methods

### When to Choose TUnit

Use PeasyPilot.TUnit when:
- Your project targets .NET 9+ exclusively
- You want the fastest test framework available
- You prefer attribute-free async hooks
- Your tests are naturally async (database calls, API tests)
- You need maximum parallelization
- You want zero boilerplate with implicit async support

### Installation

```bash
dotnet add package PeasyPilot.TUnit
```

---

## Main Abstractions

### PeasyPilotTUnitTestBase

**Namespace:** `PeasyPilot.TUnit`

Base class for TUnit test classes. Provides `BeforeEachAsync()` and `AfterEachAsync()` async hooks.

#### Declaration

```csharp
public abstract class PeasyPilotTUnitTestBase
{
    protected ITestContext TestContext { get; private set; }
    protected ITestDataFactory? TestDataFactory { get; set; }
    protected IMockFactory? MockFactory { get; set; }
    
    public virtual ValueTask BeforeEachAsync();
    public virtual ValueTask AfterEachAsync();
    
    protected T GetOrCreateTestData<T>(string key, Func<T> factory) 
        where T : class;
}
```

#### Members

| Member | Type | Purpose |
|--------|------|---------|
| `TestContext` | `ITestContext` | Per-test context for state management |
| `TestDataFactory` | `ITestDataFactory?` | Optional factory for test data generation |
| `MockFactory` | `IMockFactory?` | Optional factory for creating mocks |
| `BeforeEachAsync()` | ValueTask | Async initialization before each test |
| `AfterEachAsync()` | ValueTask | Async cleanup after each test |
| `GetOrCreateTestData<T>()` | T | Get or create cached test data by key |

### TUnitAdapter

**Namespace:** `PeasyPilot.TUnit`

Adapter implementing `ITestFrameworkAdapter` for TUnit discovery and execution.

#### Declaration

```csharp
public sealed class TUnitAdapter : ITestFrameworkAdapter
{
    public string Name { get; }
    public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default);
    public Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default);
}
```

#### Properties

| Property | Returns | Description |
|----------|---------|-------------|
| `Name` | string | Always returns `"TUnit"` |

#### Methods

| Method | Returns | Purpose |
|--------|---------|---------|
| `DiscoverAsync(CancellationToken)` | `Task<IReadOnlyCollection<TestCase>>` | Discover test cases in TUnit assemblies |
| `RunAsync(TestRunRequest, CancellationToken)` | `Task<TestRunResult>` | Execute test runs with detailed results |

---

## Async Hook Lifecycle

TUnit uses `BeforeEachAsync()` and `AfterEachAsync()` `ValueTask` methods for async test lifecycle.

### Execution Order

```
For Each Test:
  1. Test class instantiated
  2. BeforeEachAsync() awaited
  3. Test method executes (naturally async)
  4. AfterEachAsync() awaited
  5. Instance disposed
```

### Example: Basic Async Lifecycle

```csharp
public class LifecycleTests : PeasyPilotTUnitTestBase
{
    private Database _db = null!;
    
    // Called before each test (async)
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _db = new Database();
        await _db.ConnectAsync();
    }
    
    // Called after each test (async)
    public override async ValueTask AfterEachAsync()
    {
        await _db.CloseAsync();
        await base.AfterEachAsync();
    }
    
    public async Task TestUsesDatabase()
    {
        var users = await _db.GetUsersAsync();
        Assert.NotEmpty(users);
    }
}
```

### Key Points

- **Always call `await base.BeforeEachAsync()`** to ensure `TestContext` is initialized
- **BeforeEachAsync returns ValueTask** – ultra-efficient for zero-allocation scenarios
- **AfterEachAsync always runs**, even if the test fails
- **Tests are naturally async** – `public async Task` method signature
- **No attributes needed** – TUnit discovers tests by convention
- **Use try-finally for critical cleanup:**

```csharp
public override async ValueTask BeforeEachAsync()
{
    await base.BeforeEachAsync();
    try
    {
        await _resource.AcquireAsync();
    }
    catch
    {
        await AfterEachAsync();
        throw;
    }
}
```

### ValueTask vs Task

TUnit uses `ValueTask` for better performance:

```csharp
// ✅ TUnit style - ValueTask (no allocation if completed synchronously)
public override async ValueTask BeforeEachAsync()
{
    await base.BeforeEachAsync();
    _data = new Data();
}

// Still works, but less efficient - Task
public override async Task BeforeEachAsync()  // Not recommended for TUnit
{
    await base.BeforeEachAsync();
    _data = new Data();
}
```

---

## Key Classes & Members

### ITestContext

Manages per-test state and caching.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestContext
{
    T GetOrAdd<T>(string key, Func<T> factory) where T : class;
    TValue? Get<TKey, TValue>(TKey key) where TKey : notnull where TValue : class;
    void Set<TKey, TValue>(TKey key, TValue value) where TKey : notnull where TValue : class;
    void Clear();
}
```

### ITestDataFactory

Optional interface for generating test data.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestDataFactory
{
    T Create<T>() where T : class, new();
    T Create<T>(Action<T> configure) where T : class, new();
}
```

### IMockFactory

Optional interface for creating mocks.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface IMockFactory
{
    Mock<T> CreateMock<T>() where T : class;
    Mock<T> CreateMock<T>(MockBehavior behavior) where T : class;
}
```

---

## Attributes & Patterns

### Test Methods

TUnit discovers test methods by convention – `public async Task` or `public async ValueTask` methods:

```csharp
// ✅ Discovered automatically
public async Task Add_WithTwoNumbers_ReturnsSum()
{
    var result = await _calculator.AddAsync(5, 3);
    Assert.Equal(8, result);
}

// ✅ Also valid (ValueTask variant)
public async ValueTask Multiply_WithTwoNumbers_ReturnsProduct()
{
    var result = await _calculator.MultiplyAsync(4, 5);
    Assert.Equal(20, result);
}

// ✅ Async methods with parameters (parameterized tests)
public async Task FetchUser_WithVariousIds(int id)
{
    var user = await _userService.GetUserAsync(id);
    Assert.NotNull(user);
}
```

### Parameterized Tests

TUnit discovers parameter data automatically:

```csharp
public async Task Add_WithVariousInputs(int a, int b, int expected)
{
    var result = _calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

To provide test data, override:

```csharp
public static IEnumerable<object[]> AddTestData =>
    new List<object[]>
    {
        new object[] { 2, 3, 5 },
        new object[] { 0, 0, 0 },
        new object[] { -1, 1, 0 },
    };

[Parameters(nameof(AddTestData))]
public async Task Add_WithMemberData(int a, int b, int expected)
{
    var result = _calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

### Traits and Categories

Use method attributes for test organization:

```csharp
[Trait("Category", "Unit")]
[Trait("Speed", "Fast")]
public async Task QuickTest()
{
    Assert.True(true);
}

// Run: dotnet test --filter "Category=Unit&Speed=Fast"
```

### Skip Tests

Skip tests using `[Skip]` attribute:

```csharp
[Skip("Not yet implemented")]
public async Task UnfinishedFeature()
{
    // Skipped
}
```

---

## Working Examples

### Example 1: Basic Async Test

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class CalculatorTests : PeasyPilotTUnitTestBase
{
    private Calculator _calculator = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _calculator = new Calculator();
    }
    
    public async Task Add_WithPositiveNumbers_ReturnsSum()
    {
        int result = _calculator.Add(5, 3);
        await Assert.That(result).IsEqualTo(8);
    }
    
    public async Task Subtract_WithPositiveNumbers_ReturnsDifference()
    {
        int result = _calculator.Subtract(5, 3);
        await Assert.That(result).IsEqualTo(2);
    }
}
```

### Example 2: Parameterized Tests

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class MathOperationsTests : PeasyPilotTUnitTestBase
{
    private Calculator _calculator = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _calculator = new Calculator();
    }
    
    public static IEnumerable<object[]> AddTestData =>
        new[]
        {
            new object[] { 2, 3, 5 },
            new object[] { 0, 0, 0 },
            new object[] { -1, 1, 0 },
            new object[] { 100, 50, 150 },
        };
    
    [Parameters(nameof(AddTestData))]
    public async Task Add_WithVariousInputs_ReturnsCorrectSum(int a, int b, int expected)
    {
        int result = _calculator.Add(a, b);
        await Assert.That(result).IsEqualTo(expected);
    }
}
```

### Example 3: Async Operations with Await

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class AsyncOperationTests : PeasyPilotTUnitTestBase
{
    private AsyncService _service = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _service = new AsyncService();
        await _service.InitializeAsync();
    }
    
    public async Task FetchData_WithValidId_ReturnsData()
    {
        var result = await _service.FetchDataAsync(1);
        
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Name).IsEqualTo("data");
    }
    
    public async Task FetchMultipleUsers_WithIds_ReturnsAll()
    {
        var results = await _service.FetchUsersAsync(new[] { 1, 2, 3 });
        
        await Assert.That(results).HasCount(3);
        await Assert.That(results).AllSatisfy(r => Assert.That(r).IsNotNull());
    }
}
```

### Example 4: Using Test Context

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class ContextSharingTests : PeasyPilotTUnitTestBase
{
    public async Task StoreData_InContext()
    {
        var user = new User { Id = 1, Name = "Alice" };
        TestContext.Set("user", user);
        
        var retrieved = TestContext.Get<string, User>("user");
        await Assert.That(retrieved).IsNotNull();
        await Assert.That(retrieved.Name).IsEqualTo("Alice");
    }
    
    public async Task GetOrCreate_CachesData()
    {
        var user = TestContext.GetOrAdd(
            "cached_user",
            () => new User { Id = 2, Name = "Bob" }
        );
        
        var second = TestContext.GetOrAdd(
            "cached_user",
            () => new User { Id = 3, Name = "Charlie" }
        );
        
        await Assert.That(user).IsTheSameAs(second);
    }
}
```

### Example 5: Database Testing with Async Lifecycle

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class DatabaseTests : PeasyPilotTUnitTestBase
{
    private Database _db = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _db = new Database("Server=test;Database=testdb");
        await _db.ConnectAsync();
        await _db.InitializeSchemaAsync();
    }
    
    public override async ValueTask AfterEachAsync()
    {
        await _db.CleanupAsync();
        await _db.DisconnectAsync();
        await base.AfterEachAsync();
    }
    
    public async Task InsertUser_WithValidData_Succeeds()
    {
        var repo = new UserRepository(_db);
        var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
        
        await repo.InsertAsync(user);
        
        var retrieved = await repo.GetAsync(user.Id);
        await Assert.That(retrieved).IsNotNull();
        await Assert.That(retrieved.Name).IsEqualTo(user.Name);
    }
    
    public async Task QueryUsers_WithFilters_ReturnsMatching()
    {
        var repo = new UserRepository(_db);
        await repo.InsertAsync(new User { Id = Guid.NewGuid(), Name = "Alice" });
        await repo.InsertAsync(new User { Id = Guid.NewGuid(), Name = "Bob" });
        
        var results = await repo.QueryAsync(u => u.Name.StartsWith("A"));
        
        await Assert.That(results).HasCount(1);
        await Assert.That(results[0].Name).IsEqualTo("Alice");
    }
}
```

### Example 6: Exception Testing

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class ExceptionTests : PeasyPilotTUnitTestBase
{
    private Calculator _calculator = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _calculator = new Calculator();
    }
    
    public async Task Divide_ByZero_ThrowsArgumentException()
    {
        var ex = await Assert.That(
            () => _calculator.Divide(10, 0)
        ).Throws<ArgumentException>();
        
        await Assert.That(ex.ParamName).IsEqualTo("divisor");
    }
    
    public async Task AsyncOperation_OnError_ThrowsException()
    {
        var service = new FailingService();
        
        await Assert.That(
            () => service.FailAsync()
        ).ThrowsAsync<InvalidOperationException>();
    }
}
```

### Example 7: Test Data Factory with Async

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class FactoryPatternTests : PeasyPilotTUnitTestBase
{
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        TestDataFactory = new UserFactory();
    }
    
    public async Task CreateUser_WithFactory_GeneratesValidData()
    {
        var user = TestDataFactory?.Create<User>();
        
        await Assert.That(user).IsNotNull();
        await Assert.That(user.Id).IsNotNull();
        await Assert.That(user.Name).IsNotEmpty();
    }
    
    public async Task CreateMultipleUsers_WithFactory()
    {
        var users = Enumerable.Range(0, 5)
            .Select(_ => TestDataFactory?.Create<User>())
            .ToList();
        
        await Assert.That(users).HasCount(5);
        await Assert.That(users).AllSatisfy(u => Assert.That(u).IsNotNull());
    }
}

public class UserFactory : ITestDataFactory
{
    public T Create<T>() where T : class, new()
    {
        if (typeof(T) == typeof(User))
            return (new User { Id = Guid.NewGuid(), Name = "TestUser" } as T)!;
        return new T();
    }
    
    public T Create<T>(Action<T> configure) where T : class, new()
    {
        var instance = Create<T>();
        configure(instance);
        return instance;
    }
}
```

### Example 8: Parallel Test Execution

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class ParallelTests : PeasyPilotTUnitTestBase
{
    // TUnit runs these tests in parallel by default
    
    public async Task Test_One()
    {
        await Task.Delay(100);
        await Assert.That(true).IsTrue();
    }
    
    public async Task Test_Two()
    {
        await Task.Delay(100);
        await Assert.That(true).IsTrue();
    }
    
    public async Task Test_Three()
    {
        await Task.Delay(100);
        await Assert.That(true).IsTrue();
    }
    
    // All three run concurrently, total time ~100ms instead of ~300ms
}
```

### Example 9: Async Assertions Chain

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class ChainedAssertionsTests : PeasyPilotTUnitTestBase
{
    private UserService _service = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _service = new UserService();
    }
    
    public async Task CreateUser_WithValidData_CreatesAndReturnsUser()
    {
        var command = new CreateUserCommand 
        { 
            Email = "test@example.com", 
            Name = "Test User" 
        };
        
        var result = await _service.CreateAsync(command);
        
        await Assert.That(result)
            .IsNotNull()
            .And(r => r.Id, id => id.IsNotEqualTo(Guid.Empty))
            .And(r => r.Email, email => email.IsEqualTo(command.Email))
            .And(r => r.Name, name => name.IsEqualTo(command.Name));
    }
}
```

### Example 10: Skip Tests Conditionally

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class ConditionalSkipTests : PeasyPilotTUnitTestBase
{
    [Skip("Feature not yet implemented")]
    public async Task UnfinishedFeature_ShouldBeSkipped()
    {
        // Skipped with reason
    }
    
    public async Task SkipIf_Condition()
    {
        if (Environment.ProcessorCount < 4)
        {
            Assert.Skip("Test requires at least 4 cores");
        }
        
        // Test logic
        await Assert.That(true).IsTrue();
    }
}
```

---

## Best Practices

### 1. Always Await BeforeEachAsync/AfterEachAsync

```csharp
public override async ValueTask BeforeEachAsync()
{
    await base.BeforeEachAsync();  // ✅ Required
    // Your initialization here
}

public override async ValueTask AfterEachAsync()
{
    // Your cleanup here
    await base.AfterEachAsync();  // ✅ Required
}
```

### 2. Prefer ValueTask for Lightweight Operations

```csharp
// ✅ GOOD: ValueTask for zero-allocation scenarios
public override async ValueTask BeforeEachAsync()
{
    await base.BeforeEachAsync();
    _data = new Data();
}

// Still valid but less optimal:
public override async Task BeforeEachAsync()
{
    await base.BeforeEachAsync();
    _data = new Data();
}
```

### 3. Name Tests Clearly

```csharp
// ✅ GOOD: Clear intent
public async Task Add_WithPositiveNumbers_ReturnsSum() { }

// ❌ POOR: Vague
public async Task TestAdd() { }
```

### 4. Embrace Async-First Design

```csharp
// ✅ GOOD: Async-first
public async Task FetchData_WithValidId_ReturnsData()
{
    var result = await _service.FetchDataAsync(id);
    await Assert.That(result).IsNotNull();
}

// ❌ AVOID: Mixing sync operations
public async Task FetchData_WithValidId_ReturnsData()
{
    var result = _service.FetchData(id);  // Sync call blocks
    await Assert.That(result).IsNotNull();
}
```

### 5. Leverage Parallelization

TUnit runs tests in parallel by default – write tests that are independent:

```csharp
// ✅ GOOD: Independent tests (run in parallel)
public async Task Test_One() { }
public async Task Test_Two() { }
public async Task Test_Three() { }

// ❌ AVOID: Shared mutable state
private int _counter = 0;

public async Task Increment_Test()
{
    _counter++;  // Race condition
}
```

### 6. Use Try-Finally for Critical Cleanup

```csharp
public override async ValueTask BeforeEachAsync()
{
    await base.BeforeEachAsync();
    try
    {
        await _resource.AcquireAsync();
    }
    catch
    {
        await AfterEachAsync();
        throw;
    }
}
```

### 7. Keep Tests Focused

```csharp
// ✅ GOOD: One behavior per test
public async Task Add_WithPositiveNumbers_ReturnsSum()
{
    var result = _calculator.Add(5, 3);
    await Assert.That(result).IsEqualTo(8);
}

// ❌ AVOID: Multiple behaviors
public async Task Calculator_Works()
{
    var add = _calculator.Add(5, 3);
    var sub = _calculator.Subtract(5, 3);
    var mul = _calculator.Multiply(4, 5);
    // Tests multiple behaviors
}
```

---

## See Also

- [TUnit Documentation](https://thomhurst.github.io/tunit/)
- [PeasyPilot.Core API](api-core.md)
- [Framework Adapters Guide](../GUIDES/framework-adapters-guide.md)
- [Unit Testing Guide](../GUIDES/unit-testing-guide.md)

---

**Last Updated:** 2026-09-11  
**Version:** 1.0  
[← Back to REFERENCE](README.md)
