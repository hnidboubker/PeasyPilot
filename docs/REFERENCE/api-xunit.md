# PeasyPilot.XUnit API Reference

Complete API reference for the `PeasyPilot.XUnit` adapter package.

**Target Frameworks:** .NET 8, 9, 10  
**Package:** [PeasyPilot.XUnit on NuGet](https://www.nuget.org/packages/PeasyPilot.XUnit)  
**Repository:** [GitHub: PeasyPilot](https://github.com/houssinedev/PeasyPilot)

---

## Table of Contents

1. [Overview](#overview)
2. [Main Abstractions](#main-abstractions)
3. [IAsyncLifetime Lifecycle](#iasynclifetime-lifecycle)
4. [Key Classes & Members](#key-classes--members)
5. [Attributes & Patterns](#attributes--patterns)
6. [Working Examples](#working-examples)
7. [Best Practices](#best-practices)

---

## Overview

The `PeasyPilot.XUnit` adapter integrates xUnit 2.x with PeasyPilot's test infrastructure. It provides:

- **Async-native base class** implementing `IAsyncLifetime`
- **Test context management** for sharing state within a test
- **Fixture pooling** via xUnit's collection fixtures
- **Mock and data factory integration** for dependency injection patterns
- **Seamless integration** with PeasyPilot.Core features

### When to Choose xUnit

Use PeasyPilot.XUnit when:
- Your project is modern (.NET 8+)
- You prefer async-first test design
- You want xUnit's discovery and execution model
- You need fluent, composable test attributes ([Theory], [InlineData], etc.)

### Installation

```bash
dotnet add package PeasyPilot.XUnit
```

---

## Main Abstractions

### PeasyPilotTestBase

**Namespace:** `PeasyPilot.XUnit`

Base class for xUnit test classes. Implements `Xunit.IAsyncLifetime` for async initialization and cleanup.

#### Declaration

```csharp
public abstract class PeasyPilotTestBase : IAsyncLifetime
{
    protected ITestContext TestContext { get; private set; }
    protected ITestDataFactory? TestDataFactory { get; set; }
    protected IMockFactory? MockFactory { get; set; }
    
    public virtual Task InitializeAsync();
    public virtual Task DisposeAsync();
    
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
| `InitializeAsync()` | Task | Async initialization before each test |
| `DisposeAsync()` | Task | Async cleanup after each test |
| `GetOrCreateTestData<T>()` | T | Get or create cached test data by key |

### XUnitAdapter

**Namespace:** `PeasyPilot.XUnit`

Adapter implementing `ITestFrameworkAdapter` for xUnit discovery and execution.

#### Declaration

```csharp
public sealed class XUnitAdapter : ITestFrameworkAdapter
{
    public string Name { get; }
    public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default);
    public Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default);
}
```

#### Properties

| Property | Returns | Description |
|----------|---------|-------------|
| `Name` | string | Always returns `"xUnit"` |

#### Methods

| Method | Returns | Purpose |
|--------|---------|---------|
| `DiscoverAsync(CancellationToken)` | `Task<IReadOnlyCollection<TestCase>>` | Discover test cases in xUnit assemblies |
| `RunAsync(TestRunRequest, CancellationToken)` | `Task<TestRunResult>` | Execute test runs with detailed results |

### PeasyPilotCollection

**Namespace:** `PeasyPilot.XUnit`

Collection definition for sharing `PeasyPilotTestBase` fixtures across test classes.

#### Declaration

```csharp
[CollectionDefinition("PeasyPilot Collection")]
public class PeasyPilotCollection : ICollectionFixture<PeasyPilotTestBase>
{
    // Marker class - no implementation needed
}
```

#### Usage

Assign test classes to the collection using the `[Collection]` attribute:

```csharp
[Collection("PeasyPilot Collection")]
public class MyTests : PeasyPilotTestBase
{
    // All tests in this class share PeasyPilotTestBase fixture
}
```

---

## IAsyncLifetime Lifecycle

xUnit's `IAsyncLifetime` provides async initialization and disposal hooks.

### Execution Order

```
For Each Test:
  1. Fixture instantiated
  2. InitializeAsync() called (await completes)
  3. [Fact] or [Theory] test method executes
  4. DisposeAsync() called (await completes)
  5. Fixture disposed
```

### Example: Lifecycle Hooks

```csharp
public class LifecycleTests : PeasyPilotTestBase
{
    private Database _db = null!;
    
    // Called before each test
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _db = new Database();
        await _db.ConnectAsync();
    }
    
    // Called after each test
    public override async Task DisposeAsync()
    {
        await _db.CloseAsync();
        await base.DisposeAsync();
    }
    
    [Fact]
    public async Task TestUsesDatabase()
    {
        var users = await _db.GetUsersAsync();
        Assert.NotEmpty(users);
    }
}
```

### Key Points

- **Always call `base.InitializeAsync()`** to ensure `TestContext` is initialized
- **Use `async/await`** – InitializeAsync/DisposeAsync are async
- **Exceptions in InitializeAsync** fail the test immediately
- **DisposeAsync always runs**, even if the test fails
- **Use try-finally** for critical cleanup:

```csharp
public override async Task InitializeAsync()
{
    await base.InitializeAsync();
    try
    {
        await _resource.AcquireAsync();
    }
    catch
    {
        await DisposeAsync();
        throw;
    }
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

### Standard xUnit Attributes

PeasyPilot.XUnit works seamlessly with xUnit's built-in attributes:

#### [Fact]

Marks a parameterless test method.

```csharp
[Fact]
public void TestMethod()
{
    Assert.True(true);
}
```

#### [Theory] + [InlineData]

Marks a parameterized test with inline data.

```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(5, 5, 10)]
[InlineData(-1, 1, 0)]
public void Add_WithVariousInputs_ReturnsSum(int a, int b, int expected)
{
    var calc = new Calculator();
    Assert.Equal(expected, calc.Add(a, b));
}
```

#### [Theory] + [MemberData]

Use test data from class members.

```csharp
public static IEnumerable<object[]> AddTestData =>
    new List<object[]>
    {
        new object[] { 2, 2, 4 },
        new object[] { 1, 1, 2 },
    };

[Theory]
[MemberData(nameof(AddTestData))]
public void Add_WithMemberData_ReturnsSum(int a, int b, int expected)
{
    var calc = new Calculator();
    Assert.Equal(expected, calc.Add(a, b));
}
```

#### [Trait]

Categorize tests for filtering.

```csharp
[Fact]
[Trait("Category", "Unit")]
[Trait("Speed", "Fast")]
public void QuickTest()
{
    Assert.True(true);
}

// Run: dotnet test --filter "Category=Unit&Speed=Fast"
```

### Collection Fixtures (Shared Instances)

Share a single fixture instance across multiple test classes.

```csharp
// Define the fixture
public class DatabaseFixture : IAsyncLifetime
{
    public Database Db { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        Db = new Database();
        await Db.ConnectAsync();
    }
    
    public async Task DisposeAsync()
    {
        await Db.CloseAsync();
    }
}

// Define the collection
[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}

// Use in tests
[Collection("Database Collection")]
public class UserRepositoryTests : PeasyPilotTestBase
{
    private readonly DatabaseFixture _fixture;
    
    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        var user = await _fixture.Db.GetUserAsync(1);
        Assert.NotNull(user);
    }
}
```

---

## Working Examples

### Example 1: Basic Test with Context

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class CalculatorTests : PeasyPilotTestBase
{
    private Calculator _calculator = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _calculator = new Calculator();
    }
    
    [Fact]
    public void Add_WithPositiveNumbers_ReturnsSum()
    {
        int result = _calculator.Add(5, 3);
        Assert.Equal(8, result);
    }
    
    [Fact]
    public void Subtract_WithPositiveNumbers_ReturnsDifference()
    {
        int result = _calculator.Subtract(5, 3);
        Assert.Equal(2, result);
    }
}
```

### Example 2: Theory with Multiple Data Sets

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class MathOperationsTests : PeasyPilotTestBase
{
    private Calculator _calculator = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _calculator = new Calculator();
    }
    
    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 0)]
    [InlineData(100, 50, 150)]
    public void Add_WithVariousInputs_ReturnsCorrectSum(int a, int b, int expected)
    {
        int result = _calculator.Add(a, b);
        Assert.Equal(expected, result);
    }
}
```

### Example 3: Async Test Method

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class AsyncOperationTests : PeasyPilotTestBase
{
    private AsyncService _service = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new AsyncService();
        await _service.InitializeAsync();
    }
    
    [Fact]
    public async Task FetchData_WithValidId_ReturnsData()
    {
        var result = await _service.FetchDataAsync(1);
        
        Assert.NotNull(result);
        Assert.Equal("data", result.Name);
    }
}
```

### Example 4: Using Test Context for Shared State

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class ContextSharingTests : PeasyPilotTestBase
{
    [Fact]
    public void FirstTest_StoresData()
    {
        var user = new User { Id = 1, Name = "Alice" };
        TestContext.Set("user", user);
        
        var retrieved = TestContext.Get<string, User>("user");
        Assert.NotNull(retrieved);
        Assert.Equal("Alice", retrieved.Name);
    }
    
    [Fact]
    public void GetOrCreate_CachesData()
    {
        var user = TestContext.GetOrAdd(
            "cached_user",
            () => new User { Id = 2, Name = "Bob" }
        );
        
        var second = TestContext.GetOrAdd(
            "cached_user",
            () => new User { Id = 3, Name = "Charlie" }
        );
        
        // Same instance returned
        Assert.Same(user, second);
    }
}
```

### Example 5: Test Data Factory Pattern

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class FactoryPatternTests : PeasyPilotTestBase
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        TestDataFactory = new UserFactory();
    }
    
    [Fact]
    public void CreateUser_WithFactory_GeneratesValidData()
    {
        var user = TestDataFactory?.Create<User>();
        
        Assert.NotNull(user);
        Assert.NotNull(user.Id);
        Assert.NotEmpty(user.Name);
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

### Example 6: Exception Assertion

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class ExceptionTests : PeasyPilotTestBase
{
    private Calculator _calculator = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _calculator = new Calculator();
    }
    
    [Fact]
    public void Divide_ByZero_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => _calculator.Divide(10, 0));
        Assert.Equal("divisor", ex.ParamName);
    }
    
    [Fact]
    public async Task AsyncOperation_OnError_ThrowsException()
    {
        var service = new FailingService();
        
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.FailAsync()
        );
    }
}
```

### Example 7: Collection Fixture with Shared Database

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class DatabaseFixture : IAsyncLifetime
{
    public Database Db { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        Db = new Database("Server=test;Database=testdb");
        await Db.ConnectAsync();
        await Db.InitializeSchemaAsync();
    }
    
    public async Task DisposeAsync()
    {
        await Db.CleanupAsync();
        await Db.DisconnectAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollectionDefinition : ICollectionFixture<DatabaseFixture>
{
}

[Collection("Database")]
public class UserRepositoryTests : PeasyPilotTestBase
{
    private readonly DatabaseFixture _dbFixture;
    
    public UserRepositoryTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }
    
    [Fact]
    public async Task InsertUser_WithValidData_SucceedsAsync()
    {
        var repo = new UserRepository(_dbFixture.Db);
        var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
        
        await repo.InsertAsync(user);
        
        var retrieved = await repo.GetAsync(user.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(user.Name, retrieved.Name);
    }
}
```

### Example 8: Multiple Assertions with Traits

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class UserServiceTests : PeasyPilotTestBase
{
    private UserService _service = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new UserService();
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Speed", "Slow")]
    public async Task CreateUser_WithValidData_CreatesAndReturnsUser()
    {
        var command = new CreateUserCommand 
        { 
            Email = "test@example.com", 
            Name = "Test User" 
        };
        
        var result = await _service.CreateAsync(command);
        
        Assert.NotNull(result);
        Assert.True(result.Id != Guid.Empty);
        Assert.Equal(command.Email, result.Email);
        Assert.Equal(command.Name, result.Name);
    }
}
```

### Example 9: Skip Attribute for Pending Tests

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class PendingFeatureTests : PeasyPilotTestBase
{
    [Fact(Skip = "Feature not yet implemented")]
    public void UnfinishedFeature_ShouldBeSkipped()
    {
        // This test will be skipped and reported separately
    }
    
    [Theory(Skip = "Awaiting API response")]
    [InlineData(1)]
    [InlineData(2)]
    public void ExternalApiTests_SkipAll(int id)
    {
        // All theory variations skipped
    }
}
```

### Example 10: Custom Assertions with Extension Methods

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class ExtensionMethodTests : PeasyPilotTestBase
{
    [Fact]
    public void User_WithExtension_Assertions()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" };
        
        user.ShouldBeValid();
        user.Name.ShouldNotBeNullOrEmpty();
        user.Email.ShouldContain("@");
    }
}

