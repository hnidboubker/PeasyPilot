# PeasyPilot.Core API Reference

## Overview

`PeasyPilot.Core` is the foundation package providing framework-agnostic abstractions for test discovery, execution, and reporting. It enables you to build unified test engines that work seamlessly across xUnit, NUnit, TUnit, and custom test frameworks.

**Key Responsibilities:**
- Unified test discovery interface
- Framework-agnostic test execution model
- Standardized test result reporting
- Test context management (thread-safe storage)
- Dependency injection integration
- Test filtering and metadata handling

**Targets:** .NET 8.0, 9.0, 10.0

---

## Main Abstractions

### ITestDiscovery

Discovers available tests in a framework-agnostic way.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestDiscovery
{
    /// <summary>
    /// Discovers the available tests.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The discovered tests.</returns>
    Task<IReadOnlyCollection<TestCase>> DiscoverAsync(
        CancellationToken cancellationToken = default);
}
```

**Purpose:** Provides framework-agnostic test discovery. Implementations exist for xUnit, NUnit, TUnit.

**Example: Using ITestDiscovery**
```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

public class TestDiscoveryService
{
    private readonly ITestDiscovery _discovery;

    public TestDiscoveryService(ITestDiscovery discovery)
    {
        _discovery = discovery;
    }

    public async Task<int> CountTestsAsync()
    {
        var tests = await _discovery.DiscoverAsync();
        return tests.Count;
    }

    public async Task<IEnumerable<string>> GetTestNamesAsync()
    {
        var tests = await _discovery.DiscoverAsync();
        return tests.Select(t => t.Name);
    }
}
```

### ITestEngine

Executes test runs and returns unified results.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestEngine
{
    /// <summary>
    /// Executes the provided test run request.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The unified execution result.</returns>
    Task<TestRunResult> RunAsync(
        TestRunRequest request,
        CancellationToken cancellationToken = default);
}
```

**Purpose:** Executes tests and provides unified results regardless of the underlying test framework.

**Example: Running Tests with ITestEngine**
```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

public class TestRunner
{
    private readonly ITestEngine _engine;

    public TestRunner(ITestEngine engine)
    {
        _engine = engine;
    }

    public async Task<TestRunResult> RunAllTestsAsync()
    {
        var request = new TestRunRequest
        {
            Name = "Full Test Suite",
            Metadata = new() { ["environment"] = "ci" }
        };

        // Add test cases to request...
        return await _engine.RunAsync(request);
    }

    public async Task<bool> HasFailuresAsync(TestRunResult result)
    {
        return result.Failed > 0;
    }
}
```

### ITestReporter

Reports test results in a framework-agnostic way.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestReporter
{
    /// <summary>
    /// Writes the run result to an output destination.
    /// </summary>
    /// <param name="result">The test run result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The serialized output.</returns>
    Task<string> ReportAsync(
        TestRunResult result,
        CancellationToken cancellationToken = default);
}
```

**Purpose:** Provides multiple output formats (console, JSON, XML, HTML) for test results.

**Example: Generating Test Reports**
```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;
using System.IO;

public class ReportGenerator
{
    private readonly ITestReporter _reporter;

    public ReportGenerator(ITestReporter reporter)
    {
        _reporter = reporter;
    }

    public async Task<string> GenerateReportAsync(TestRunResult result)
    {
        return await _reporter.ReportAsync(result);
    }

    public async Task SaveReportAsync(TestRunResult result, string filePath)
    {
        var report = await _reporter.ReportAsync(result);
        await File.WriteAllTextAsync(filePath, report);
    }
}
```

### ITestContext

Thread-safe context for storing and retrieving test data during execution.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestContext
{
    /// <summary>
    /// Gets or adds a value to the test context.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value.</param>
    /// <returns>The value from cache or newly created value.</returns>
    T GetOrAdd<T>(string key, Func<T> factory);
}
```

**Purpose:** Provides thread-safe storage for test data, useful for sharing state across tests or test phases.

