# Framework Adapters Guide

## Overview

PeasyPilot works with three test frameworks: **xUnit**, **NUnit**, and **TUnit**. Choose based on your team's preference.

**Time:** 15 minutes  

---

## Comparison

| Feature | xUnit | NUnit | TUnit |
|---------|-------|-------|-------|
| Async-native | ✅ | ✅ | ✅✅ |
| Setup/Teardown | `IAsyncLifetime` | `[SetUp]` | `Hooks` |
| Assertions | Custom | Built-in | Built-in |
| Parallelization | ✅ | ✅ | ✅✅ |
| Modern .NET | ✅✅ | ✅ | ✅✅ |

---

## xUnit (Recommended for Modern .NET)

**Best for:** Modern async-first projects

```csharp
using Xunit;
using PeasyPilot.XUnit;

public class CalculatorTests : PeasyPilotTestBase
{
    private Calculator _calculator = null!;
    
    // Async initialization (IAsyncLifetime)
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _calculator = new Calculator();
    }
    
    [Fact]
    public void Add_ReturnSum() => Assert.Equal(8, _calculator.Add(5, 3));
    
    [Theory]
    [InlineData(5, 3, 8)]
    [InlineData(0, 0, 0)]
    public void Add_WithData(int a, int b, int expected) 
        => Assert.Equal(expected, _calculator.Add(a, b));
    
    [Fact]
    public async Task AsyncOperation_Completes()
    {
        await _calculator.InitializeAsync();
        Assert.NotNull(_calculator);
    }
}
```

**Install:**
```bash
dotnet add package PeasyPilot.XUnit
```

---

## NUnit (Familiar for Enterprise)

**Best for:** Legacy projects, teams familiar with NUnit

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

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
    public void Add_ReturnSum() => Assert.That(_calculator.Add(5, 3), Is.EqualTo(8));
    
    [TestCase(5, 3, 8)]
    [TestCase(0, 0, 0)]
    public void Add_WithData(int a, int b, int expected)
        => Assert.That(_calculator.Add(a, b), Is.EqualTo(expected));
    
    [Test]
    public async Task AsyncOperation_Completes()
    {
        await _calculator.InitializeAsync();
        Assert.That(_calculator, Is.Not.Null);
    }
}
```

**Install:**
```bash
dotnet add package PeasyPilot.NUnit
```

---

## TUnit (Most Modern)

**Best for:** Cutting-edge projects prioritizing performance

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

public class CalculatorTests : PeasyPilotTUnitTestBase
{
    private Calculator _calculator = null!;
    
    [Before(Test)]
    public async Task Setup()
    {
        await InitializeAsync();
        _calculator = new Calculator();
    }
    
    [Test]
    public async Task Add_ReturnSum()
    {
        var result = _calculator.Add(5, 3);
        await Assert.That(result).IsEqualTo(8);
    }
    
    [Test]
    [Arguments(5, 3, 8)]
    [Arguments(0, 0, 0)]
    public async Task Add_WithData(int a, int b, int expected)
    {
        var result = _calculator.Add(a, b);
        await Assert.That(result).IsEqualTo(expected);
    }
}
```

**Install:**
```bash
dotnet add package PeasyPilot.TUnit
```

---

## Lifecycle Comparison

### xUnit (IAsyncLifetime)
```csharp
public override async Task InitializeAsync()  // Runs before each test
{
    await base.InitializeAsync();
    // Setup
}

public override async Task DisposeAsync()     // Runs after each test
{
    // Cleanup
    await base.DisposeAsync();
}
```

### NUnit ([SetUp] / [TearDown])
```csharp
[SetUp]
public override void Setup()      // Runs before each test
{
    base.Setup();
    // Setup
}

[TearDown]
public override void TearDown()   // Runs after each test
{
    base.TearDown();
    // Cleanup
}
```

### TUnit (Hooks)
```csharp
[Before(Test)]
public async Task SetupAsync()    // Runs before each test
{
    // Setup (async)
}

[After(Test)]
public async Task CleanupAsync()  // Runs after each test
{
    // Cleanup (async)
}
```

---

## Assertion Differences

### xUnit
```csharp
Assert.Equal(expected, actual);
Assert.NotNull(value);
Assert.True(condition);
Assert.Throws<ArgumentException>(() => code);
```

### NUnit
```csharp
Assert.That(actual, Is.EqualTo(expected));
Assert.That(value, Is.Not.Null);
Assert.That(condition, Is.True);
Assert.Throws<ArgumentException>(() => code);
```

### TUnit
```csharp
await Assert.That(actual).IsEqualTo(expected);
await Assert.That(value).IsNotNull();
await Assert.That(condition).IsTrue();
await Assert.That(code).ThrowsAsync<ArgumentException>();
```

---

## Migration Between Frameworks

To switch from xUnit to NUnit:

1. **Change base class**
   ```csharp
   // From
   public class Tests : PeasyPilotTestBase
   
   // To
   public class Tests : PeasyPilotNUnitTestBase
   ```

2. **Change test attributes**
   ```csharp
   // From
   [Fact]
   
   // To
   [Test]
   ```

3. **Update lifecycle methods**
   ```csharp
   // From
   public override async Task InitializeAsync()
   
   // To
   [SetUp]
   public override void Setup()
   ```

4. **Update assertions**
   ```csharp
   // From
   Assert.Equal(8, result);
   
   // To
   Assert.That(result, Is.EqualTo(8));
   ```

---

## Best Practices

✅ **DO**
- Choose one framework per project
- Use framework-specific patterns consistently
- Leverage async features (all three support it)
- Use parameterized tests for multiple scenarios

❌ **DON'T**
- Mix frameworks in the same project
- Ignore framework-specific best practices
- Use synchronous patterns when async is available
- Forget to call `base.Setup()` / `base.InitializeAsync()`

---

## Choosing a Framework

**→ xUnit** if:
- Building modern greenfield projects
- Team prefers minimal setup, maximalist testing
- Using ASP.NET Core

**→ NUnit** if:
- Migrating legacy projects
- Team familiar with NUnit conventions
- Need extensive setup/teardown logic

**→ TUnit** if:
- Prioritizing test execution speed
- Want cutting-edge async patterns
- Building high-performance test suites

---

## Next Steps

📖 **[Unit Testing Guide](./unit-testing-guide.md)** – Write tests in your chosen framework  
📖 **[Integration Testing Guide](./integration-testing-guide.md)** – Integration patterns  
📖 **[BDD Testing Guide](./bdd-testing-guide.md)** – Behavior-driven testing  
📖 **[Test Generation Guide](./test-generation-guide.md)** – Auto-generate test cases  

All three frameworks work great with PeasyPilot! Pick your favorite. 🚀

---

**[← Back to Learning Guides](./README.md)** | **[← Back to Documentation Hub](../README.md)**

**Version:** English | **[Français](./framework-adapters-guide-FR.md)**
