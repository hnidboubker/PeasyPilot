# Extensibility & Contributing Guide

## Overview

This guide shows you how to extend PeasyPilot with custom components and how to contribute back to the project.

**You'll learn:**
- Plugin architecture and extension points
- Creating custom reporters for specialized output formats
- Implementing custom code analyzers
- Writing custom BDD step definitions
- Building custom mock strategies
- Registering components with dependency injection
- Contributing code, tests, and documentation

**Prerequisites:** Complete [Getting Started](../GETTING-STARTED.md) and review [Advanced Testing Patterns](testing-patterns.md)  
**Time estimate:** 60 minutes  
**Code examples:** 10+ working examples  
**Frameworks:** xUnit, NUnit, TUnit

---

## Plugin Architecture

PeasyPilot is built on extensible abstractions. Every major component has an interface you can implement:

```
Core Abstractions (ITestReporter, ICodeAnalyzer, IMockFactory, etc.)
           ↓
Your Implementation
           ↓
Register in Dependency Injection
           ↓
PeasyPilot loads and uses your component
```

### Discovery Pattern

PeasyPilot components are discovered via dependency injection. Register your implementation, and the framework automatically detects and uses it.

**Key Extension Points:**

| Component | Interface | Purpose |
|-----------|-----------|---------|
| **Reporters** | `ITestReporter` | Custom test result formats (JSON, XML, HTML) |
| **Code Analyzers** | `ICodeAnalyzer` | Analyze methods for test generation |
| **Mock Factories** | `IMockFactory` | Custom mock creation strategies |
| **Test Contexts** | `ITestContext` | Custom context storage |
| **Step Bindings** | `BddStepDefinition` (derived) | BDD step implementations |

---

## Custom Reporters

### Understanding ITestReporter

The `ITestReporter` interface defines how test results are formatted and output:

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestReporter
{
    Task<string> ReportAsync(
        TestRunResult result, 
        CancellationToken cancellationToken = default
    );
}
```

### Example 1: Custom JSON Reporter

Create a reporter that outputs detailed JSON with custom formatting:

```csharp
using System.Text.Json;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace MyApp.Testing.Reports;

public class CustomJsonReporter : ITestReporter
{
    private readonly JsonSerializerOptions _jsonOptions;

