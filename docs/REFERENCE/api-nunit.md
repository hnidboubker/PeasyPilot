# PeasyPilot.NUnit API Reference

Complete API reference for the `PeasyPilot.NUnit` adapter package.

**Target Frameworks:** .NET 8, 9, 10  
**Package:** [PeasyPilot.NUnit on NuGet](https://www.nuget.org/packages/PeasyPilot.NUnit)  
**Repository:** [GitHub: PeasyPilot](https://github.com/houssinedev/PeasyPilot)

---

## Table of Contents

1. [Overview](#overview)
2. [Main Abstractions](#main-abstractions)
3. [Setup/Teardown Lifecycle](#setupteardown-lifecycle)
4. [Key Classes & Members](#key-classes--members)
5. [Attributes & Patterns](#attributes--patterns)
6. [Working Examples](#working-examples)
7. [Best Practices](#best-practices)

---

## Overview

The `PeasyPilot.NUnit` adapter integrates NUnit 3.x with PeasyPilot's test infrastructure. It provides:

- **Synchronous and asynchronous support** with `[SetUp]` and `[TearDown]` attributes
- **Test context management** for sharing state within a test
- **TestFixture pattern** for organizing related tests
- **Mock and data factory integration** for dependency injection patterns
- **Seamless integration** with PeasyPilot.Core features
- **Enterprise-friendly** approach familiar to NUnit users

### When to Choose NUnit

Use PeasyPilot.NUnit when:
- Your project uses NUnit 3.x
- You have a team familiar with NUnit conventions
- You're working with legacy or enterprise projects
- You need both sync and async test support
- You prefer explicit [SetUp] and [TearDown] lifecycle hooks

### Installation

```bash
dotnet add package PeasyPilot.NUnit
```

---

## Main Abstractions

### PeasyPilotNUnitTestBase

**Namespace:** `PeasyPilot.NUnit`

Base class for NUnit test classes. Provides `[SetUp]` and `[TearDown]` lifecycle hooks.

#### Declaration

```csharp
public abstract class PeasyPilotNUnitTestBase
{
    protected ITestContext TestContext { get; private set; }
    protected ITestDataFactory? TestDataFactory { get; set; }
    protected IMockFactory? MockFactory { get; set; }
    
    [SetUp]
    public virtual void Setup();
    
    [TearDown]
    public virtual void TearDown();
    
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
| `Setup()` | void | Synchronous setup before each test |
| `TearDown()` | void | Synchronous cleanup after each test |
| `GetOrCreateTestData<T>()` | T | Get or create cached test data by key |

### NUnitAdapter

**Namespace:** `PeasyPilot.NUnit`

Adapter implementing `ITestFrameworkAdapter` for NUnit discovery and execution.

#### Declaration

```csharp
public sealed class NUnitAdapter : ITestFrameworkAdapter
{
    public string Name { get; }
    public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default);
    public Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default);
}
```

#### Properties

| Property | Returns | Description |
|----------|---------|-------------|
| `Name` | string | Always returns `"NUnit"` |

#### Methods

| Method | Returns | Purpose |
|--------|---------|---------|
| `DiscoverAsync(CancellationToken)` | `Task<IReadOnlyCollection<TestCase>>` | Discover test cases in NUnit assemblies |
| `RunAsync(TestRunRequest, CancellationToken)` | `Task<TestRunResult>` | Execute test runs with detailed results |

---

## Setup/Teardown Lifecycle

NUnit uses `[SetUp]` and `[TearDown]` attributes for test lifecycle management.

### Execution Order

```
For Each Test:
  1. Test class instantiated
  2. All [OneTimeSetUp] methods run (if any)
  3. [SetUp] method called
  4. [Test] or [TestCase] method executes
  5. [TearDown] method called
  6. (After all tests: [OneTimeTearDown])
```

### Example: Basic Lifecycle

```csharp
[TestFixture]
public class LifecycleTests : PeasyPilotNUnitTestBase
{
    private Calculator _calculator = null!;
    
    // Called before each test
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _calculator = new Calculator();
    }
    
    // Called after each test
    [TearDown]
    public override void TearDown()
    {
        // Cleanup if needed
        base.TearDown();
    }
    
    [Test]
    public void Add_WithTwoNumbers_ReturnsSum()
    {
        var result = _calculator.Add(5, 3);
        Assert.That(result, Is.EqualTo(8));
    }
}
```

### Key Points

- **Always call `base.Setup()`** to ensure `TestContext` is initialized
- **[SetUp] runs before each test** – perfect for per-test initialization
- **[TearDown] runs after each test** – even if the test fails
- **[OneTimeSetUp]** runs once per test fixture class (not per test)
- **[OneTimeTearDown]** runs once after all tests in the fixture
- **Use try-finally** for critical cleanup:

```csharp
[TearDown]
public override void TearDown()
{
    try
    {
        // Critical cleanup
        _resource?.Dispose();
    }
    finally
    {
        base.TearDown();
    }
}
```

### One-Time Setup Example

```csharp
[TestFixture]
public class OneTimeSetupExample : PeasyPilotNUnitTestBase
{
    private static Database _sharedDb = null!;
    
    // Runs once before all tests in this fixture
    [OneTimeSetUp]
    public static void OneTimeSetup()
    {
        _sharedDb = new Database();
        _sharedDb.Connect();
        _sharedDb.InitializeSchema();
    }
    
    // Runs before each test
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        // Clear data before each test
        _sharedDb.ClearAllTables();
    }
    
    // Runs once after all tests in this fixture
    [OneTimeTearDown]
    public static void OneTimeTearDown()
    {
        _sharedDb?.Disconnect();
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

### Standard NUnit Attributes

PeasyPilot.NUnit works seamlessly with NUnit's built-in attributes:

#### [TestFixture]

Marks a class as containing tests.

```csharp
[TestFixture]
public class CalculatorTests : PeasyPilotNUnitTestBase
{
    // Tests here
}
```

#### [Test]

Marks a parameterless test method.

```csharp
[Test]
public void Add_WithTwoNumbers_ReturnsSum()
{
    Assert.That(_calculator.Add(5, 3), Is.EqualTo(8));
}
```

#### [TestCase] with Multiple Data Sets

Marks a parameterized test.

```csharp
[TestCase(1, 2, 3)]
[TestCase(5, 5, 10)]
[TestCase(-1, 1, 0)]
public void Add_WithVariousInputs_ReturnsSum(int a, int b, int expected)
{
    var calc = new Calculator();
    Assert.That(calc.Add(a, b), Is.EqualTo(expected));
}
```

#### [Category]

Categorize tests for filtering.

```csharp
[Test]
[Category("Unit")]
[Category("Fast")]
public void QuickTest()
{
    Assert.That(true, Is.True);
}

// Run: dotnet test --filter "Category=Unit&Category=Fast"
```

### Assertion Patterns

NUnit's fluent assertion syntax:

```csharp
// Equality
Assert.That(result, Is.EqualTo(8));

// Null checks
Assert.That(obj, Is.Null);
Assert.That(obj, Is.Not.Null);

// Collections
Assert.That(list, Is.Empty);
Assert.That(list, Is.Not.Empty);
Assert.That(list, Contains.Item(5));

// Strings
Assert.That(text, Does.Contain("hello"));
Assert.That(text, Does.StartWith("the"));
Assert.That(text, Does.EndWith("end"));

// Numbers
Assert.That(value, Is.GreaterThan(10));
Assert.That(value, Is.LessThan(100));
Assert.That(value, Is.InRange(1, 10));

// Type checks
Assert.That(obj, Is.TypeOf<User>());
Assert.That(obj, Is.InstanceOf<IRepository>());
```

### Multiple Assertions

```csharp
[Test]
public void User_WithValidData_HasRequiredProperties()
{
    var user = new User { Id = 1, Name = "Alice" };
    
    Assert.Multiple(() =>
    {
        Assert.That(user.Id, Is.Not.EqualTo(0));
        Assert.That(user.Name, Is.Not.Null);
        Assert.That(user.Name, Does.StartWith("A"));
    });
}
```

---

## Working Examples

### Example 1: Basic TestFixture

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class CalculatorTests : PeasyPilotNUnitTestBase
{
    private Calculator _calculator = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _calculator = new Calculator();
    }
    
    [Test]
    public void Add_WithPositiveNumbers_ReturnsSum()
    {
        int result = _calculator.Add(5, 3);
        Assert.That(result, Is.EqualTo(8));
    }
    
    [Test]
    public void Subtract_WithPositiveNumbers_ReturnsDifference()
    {
        int result = _calculator.Subtract(5, 3);
        Assert.That(result, Is.EqualTo(2));
    }
}
```

### Example 2: TestCase with Multiple Data Sets

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class MathOperationsTests : PeasyPilotNUnitTestBase
{
    private Calculator _calculator = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _calculator = new Calculator();
    }
    
    [TestCase(2, 3, 5)]
    [TestCase(0, 0, 0)]
    [TestCase(-1, 1, 0)]
    [TestCase(100, 50, 150)]
    public void Add_WithVariousInputs_ReturnsCorrectSum(int a, int b, int expected)
    {
        int result = _calculator.Add(a, b);
        Assert.That(result, Is.EqualTo(expected));
    }
}
```

### Example 3: Using Test Context for Shared State

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class ContextSharingTests : PeasyPilotNUnitTestBase
{
    [Test]
    public void FirstTest_StoresData()
    {
        var user = new User { Id = 1, Name = "Alice" };
        TestContext.Set("user", user);
        
        var retrieved = TestContext.Get<string, User>("user");
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved.Name, Is.EqualTo("Alice"));
    }
    
    [Test]
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
        Assert.That(user, Is.SameAs(second));
    }
}
```

### Example 4: Test Data Factory Pattern

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class FactoryPatternTests : PeasyPilotNUnitTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        TestDataFactory = new UserFactory();
    }
    
    [Test]
    public void CreateUser_WithFactory_GeneratesValidData()
    {
        var user = TestDataFactory?.Create<User>();
        
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Id, Is.Not.Null);
        Assert.That(user.Name, Is.Not.Empty);
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

### Example 5: Exception Testing

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class ExceptionTests : PeasyPilotNUnitTestBase
{
    private Calculator _calculator = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _calculator = new Calculator();
    }
    
    [Test]
    public void Divide_ByZero_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => _calculator.Divide(10, 0));
        Assert.That(ex.ParamName, Is.EqualTo("divisor"));
    }
    
    [Test]
    public void NullArgument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _calculator.Multiply(null));
    }
}
```

### Example 6: OneTimeSetUp and OneTimeTearDown

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class DatabaseTests : PeasyPilotNUnitTestBase
{
    private static Database _db = null!;
    
    [OneTimeSetUp]
    public static void SetupDatabase()
    {
        _db = new Database("Server=test;Database=testdb");
        _db.Connect();
        _db.InitializeSchema();
    }
    
    [OneTimeTearDown]
    public static void TeardownDatabase()
    {
        _db?.Cleanup();
        _db?.Disconnect();
    }
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _db.ClearAllTables();
    }
    
    [Test]
    public void InsertUser_WithValidData_Succeeds()
    {
        var repo = new UserRepository(_db);
        var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
        
        repo.Insert(user);
        
        var retrieved = repo.Get(user.Id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved.Name, Is.EqualTo(user.Name));
    }
}
```

### Example 7: Multiple Assertions Pattern

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class UserServiceTests : PeasyPilotNUnitTestBase
{
    private UserService _service = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _service = new UserService();
    }
    
    [Test]
    [Category("Integration")]
    public void CreateUser_WithValidData_CreatesAndReturnsUser()
    {
        var command = new CreateUserCommand 
        { 
            Email = "test@example.com", 
            Name = "Test User" 
        };
        
        var result = _service.Create(command);
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Email, Is.EqualTo(command.Email));
            Assert.That(result.Name, Is.EqualTo(command.Name));
        });
    }
}
```

### Example 8: Ignore Attribute for Pending Tests

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class PendingFeatureTests : PeasyPilotNUnitTestBase
{
    [Test]
    [Ignore("Feature not yet implemented")]
    public void UnfinishedFeature_ShouldBeIgnored()
    {
        // This test will be skipped and reported separately
    }
    
    [TestCase(1)]
    [TestCase(2)]
    [Ignore("Awaiting API response")]
    public void ExternalApiTests_IgnoreAll(int id)
    {
        // All test cases ignored
    }
}
```

### Example 9: Category Filtering

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
[Category("Integration")]
public class IntegrationTests : PeasyPilotNUnitTestBase
{
    [Test]
    [Category("Database")]
    public void DbConnection_Succeeds()
    {
        Assert.That(true, Is.True);
    }
    
    [Test]
    [Category("Api")]
    public void ApiCall_Succeeds()
    {
        Assert.That(true, Is.True);
    }
}

// Run only Database tests: dotnet test --filter "Category=Database"
// Run only Integration: dotnet test --filter "Category=Integration"
```

### Example 10: Async Tests with NUnit

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class AsyncTests : PeasyPilotNUnitTestBase
{
    private AsyncService _service = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _service = new AsyncService();
    }
    
    [Test]
    public async Task FetchData_WithValidId_ReturnsData()
    {
        var result = await _service.FetchDataAsync(1);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("data"));
    }
    
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task FetchUsers_WithVariousIds_ReturnsResults(int id)
    {
        var result = await _service.FetchUserAsync(id);
        Assert.That(result, Is.Not.Null);
    }
}
```

---

## Best Practices

### 1. Always Call Base Methods

```csharp
[SetUp]
public override void Setup()
{
    base.Setup();  // ✅ Required
    // Your initialization here
}

[TearDown]
public override void TearDown()
{
    // Your cleanup here
    base.TearDown();  // ✅ Required
}
```

### 2. Use [TestFixture] Attribute

```csharp
// ✅ GOOD: Explicitly marks test class
[TestFixture]
public class CalculatorTests : PeasyPilotNUnitTestBase { }

// ❌ AVOID: Missing fixture declaration (may cause issues)
public class CalculatorTests : PeasyPilotNUnitTestBase { }
```

### 3. Name Tests Clearly

```csharp
// ✅ GOOD: Clear intent
[Test]
public void Add_WithPositiveNumbers_ReturnsSum() { }

// ❌ POOR: Vague
[Test]
public void TestAdd() { }
```

### 4. Use OneTimeSetUp for Expensive Resources

```csharp
// ✅ GOOD: Shared expensive resource
[OneTimeSetUp]
public static void SetupDatabase()
{
    _db = new Database();
    _db.Connect();  // Expensive operation, done once
}

// ❌ AVOID: Per-test expensive resource
[SetUp]
public override void Setup()
{
    base.Setup();
    _db = new Database();  // Called before each test
    _db.Connect();
}
```

### 5. Clean State Between Tests

```csharp
[SetUp]
public override void Setup()
{
    base.Setup();
    // Clear data before each test
    _db.ClearAllTables();
}
```

### 6. Use Multiple Assertions Carefully

```csharp
// ✅ GOOD: Use Assert.Multiple for related assertions
[Test]
public void User_HasRequiredProperties()
{
    var user = new User { Id = 1, Name = "Alice" };
    
    Assert.Multiple(() =>
    {
        Assert.That(user.Id, Is.Not.EqualTo(0));
        Assert.That(user.Name, Is.Not.Null);
    });
}

// ❌ AVOID: Testing multiple unrelated behaviors
[Test]
public void AllFeatures_Work()
{
    Assert.That(_calc.Add(2, 2), Is.EqualTo(4));
    Assert.That(_calc.Multiply(3, 3), Is.EqualTo(9));
    Assert.That(_calc.Divide(10, 2), Is.EqualTo(5));
}
```

### 7. Cleanup Always Runs

Remember that `[TearDown]` executes even if the test fails:

```csharp
[TearDown]
public override void TearDown()
{
    try
    {
        // Cleanup happens regardless of test outcome
        _resource?.Dispose();
    }
    finally
    {
        base.TearDown();
    }
}
```

---

## See Also

- [NUnit Documentation](https://docs.nunit.org/)
- [PeasyPilot.Core API](api-core.md)
- [Framework Adapters Guide](../GUIDES/framework-adapters-guide.md)
- [Unit Testing Guide](../GUIDES/unit-testing-guide.md)

---

**Last Updated:** 2026-09-11  
**Version:** 1.0  
[← Back to REFERENCE](README.md)