**Example: Using TestContext**
```csharp
using PeasyPilot.Core.Context;
using PeasyPilot.Core.Abstractions;

public class TestWithContext
{
    private readonly ITestContext _context;

    public TestWithContext()
    {
        _context = new TestContext();
    }

    [Fact]
    public void TestWithSharedData()
    {
        // Get or create a database connection
        var connection = _context.GetOrAdd("db_connection", () =>
        {
            return new DatabaseConnection("Server=.;Database=test");
        });

        // Connection is reused if accessed again
        var sameConnection = _context.GetOrAdd("db_connection", () =>
        {
            throw new InvalidOperationException("Should reuse existing");
        });

        Assert.Same(connection, sameConnection);
    }
}
```

---

## Core Classes

### TestContext

Thread-safe dictionary-based context for storing test data.

```csharp
namespace PeasyPilot.Core.Context;

public class TestContext : ITestContext
{
    /// <summary>
    /// Gets or adds a value to the test context.
    /// </summary>
    public T GetOrAdd<T>(string key, Func<T> factory)
    {
        return (T)_data.GetOrAdd(key, _ => factory()!);
    }
}
```

**Implementation Details:**
- Uses `ConcurrentDictionary<string, object>` internally
- Thread-safe for multi-threaded test scenarios
- Type-safe with generic GetOrAdd method
- Lazy initialization via factory pattern

**Example: Creating and Using TestContext**
```csharp
using PeasyPilot.Core.Context;

public class ContextExample
{
    public void DemonstrateCaching()
    {
        var context = new TestContext();

        // First call executes factory
        int count1 = context.GetOrAdd("counter", () =>
        {
            Console.WriteLine("Creating counter");
            return 42;
        });

        // Second call returns cached value
        int count2 = context.GetOrAdd("counter", () =>
        {
            throw new InvalidOperationException("Factory should not run");
        });

        Assert.Equal(42, count1);
        Assert.Equal(42, count2);
    }
}
```

---

## Data Models

### TestCase

Represents a single test case in a unified run request.

```csharp
namespace PeasyPilot.Core.Models;

public class TestCase
{
    /// <summary>Gets or sets the test case name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category or suite name.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the test classification kind.</summary>
    public TestKind Kind { get; set; } = TestKind.Unit;

    /// <summary>Gets or sets optional metadata attached to the test case.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum TestKind
{
    Unit = 0,
    Integration = 1,
    E2E = 2,
    Performance = 3,
    Security = 4
}
```

**Example: Creating TestCase Instances**
```csharp
using PeasyPilot.Core.Models;

var unitTest = new TestCase
{
    Name = "Calculator_Add_ReturnsCorrectSum",
    Category = "MathTests",
    Kind = TestKind.Unit,
    Metadata = new()
    {
        ["priority"] = "high",
        ["author"] = "john.doe"
    }
};

var integrationTest = new TestCase
{
    Name = "Database_Connection_ValidatesSchema",
    Category = "DatabaseTests",
    Kind = TestKind.Integration
};
```

### TestRunRequest

Represents a request to run one or more test cases.

```csharp
namespace PeasyPilot.Core.Models;

public class TestRunRequest
{
    /// <summary>Gets or sets the name of the run.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets metadata used by the engine.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>Gets or sets the list of test cases to execute.</summary>
    public List<TestCase> TestCases { get; set; } = new();
}
```

**Example: Building Test Requests**
```csharp
using PeasyPilot.Core.Models;

// Create a test run request
var request = new TestRunRequest
{
    Name = "Nightly Build Tests",
    Metadata = new()
    {
        ["build_id"] = "build_12345",
        ["branch"] = "main",
        ["environment"] = "staging"
    },
    TestCases = new()
    {
        new TestCase
        {
            Name = "UserService_CreateUser_Success",
            Category = "UserTests",
            Kind = TestKind.Unit
        },
        new TestCase
        {
            Name = "PaymentService_ProcessPayment_ValidatesAmount",
            Category = "PaymentTests",
            Kind = TestKind.Integration
        }
    }
};
```

