# Test Generation Guide

## Overview

TestAssistant analyzes your code and generates test cases automatically. Save time writing boilerplate tests.

**Prerequisites:** [Unit Testing Guide](./unit-testing-guide.md)  
**Time:** 20 minutes  

---

## How It Works

1. **Analyze** – Reflection scans your types (methods, parameters, dependencies)
2. **Generate** – Creates test cases for happy path, boundaries, errors
3. **Render** – Produces framework-specific code (xUnit/NUnit/TUnit)
4. **Customize** – Edit generated tests to match your needs

---

## Basic Usage

```csharp
using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Rendering;

// Your class to test
public class Calculator
{
    public int Add(int a, int b) => a + b;
    public decimal Divide(decimal a, decimal b) => a / b;
}

// Step 1: Analyze
var analyzer = new ReflectionTestScenarioAnalyzer();
var options = new TestBatteryAnalysisOptions
{
    TargetFramework = "xunit",
    IncludeBoundaryTests = true,
    MaxEnumCases = 10
};
var proposal = analyzer.Analyze(typeof(Calculator), options);

// Step 2: Render
var registry = new TestBatteryRendererRegistry();
var renderer = registry.GetRenderer("xunit");
var renderOptions = new RenderOptions { OutputNamespace = "MyApp.Tests" };
string generatedCode = renderer.Render(proposal, renderOptions);

// Step 3: Write to file
File.WriteAllText("CalculatorTests.cs", generatedCode);
```

---

## Generated Output Example

```csharp
using Xunit;
using PeasyPilot.XUnit;
using MyApp;

namespace MyApp.Tests;

public class CalculatorTests : PeasyPilotTestBase
{
    private Calculator _subject = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _subject = new Calculator();
    }
    
    [Fact]
    public void Add_HappyPath()
    {
        // Happy path: typical case
        var result = _subject.Add(5, 3);
        Assert.Equal(8, result);
    }
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(int.MinValue, 0)]
    [InlineData(int.MaxValue, -1)]
    public void Add_BoundaryValues(int a, int b)
    {
        // Boundary cases
        var result = _subject.Add(a, b);
        Assert.NotNull(result);
    }
    
    [Fact]
    public void Divide_ByZero_ThrowsException()
    {
        // Error case
        Assert.Throws<DivideByZeroException>(() => _subject.Divide(10, 0));
    }
}
```

---

## Supported Scenarios

TestAssistant detects and generates:

- **Happy path** – Normal operation
- **Boundary values** – Min/max values, zero, empty strings
- **Null inputs** – Handling of null parameters
- **Exceptions** – Error conditions and error messages
- **Async methods** – Async/await patterns
- **Dependencies** – Constructor injection, mocking

---

## Customizing Generation

### Framework Selection
```csharp
// xUnit
var renderer = registry.GetRenderer("xunit");

// NUnit
var renderer = registry.GetRenderer("nunit");

// TUnit
var renderer = registry.GetRenderer("tunit");
```

### Analysis Options
```csharp
var options = new TestBatteryAnalysisOptions
{
    TargetFramework = "xunit",
    IncludeBoundaryTests = true,        // Generate boundary tests
    IncludeNullTests = true,             // Test null inputs
    IncludeExceptionTests = true,        // Test error conditions
    MaxEnumCases = 10,                   // Limit enum value tests
    MaxCollectionSize = 5                // Limit collection test sizes
};
```

---

## Workflow Integration

### In Your Build
```bash
# Generate tests as part of build
dotnet run --project TestAssistant.Generator -- --input "src/MyClass.cs" --output "tests/"
```

### In CI/CD
```yaml
- name: Generate tests
  run: dotnet run --project TestAssistant.Generator -- --input "src/" --output "tests/"

- name: Run all tests
  run: dotnet test
```

---

## Best Practices

✅ **DO**
- Review generated tests before committing
- Customize test data to match real scenarios
- Use generated tests as a starting point
- Keep manual and generated tests in sync
- Run generation regularly as code changes

❌ **DON'T**
- Rely entirely on generated tests
- Ignore edge cases TestAssistant missed
- Regenerate without reviewing changes
- Commit untested generated code

---

## Next Steps

📖 **[Framework Adapters](./framework-adapters-guide.md)** – Choose your test framework  
📖 **[Unit Testing Guide](./unit-testing-guide.md)** – Manual test writing  

Generated tests speed up development while maintaining quality! ⚡