    public CustomJsonReporter()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<string> ReportAsync(
        TestRunResult result, 
        CancellationToken cancellationToken = default)
    {
        var reportData = new
        {
            summary = new
            {
                result.TotalTests,
                result.PassedTests,
                result.FailedTests,
                result.SkippedTests,
                Duration = result.Duration?.TotalMilliseconds
            },
            tests = result.Results.Select(r => new
            {
                r.TestName,
                r.Status,
                r.Message,
                DurationMs = r.Duration?.TotalMilliseconds
            }).ToList(),
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(
            reportData, 
            _jsonOptions
        );

        return await Task.FromResult(json);
    }
}
```

### Example 2: HTML Reporter

Create a reporter that generates an HTML report:

```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace MyApp.Testing.Reports;

public class HtmlReporter : ITestReporter
{
    public async Task<string> ReportAsync(
        TestRunResult result, 
        CancellationToken cancellationToken = default)
    {
        var passPercentage = result.TotalTests > 0 
            ? (result.PassedTests / (decimal)result.TotalTests) * 100 
            : 0;

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Test Report</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .summary {{ background: #f0f0f0; padding: 15px; border-radius: 5px; }}
        .test-list {{ margin-top: 20px; }}
        .passed {{ color: green; }}
        .failed {{ color: red; }}
        .skipped {{ color: orange; }}
    </style>
</head>
<body>
    <h1>Test Execution Report</h1>
    <div class='summary'>
        <h2>Summary</h2>
        <p>Total Tests: {result.TotalTests}</p>
        <p class='passed'>Passed: {result.PassedTests}</p>
        <p class='failed'>Failed: {result.FailedTests}</p>
        <p class='skipped'>Skipped: {result.SkippedTests}</p>
        <p>Success Rate: {passPercentage:F2}%</p>
    </div>
    <div class='test-list'>
        <h2>Test Results</h2>
        <ul>
            {string.Join("", result.Results.Select(r => 
                $"<li class='{r.Status.ToString().ToLower()}'>{r.TestName}: {r.Status}</li>"
            ))}
        </ul>
    </div>
</body>
</html>";

        return await Task.FromResult(html);
    }
}
```

### Registering Custom Reporters

Register your reporter in dependency injection:

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register your custom reporter
services.AddSingleton<ITestReporter, CustomJsonReporter>();

var serviceProvider = services.BuildServiceProvider();
var reporter = serviceProvider.GetRequiredService<ITestReporter>();
```

---

## Custom Code Analyzers

### Understanding ICodeAnalyzer

The `ICodeAnalyzer` interface analyzes methods for test generation:

```csharp
namespace PeasyPilot.TestAssistant.Abstractions;

public interface ICodeAnalyzer
{
    Task<IReadOnlyList<MethodTestModel>> AnalyzeFileAsync(string filePath);
    Task<IReadOnlyList<MethodTestModel>> AnalyzeTypeAsync(Type type);
    Task<MethodTestModel?> AnalyzeMethodAsync(Type? type, string methodName);
}
```

### Example 3: Custom Python-to-CSharp Analyzer

Imagine you want to analyze Python code and suggest CSharp test patterns:

```csharp
using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;

namespace MyApp.Analysis;

public class PythonPatternAnalyzer : ICodeAnalyzer
{
    public async Task<IReadOnlyList<MethodTestModel>> AnalyzeFileAsync(string filePath)
    {
        if (!filePath.EndsWith(".py"))
            throw new ArgumentException("Expected .py file");

        var pythonCode = await File.ReadAllTextAsync(filePath);
        var methods = ExtractPythonMethods(pythonCode);
        
        return methods
            .Select(m => AnalyzePythonMethod(m))
            .ToList()
            .AsReadOnly();
    }

    public Task<IReadOnlyList<MethodTestModel>> AnalyzeTypeAsync(Type type)
    {
        throw new NotSupportedException("Use AnalyzeFileAsync for Python files");
    }

    public Task<MethodTestModel?> AnalyzeMethodAsync(Type? type, string methodName)
    {
        throw new NotSupportedException("Use AnalyzeFileAsync for Python files");
    }

    private MethodTestModel AnalyzePythonMethod(string methodCode)
    {
        // Extract method name, parameters, exceptions, etc. from Python code
        var name = ExtractMethodName(methodCode);
        var parameters = ExtractParameters(methodCode);
        var exceptions = ExtractExceptions(methodCode);

        return new MethodTestModel
        {
            MethodName = name,
            Parameters = parameters,
            Exceptions = exceptions,
            IsAsync = methodCode.Contains("async def"),
            TestableScenarios = GenerateScenarios(methodCode)
        };
    }

    private string ExtractMethodName(string code) => "method_name";
    private List<MethodParameterInfo> ExtractParameters(string code) => new();
    private List<ExceptionInfo> ExtractExceptions(string code) => new();
    private List<TestableScenario> GenerateScenarios(string code) => new();
}
```

---

## Custom Step Definitions (BDD)

### Understanding BddStepDefinition

Step definitions bind Gherkin steps to C# code. Create a class inheriting from `BddStepDefinition`:

```csharp
namespace PeasyPilot.BDD.StepDefinitions;

public abstract class BddStepDefinition
{
    // Your step methods with [Given], [When], [Then] attributes
}
```

### Example 4: API Testing Step Definitions

```csharp
using PeasyPilot.BDD.StepDefinitions;
using System.Net.Http;

namespace MyApp.BddTests;

public class ApiSteps : BddStepDefinition
{
    private HttpClient? _httpClient;
    private HttpResponseMessage? _lastResponse;
    private string? _baseUrl;

    [Given("an API endpoint at {url}")]
    public void SetupApiEndpoint(string url)
    {
        _baseUrl = url;
        _httpClient = new HttpClient { BaseAddress = new Uri(url) };
    }

    [When("I make a GET request to {endpoint}")]
    public async Task MakeGetRequest(string endpoint)
    {
        if (_httpClient == null)
            throw new InvalidOperationException("API not configured");

        _lastResponse = await _httpClient.GetAsync(endpoint);
    }

    [When("I send a POST request with {json}")]
    public async Task MakePostRequest(string json)
    {
        if (_httpClient == null)
            throw new InvalidOperationException("API not configured");

        var content = new StringContent(
            json, 
            System.Text.Encoding.UTF8, 
            "application/json"
        );
        _lastResponse = await _httpClient.PostAsync("/api/endpoint", content);
    }

    [Then("the response status should be {statusCode:int}")]
    public void AssertStatusCode(int statusCode)
    {
        if (_lastResponse == null)
            throw new InvalidOperationException("No response recorded");

        var actual = (int)_lastResponse.StatusCode;
        if (actual != statusCode)
            throw new AssertionException(
                $"Expected status {statusCode}, got {actual}"
            );
    }

    [Then("the response should contain {expectedText}")]
    public async Task AssertResponseContent(string expectedText)
    {
        if (_lastResponse == null)
            throw new InvalidOperationException("No response recorded");

        var content = await _lastResponse.Content.ReadAsStringAsync();
        if (!content.Contains(expectedText))
            throw new AssertionException(
                $"Response does not contain '{expectedText}'"
            );
    }
}
```

### Example 5: Database Step Definitions

```csharp
using PeasyPilot.BDD.StepDefinitions;

namespace MyApp.BddTests;

public class DatabaseSteps : BddStepDefinition
{
    private ITestDatabase? _database;
    private List<(string TableName, object Data)> _insertedData = new();

    [Given("a clean database")]
    public async Task InitializeDatabase()
    {
        _database = new InMemoryTestDatabase();
        await _database.InitializeAsync();
    }

    [Given("the users table has {count:int} users")]
    public async Task SeedUsers(int count)
    {
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");

        for (int i = 1; i <= count; i++)
        {
            var user = new { Id = i, Name = $"User{i}", Email = $"user{i}@test.com" };
            await _database.InsertAsync("Users", user);
            _insertedData.Add(("Users", user));
        }
    }

    [When("I query for user {userId:int}")]
    public async Task QueryUser(int userId)
    {
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");

        var user = await _database.QueryAsync("Users", userId);
        // Store in step context
    }

    [Then("the user should exist")]
    public void AssertUserExists()
    {
        if (_database == null)
            throw new InvalidOperationException("Database not initialized");
        // Verify user was found
    }
}
```

---

## Custom Mock Strategies

### Understanding IMockFactory

The `IMockFactory` interface creates mock objects:

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface IMockFactory
{
    object Create(Type type);
}
```

### Example 6: Advanced Mock Factory with Recording

Create a mock factory that records method calls:

```csharp
using Moq;
using PeasyPilot.Core.Abstractions;

namespace MyApp.Testing.Mocks;

public class RecordingMockFactory : IMockFactory
{
    private readonly Dictionary<Type, List<(string Method, object?[] Args)>> _callHistory;

    public RecordingMockFactory()
    {
        _callHistory = new();
    }

    public object Create(Type type)
    {
        var mockType = typeof(Mock<>).MakeGenericType(type);
        var mockInstance = (Mock)Activator.CreateInstance(mockType)!;

        // Set up recording of all method calls
        RecordMethodCalls(mockInstance, type);

        return mockInstance.GetType().GetProperty("Object")!.GetValue(mockInstance)!;
    }

    private void RecordMethodCalls(Mock mockInstance, Type interfaceType)
    {
        if (!_callHistory.ContainsKey(interfaceType))
            _callHistory[interfaceType] = new();

        // Configure mock to track calls
        var callsProperty = mockInstance.GetType().GetProperty("Calls");
        if (callsProperty != null)
        {
            var calls = callsProperty.GetValue(mockInstance);
            // Record calls as they happen
        }
    }

    public IReadOnlyList<(string Method, object?[] Args)> GetCallHistory(Type type)
    {
        return _callHistory.ContainsKey(type) 
            ? _callHistory[type].AsReadOnly() 
            : new List<(string, object?[])>().AsReadOnly();
    }

    public void Reset(Type type)
    {
        if (_callHistory.ContainsKey(type))
            _callHistory[type].Clear();
    }
}
```

### Example 7: Stub Mock Factory

Create a simple stub factory for quick mocks:

```csharp
using PeasyPilot.Core.Abstractions;

namespace MyApp.Testing.Mocks;

public class StubMockFactory : IMockFactory
{
    public object Create(Type type)
    {
        if (!type.IsInterface)
            throw new ArgumentException("Only interfaces can be stubbed", nameof(type));

        // Create a proxy object using DispatchProxy
        return DispatchProxy.Create(type, typeof(StubInterceptor<>)
            .MakeGenericType(type));
    }

    private class StubInterceptor<T> : DispatchProxy where T : class
    {
        protected override object? Invoke(
            System.Reflection.MethodInfo? targetMethod, 
            object?[]? args)
        {
            // Return default values for all calls
            return targetMethod?.ReturnType == typeof(void)
                ? null
                : Activator.CreateInstance(targetMethod?.ReturnType ?? typeof(object));
        }
    }
}
```

---

## Dependency Injection Integration

### Registering Components

PeasyPilot uses Microsoft.Extensions.DependencyInjection. Register your custom components:

### Example 8: Complete DI Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using MyApp.Testing.Reports;
using MyApp.Testing.Mocks;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Integration.Fixtures;

namespace MyApp.Testing;

public class TestServiceSetup : IntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Register custom reporters
        services.AddSingleton<ITestReporter, CustomJsonReporter>();
        
        // Or register with factory pattern
        services.AddSingleton<ITestReporter>(sp => 
            new HtmlReporter()
        );

        // Register custom mock factories
        services.AddSingleton<IMockFactory, RecordingMockFactory>();

        // Register your application services
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IOrderService, OrderService>();
    }
}
```

### Service Lifetimes

Choose the right lifetime for your components:

```csharp
// Singleton: Created once, shared across all tests
services.AddSingleton<IUserRepository, UserRepository>();

// Transient: Created fresh each time requested
services.AddTransient<IOrderService, OrderService>();

// Scoped: Created once per scope (useful for test context)
services.AddScoped<ITestContext, TestContext>();
```

---

## Contributing to PeasyPilot

### Code Contribution Guidelines

#### 1. Fork and Branch

```bash
# Clone the repository
git clone https://github.com/Houssine/PeasyPilot.git
cd PeasyPilot

# Create a feature branch
git checkout -b feature/my-feature-name
```

#### 2. Code Style

Follow the existing code style:

```csharp
// ✅ GOOD: Proper formatting and naming
namespace PeasyPilot.NewFeature.Abstractions;

public interface IMyNewAbstraction
{
    Task<string> ProcessAsync(string input, CancellationToken cancellationToken = default);
}

// ❌ BAD: Inconsistent style
namespace PeasyPilot.NewFeature;
public interface IMyNewAbstraction{Task<string> ProcessAsync(string input);}
```

#### 3. Write Tests for Your Feature

Create comprehensive tests in the appropriate test project:

```csharp
using Xunit;
using PeasyPilot.NewFeature;

namespace PeasyPilot.Tests.NewFeature;

public class MyNewFeatureTests
{
    [Fact]
    public async Task ProcessAsync_WithValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var feature = new MyNewFeature();
        var input = "test input";

        // Act
        var result = await feature.ProcessAsync(input);

        // Assert
        Assert.Equal("expected output", result);
    }

    [Fact]
    public async Task ProcessAsync_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var feature = new MyNewFeature();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => feature.ProcessAsync(null!)
        );
    }
}
```

#### 4. Add XML Documentation

Document public types and members:

```csharp
/// <summary>
/// Processes input data according to custom business logic.
/// </summary>
/// <param name="input">The input string to process.</param>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>The processed result.</returns>
/// <exception cref="ArgumentNullException">Thrown if input is null.</exception>
public async Task<string> ProcessAsync(
    string input, 
    CancellationToken cancellationToken = default)
{
}
```

#### 5. Update Documentation

If your feature is user-facing, add documentation:

- Create or update a guide in `docs/GUIDES/`
- Add examples with working code
- Include both English and French versions
- Link from `README.md`

#### 6. Submit a Pull Request

```bash
# Commit your changes
git add .
git commit -m "feat: Add my new feature with tests and docs"

# Push to your fork
git push origin feature/my-feature-name
```

Then create a PR on GitHub with:
- Clear description of the feature
- Link to related issues
- Checklist of testing
- Example usage

### Testing Requirements

All contributions must pass:

```bash
# Run all tests across all frameworks
dotnet test

# Run tests for specific framework
dotnet test --framework net10.0

# Check code coverage (if available)
dotnet test /p:CollectCoverage=true
```

### Documentation Standards

**English Guide Requirements:**
- ~2,000-3,000 words
- 5+ working code examples
- Progressive structure (basic → advanced)
- Clear section headings
- Cross-links to related docs

**French Translation Requirements:**
- Professional French (not machine-translated)
- Maintains original structure and examples
- All code comments translated
- All headings and labels translated

---

## Development Setup

### Prerequisites

- .NET SDK 8.0 or later (currently targeting 10.0)
- Git
- IDE: Visual Studio 2022, VS Code, or Rider

### Clone and Build

```bash
# Clone repository
git clone https://github.com/Houssine/PeasyPilot.git
cd PeasyPilot

# Restore NuGet packages
dotnet restore

# Build all projects
dotnet build

# Run all tests
dotnet test
```

### Project Structure

```
src/                      # Production code (NuGet packages)
├── PeasyPilot.Core/      # Core abstractions & models
├── PeasyPilot.BDD/       # Behavior-Driven Development
├── PeasyPilot.XUnit/     # xUnit adapter
├── PeasyPilot.NUnit/     # NUnit adapter
└── [other packages]

tests/                    # Unit & integration tests
├── PeasyPilot.Core.Tests/
└── [framework-specific tests]

samples/                  # Working examples
├── PeasyPilot.XUnit.Samples/
└── [other samples]

docs/                     # Documentation
├── GETTING-STARTED.md
├── GUIDES/
├── MCP/
├── REFERENCE/
└── ADVANCED/
```

### Making Changes

1. Create a branch: `git checkout -b feature/description`
2. Make your changes in `src/`
3. Write tests in `tests/`
4. Update docs if needed
5. Run `dotnet build && dotnet test`
6. Commit and push

### Running Specific Tests

```bash
# Run tests for a specific class
dotnet test --filter "ClassName"

# Run tests matching a pattern
dotnet test --filter "TestMethod*"

# Run with verbose output
dotnet test -v detailed
```

---

## Common Extension Scenarios

### Scenario 1: Add Support for a New Output Format

1. Implement `ITestReporter`
2. Register in DI setup
3. Add tests
4. Document usage

### Scenario 2: Custom Code Analyzer for Domain Language

1. Implement `ICodeAnalyzer`
2. Analyze your domain-specific language
3. Generate test scenarios
4. Register and test

### Scenario 3: New Framework Adapter

1. Implement `ITestDiscovery` and `ITestOrchestrator`
2. Handle framework-specific attributes
3. Create corresponding adapter package
4. Add samples and tests

---

## Support & Questions

- **Issues:** https://github.com/Houssine/PeasyPilot/issues
- **Discussions:** https://github.com/Houssine/PeasyPilot/discussions
- **Documentation:** See the docs/ folder

---

[← Back to Advanced Guides](README.md)