### TestRunResult

Represents the outcome of a unified test run.

```csharp
namespace PeasyPilot.Core.Models;

using PeasyPilot.Core.Eums;

public class TestRunResult
{
    /// <summary>Gets or sets the total number of passed tests.</summary>
    public int Passed { get; set; }

    /// <summary>Gets or sets the total number of failed tests.</summary>
    public int Failed { get; set; }

    /// <summary>Gets or sets the total number of skipped tests.</summary>
    public int Skipped { get; set; }

    /// <summary>Gets or sets the total duration of the run.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Gets or sets the final status of the run.</summary>
    public TestRunStatus Status { get; set; }
}

public enum TestRunStatus
{
    Passed = 0,
    Failed = 1,
    Skipped = 2,
    Incomplete = 3
}
```

**Example: Analyzing TestRunResult**
```csharp
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Eums;

public class ResultAnalyzer
{
    public void AnalyzeRunResults(TestRunResult result)
    {
        Console.WriteLine($"Test Run Results:");
        Console.WriteLine($"  Passed:   {result.Passed}");
        Console.WriteLine($"  Failed:   {result.Failed}");
        Console.WriteLine($"  Skipped:  {result.Skipped}");
        Console.WriteLine($"  Duration: {result.Duration.TotalSeconds:F2}s");
        Console.WriteLine($"  Status:   {result.Status}");

        int total = result.Passed + result.Failed + result.Skipped;
        double successRate = (double)result.Passed / total * 100;
        Console.WriteLine($"  Success Rate: {successRate:F1}%");
    }

    public bool IsSuccessful(TestRunResult result)
    {
        return result.Status == TestRunStatus.Passed && result.Failed == 0;
    }
}
```

### TestResult

Represents the execution outcome of a single test case.

```csharp
namespace PeasyPilot.Core.Models;

public class TestResult
{
    /// <summary>Gets or sets the test name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category or suite.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the execution status.</summary>
    public TestRunStatus Status { get; set; } = TestRunStatus.Passed;

    /// <summary>Gets or sets an optional execution message.</summary>
    public string? Message { get; set; }

    /// <summary>Gets or sets standardized failure details when status is Failed.</summary>
    public TestFailure? Failure { get; set; }

    /// <summary>Gets or sets the execution duration.</summary>
    public TimeSpan Duration { get; set; }
}

public class TestFailure
{
    /// <summary>Gets or sets the exception type.</summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>Gets or sets the failure message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the stack trace.</summary>
    public string StackTrace { get; set; } = string.Empty;
}
```

**Example: Handling TestResult with Failures**
```csharp
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Eums;

public class FailureHandler
{
    public void ProcessTestResult(TestResult result)
    {
        if (result.Status == TestRunStatus.Failed && result.Failure != null)
        {
            Console.WriteLine($"Test Failed: {result.Name}");
            Console.WriteLine($"Exception: {result.Failure.ExceptionType}");
            Console.WriteLine($"Message: {result.Failure.Message}");
            Console.WriteLine($"Stack Trace:\n{result.Failure.StackTrace}");
        }
        else if (result.Status == TestRunStatus.Passed)
        {
            Console.WriteLine($"Test Passed: {result.Name} ({result.Duration.TotalMilliseconds}ms)");
        }
    }
}
```

---

## Configuration

### Dependency Injection Setup

Register PeasyPilot.Core services in your DI container:

```csharp
using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Discovery;
using PeasyPilot.Core.Engines;
using PeasyPilot.Core.Reporting;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register test discovery for your framework
        services.AddScoped<ITestDiscovery, ReflectionTestDiscovery>();

        // Register test engine
        services.AddScoped<ITestEngine, TestEngine>();

        // Register reporters (add multiple for different formats)
        services.AddScoped<ITestReporter, ConsoleReporter>();
        services.AddScoped<ITestReporter, JsonFileReporter>();

        // Register test context
        services.AddScoped<ITestContext, TestContext>();
    }
}
```

---