public static class UserAssertions
{
    public static void ShouldBeValid(this User user)
    {
        Assert.NotNull(user);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.NotEmpty(user.Name);
    }
    
    public static void ShouldNotBeNullOrEmpty(this string? value)
    {
        Assert.NotNull(value);
        Assert.NotEmpty(value);
    }
}
```

---

## Best Practices

### 1. Always Call Base Methods

```csharp
public override async Task InitializeAsync()
{
    await base.InitializeAsync();  // ✅ Required
    // Your initialization here
}

public override async Task DisposeAsync()
{
    // Your cleanup here
    await base.DisposeAsync();  // ✅ Required
}
```

### 2. Use Async/Await for I/O Operations

```csharp
// ✅ GOOD: Async initialization
public override async Task InitializeAsync()
{
    await base.InitializeAsync();
    _db = new Database();
    await _db.ConnectAsync();  // Async operation
}

// ❌ AVOID: Blocking calls
public override async Task InitializeAsync()
{
    await base.InitializeAsync();
    _db = new Database();
    _db.Connect();  // Sync call blocks the async pipeline
}
```

### 3. Name Tests Clearly

```csharp
// ✅ GOOD: Clear intent
[Fact]
public void Add_WithPositiveNumbers_ReturnsSum() { }

// ❌ POOR: Vague
[Fact]
public void TestAdd() { }
```

### 4. One Assertion Focus Per Test

```csharp
// ✅ GOOD: Focused
[Fact]
public void Add_WithTwoNumbers_ReturnsSum()
{
    var result = _calculator.Add(5, 3);
    Assert.Equal(8, result);
}

