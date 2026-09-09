# PeasyPilot Core

Core abstractions, test context, discovery, orchestration, reporting, and DI integration.

---

## What is PeasyPilot.Core?

**PeasyPilot.Core** is the foundation of the PeasyPilot framework. It provides:

- ✅ Test abstractions and base classes
- ✅ Test discovery (finding all test methods)
- ✅ Test orchestration (execution pipeline)
- ✅ Test reporting (results formatting)
- ✅ Dependency injection integration
- ✅ Fluent assertions

---

## How It Works

### Pipeline

```
1. Discovery
   ↓
   Scan assemblies for test methods
   ↓
2. Orchestration
   ↓
   Prepare execution environment
   Execute each test
   Collect results
   ↓
3. Reporting
   ↓
   Format results (JSON, JUnit, Console)
```

---

## Key Components

### ITestCase

Represents a single test case.

```csharp
public interface ITestCase
{
    string Name { get; }
    string? Description { get; }
    Type TargetType { get; }
    MethodInfo Method { get; }
    IReadOnlyList<object> Parameters { get; }
}
```

### ITestDiscovery

Finds all test cases in assemblies.

```csharp
public interface ITestDiscovery
{
    Task<IReadOnlyList<ITestCase>> DiscoverAsync(params Assembly[] assemblies);
}
```

### ITestOrchestrator

Executes tests and collects results.

```csharp
public interface ITestOrchestrator
{
    Task<IReadOnlyList<TestRunResult>> RunAsync(
        IEnumerable<ITestCase> testCases,
        CancellationToken cancellationToken = default);
}
```

### ITestReporter

Formats test results.

```csharp
public interface ITestReporter
{
    string Format(IReadOnlyList<TestRunResult> results);
    Task WriteAsync(string path, IReadOnlyList<TestRunResult> results);
}
```

### Assert — Fluent Assertions

```csharp
using PeasyPilot.Core.Assertions;

Assert.That(value)
    .IsNotNull()
    .IsGreaterThan(0)
    .IsLessThan(100);

Assert.That(text)
    .Contains("expected")
    .StartsWith("prefix");
```

---

## Usage

### Basic Setup

```csharp
using PeasyPilot.Core;

// Use in your test framework adapter
var discovery = new XUnitTestDiscovery();
var orchestrator = new XUnitTestOrchestrator();
var reporter = new JsonTestReporter();

var testCases = await discovery.DiscoverAsync(typeof(MyTests).Assembly);
var results = await orchestrator.RunAsync(testCases);
var json = reporter.Format(results);

Console.WriteLine(json);
```

### With DI Container

```csharp
var services = new ServiceCollection();
services.AddScoped<IUserRepository, TestUserRepository>();

// Framework adapters use this
var testContext = new TestContext(services);
```

---

## Architecture

```
PeasyPilot.Core/
├── Abstractions/
│   ├── ITestCase.cs
│   ├── ITestDiscovery.cs
│   ├── ITestOrchestrator.cs
│   └── ITestReporter.cs
├── Assertions/
│   └── Assert.cs
├── Reporting/
│   ├── TestRunResult.cs
│   └── JsonTestReporter.cs
└── Context/
    └── TestContext.cs
```

---

## Supported Frameworks

Core works with:
- xUnit (via PeasyPilot.XUnit)
- NUnit (via PeasyPilot.NUnit)
- TUnit (via PeasyPilot.TUnit)

---

## Best Practices

✅ Use fluent assertions for readability
✅ Leverage DI for test dependencies
✅ Use TestContext for test metadata
✅ Organize tests by domain/feature

---

**See also:** PeasyPilot-XUnit, PeasyPilot-NUnit, PeasyPilot-TUnit
