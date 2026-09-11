# Getting Started with PeasyPilot

Welcome! This guide will take you from zero to your first working test in about 25 minutes.

## What is PeasyPilot?

PeasyPilot is a modular .NET testing framework that lets you write unit tests, 
integration tests, and BDD-style tests using a consistent, clean API.

Think of it as a **unified testing experience** across xUnit, NUnit, and TUnit, 
with built-in support for mocking, fake data generation, integration testing, 
and BDD scenarios.

## Why Should You Care?

- **One API for all tests** – Same style whether you're writing unit, integration, or BDD tests
- **Built-in test utilities** – Builders, assertions, mocking, fake data generation
- **Integration testing made easy** – Database fixtures, automatic reset, clean test isolation
- **BDD support** – Write scenarios in Gherkin, bind steps automatically
- **Framework agnostic** – Works with xUnit, NUnit, or TUnit

## System Requirements

- **.NET 8.0, 9.0, or 10.0**
- **C# 10.0 or later**
- Your favorite test framework (xUnit, NUnit, or TUnit)

## Installation (3 minutes)

### Step 1: Create or open a test project

```bash
dotnet new xunit -n MyProject.Tests
cd MyProject.Tests
```

### Step 2: Add PeasyPilot packages

```bash
# Core testing utilities
dotnet add package PeasyPilot.Unit

# Framework integration (choose one)
dotnet add package PeasyPilot.XUnit      # For xUnit
dotnet add package PeasyPilot.NUnit      # For NUnit
dotnet add package PeasyPilot.TUnit      # For TUnit

# Optional: mocking, fake data, BDD
dotnet add package PeasyPilot.Moq        # For mocking
dotnet add package PeasyPilot.Bogus      # For fake data
dotnet add package PeasyPilot.BDD        # For BDD tests
```

### Step 3: Verify installation

```bash
dotnet build
```

Expected: `Build succeeded. 0 Warning(s)`

---

## Your First Unit Test (8 minutes)

Let's write a simple unit test to verify you're all set.

### Step 1: Create a class to test

Create `src/Calculator.cs` (or use an existing class):

```csharp
namespace MyProject;

/// <summary>
/// Simple calculator for testing
/// </summary>
public class Calculator
{
    /// <summary>
    /// Adds two numbers
    /// </summary>
    public int Add(int a, int b) => a + b;
    
    /// <summary>
    /// Multiplies two numbers
    /// </summary>
    public int Multiply(int a, int b) => a * b;
}
```

### Step 2: Create your first test

Create `Tests/CalculatorTests.cs`:

```csharp
using Xunit;
using PeasyPilot.XUnit;
using MyProject;

namespace MyProject.Tests;

/// <summary>
/// Tests for Calculator class
/// </summary>
public class CalculatorTests : PeasyPilotTestBase
{
    private Calculator _calculator = null!;
    
    public override async Task InitializeAsync()
    {
        // Call base initialization first
        await base.InitializeAsync();
        
        // Create the calculator instance for each test
        _calculator = new Calculator();
    }
    
    [Fact]
    public void Add_WithPositiveNumbers_ReturnSum()
    {
        // Arrange
        int a = 5;
        int b = 3;
        int expected = 8;
        
        // Act
        int result = _calculator.Add(a, b);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void Multiply_WithTwoNumbers_ReturnsProduct()
    {
        // Arrange
        int a = 4;
        int b = 5;
        int expected = 20;
        
        // Act
        int result = _calculator.Multiply(a, b);
        
        // Assert
        Assert.Equal(expected, result);
    }
}
```

### Step 3: Run the test

```bash
dotnet test
```

Expected output:
```
Test Run Successful.
Total tests: 2
     Passed: 2
     Failed: 0
```

✅ **Congratulations!** Your first PeasyPilot tests are passing!

---

## Test Anatomy: What Just Happened?

Every PeasyPilot test follows the **AAA pattern**:

1. **Arrange** – Set up test data and preconditions
2. **Act** – Execute the code you're testing
3. **Assert** – Verify the results

Your test:
```csharp
public void Add_WithPositiveNumbers_ReturnSum()
{
    // Arrange: Create input data
    int a = 5;
    int b = 3;
    
    // Act: Call the method
    int result = _calculator.Add(a, b);
    
    // Assert: Check the result
    Assert.Equal(expected: 8, actual: result);
}
```

This pattern keeps tests **readable**, **focused**, and **maintainable**.

---

## Next Steps (2 minutes)

You've mastered the basics! Here's where to go next:

### Want to Learn More About Unit Testing?
📖 [Unit Testing Guide](./GUIDES/unit-testing-guide.md) – Deeper patterns, assertions, builders, mocking

### Want to Test Database Code?
📖 [Integration Testing Guide](./GUIDES/integration-testing-guide.md) – Fixtures, databases, transactions

### Want to Test Business Behavior?
📖 [BDD Testing Guide](./GUIDES/bdd-testing-guide.md) – Gherkin scenarios, step definitions

### Want to Generate Tests Automatically?
📖 [Test Generation Guide](./GUIDES/test-generation-guide.md) – TestAssistant code generation

### Want to Understand MCP?
📖 [MCP Overview](./MCP/mcp-overview.md) – What MCP is and why it matters for testing

---

## Troubleshooting

### "PeasyPilotTestBase not found"
Check that you installed the framework-specific package:
```bash
dotnet add package PeasyPilot.XUnit    # For xUnit
dotnet add package PeasyPilot.NUnit    # For NUnit
dotnet add package PeasyPilot.TUnit    # For TUnit
```

### "InitializeAsync is not overriding any method"
Make sure you're inheriting from `PeasyPilotTestBase` and using `async Task` (not `void`):
```csharp
public class MyTests : PeasyPilotTestBase
{
    public override async Task InitializeAsync()  // ✅ Correct
    {
        await base.InitializeAsync();
    }
}
```

### Tests aren't running
Make sure you're in the correct directory and run:
```bash
dotnet test --verbose
```

---

## Key Concepts

### PeasyPilotTestBase
This is your base class for all tests. It provides:
- `TestContext` – Access to test execution context
- `InitializeAsync()` – Setup before each test
- `DisposeAsync()` – Cleanup after each test

### TestContext
Manages test data, DI registration, and lifecycle. You'll use it in more advanced scenarios.

### IAsyncLifetime
xUnit's async test lifecycle interface. PeasyPilotTestBase implements this so your tests can use `async/await`.

---

## You're Ready!

You now know:
✅ How to install PeasyPilot  
✅ How to write your first test  
✅ How tests are structured (AAA pattern)  
✅ Where to learn more (the guides below)

**Pick a guide and dive deeper:**

| Want to... | Read... |
|-----------|---------|
| Master unit testing | [Unit Testing Guide](./GUIDES/unit-testing-guide.md) |
| Test databases | [Integration Testing Guide](./GUIDES/integration-testing-guide.md) |
| Write scenarios | [BDD Testing Guide](./GUIDES/bdd-testing-guide.md) |
| Generate tests | [Test Generation Guide](./GUIDES/test-generation-guide.md) |
| Choose a framework | [Framework Adapters](./GUIDES/framework-adapters-guide.md) |

Happy testing! 🚀

---

**[← Back to Documentation Hub](./README.md)** | **[← Back to Main README](../README.md)**

**Version:** English | **[Français](./GETTING-STARTED-FR.md)**