// ❌ AVOID: Multiple behaviors
[Fact]
public void Calculator_Works()
{
    Assert.Equal(8, _calculator.Add(5, 3));
    Assert.Equal(2, _calculator.Subtract(5, 3));
    Assert.Throws<Exception>(() => _calculator.Divide(1, 0));
}
```

### 5. Use Fixtures for Expensive Resources

```csharp
// ✅ GOOD: Shared fixture for expensive resource
[Collection("Database")]
public class RepositoryTests : PeasyPilotTestBase
{
    private readonly DatabaseFixture _db;
    public RepositoryTests(DatabaseFixture db) => _db = db;
}

// ❌ AVOID: Creating new database per test
public class RepositoryTests : PeasyPilotTestBase
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var db = new Database();  // Expensive operation per test
        await db.ConnectAsync();
    }
}
```

### 6. Cleanup Always Runs

Remember that `DisposeAsync()` executes even if the test fails. Use it for critical cleanup:

```csharp
public override async Task DisposeAsync()
{
    try
    {
        // Cleanup happens regardless of test outcome
        await _resource.ReleaseAsync();
    }
    finally
    {
        await base.DisposeAsync();
    }
}
```

---

## See Also

- [xUnit.net Documentation](https://xunit.net/)
- [PeasyPilot.Core API](api-core.md)
- [Framework Adapters Guide](../GUIDES/framework-adapters-guide.md)
- [Unit Testing Guide](../GUIDES/unit-testing-guide.md)

---

**Last Updated:** 2026-09-11  
**Version:** 1.0  
[← Back to REFERENCE](README.md)