## Common Patterns

### Pattern 1: End-to-End Test Execution

```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

public class TestExecutionPipeline
{
    private readonly ITestDiscovery _discovery;
    private readonly ITestEngine _engine;
    private readonly ITestReporter _reporter;

    public TestExecutionPipeline(
        ITestDiscovery discovery,
        ITestEngine engine,
        ITestReporter reporter)
    {
        _discovery = discovery;
        _engine = engine;
        _reporter = reporter;
    }

    public async Task<string> ExecuteAndReportAsync()
    {
        // 1. Discover tests
        var tests = await _discovery.DiscoverAsync();
        
        // 2. Build request
        var request = new TestRunRequest
        {
            Name = "Full Suite",
            TestCases = tests.Cast<TestCase>().ToList()
        };

        // 3. Execute
        var result = await _engine.RunAsync(request);

        // 4. Report
        return await _reporter.ReportAsync(result);
    }
}
```

### Pattern 2: Filtered Test Execution

```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

public class FilteredTestRunner
{
    private readonly ITestDiscovery _discovery;
    private readonly ITestEngine _engine;

    public async Task<TestRunResult> RunUnitTestsOnlyAsync()
    {
        var allTests = await _discovery.DiscoverAsync();
        
        // Filter to unit tests only
        var unitTests = allTests
            .Where(t => t.Kind == TestKind.Unit)
            .ToList();

        var request = new TestRunRequest
        {
            Name = "Unit Tests",
            TestCases = unitTests
        };

        return await _engine.RunAsync(request);
    }

    public async Task<TestRunResult> RunHighPriorityTestsAsync()
    {
        var allTests = await _discovery.DiscoverAsync();
        
        // Filter by priority metadata
        var highPriority = allTests
            .Where(t => t.Metadata.ContainsKey("priority") &&
                       t.Metadata["priority"] == "high")
            .ToList();

        var request = new TestRunRequest
        {
            Name = "High Priority Tests",
            TestCases = highPriority
        };

        return await _engine.RunAsync(request);
    }
}
```

### Pattern 3: Context-Based Test State Management

```csharp
using PeasyPilot.Core.Context;

public class TestStateManager
{
    private readonly TestContext _context;

    public TestStateManager()
    {
        _context = new TestContext();
    }

    public void InitializeTestEnvironment()
    {
        // Initialize shared resources once
        var database = _context.GetOrAdd("database", () =>
            new TestDatabase("connection_string"));

        var httpClient = _context.GetOrAdd("http_client", () =>
            new HttpClient { BaseAddress = new Uri("http://localhost:5000") });

        var config = _context.GetOrAdd("config", () =>
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build());
    }

    public T GetOrCreateService<T>(string key, Func<T> factory) where T : class
    {
        return _context.GetOrAdd(key, factory);
    }
}
```

---

## Reference Summary

| Component | Purpose | Stability |
|-----------|---------|-----------|
| ITestDiscovery | Discover tests in framework-agnostic way | ✅ Stable |
| ITestEngine | Execute tests and return results | ✅ Stable |
| ITestReporter | Format and report test results | ✅ Stable |
| ITestContext | Thread-safe test state storage | ✅ Stable |
| TestContext | Implementation of ITestContext | ✅ Stable |
| TestCase | Represents a single test | ✅ Stable |
| TestRunRequest | Represents a test run request | ✅ Stable |
| TestRunResult | Represents overall run outcome | ✅ Stable |
| TestResult | Represents single test outcome | ✅ Stable |
| TestFailure | Failure details | ✅ Stable |

---

## See Also

- **GETTING-STARTED.md** — Quick start guide
- **unit-testing-guide.md** — Unit testing patterns
- **integration-testing-guide.md** — Integration testing setup
- **api-unit.md** — PeasyPilot.Unit API
- **api-integration.md** — PeasyPilot.Integration API

---

**[← Back to API References](./README.md)** | **[← Back to Documentation Hub](../README.md)**

**Version:** English | **[Français](./api-core-FR.md)**
