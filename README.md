![PeasyPilot](./assets/images/banner.png)

# PeasyPilot

A modular .NET testing framework for building, orchestrating, and running unit, integration, and BDD-style testing workflows with a consistent API.

![Build](https://github.com/hnidboubker/PeasyPilot/actions/workflows/build.yml/badge.svg?branch=main)
![coverage](https://github.com/hnidboubker/PeasyPilot/actions/workflows/coverage.yml/badge.svg?branch=main)
![release](https://github.com/hnidboubker/PeasyPilot/actions/workflows/release.yml/badge.svg?branch=main)
[![NuGet](https://img.shields.io/nuget/v/PeasyPilot.Unit)](https://www.nuget.org/packages/PeasyPilot.Unit)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PeasyPilot.Unit)](https://www.nuget.org/packages/PeasyPilot.Unit)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-purple)](https://dotnet.microsoft.com/)

## Overview

PeasyPilot is composed of focused packages that work together to provide a lightweight, extensible testing foundation for .NET projects.

### Included packages

- **PeasyPilot.Core** – core abstractions, test context, discovery, orchestration, reporting, and DI integration
- **PeasyPilot.CLI** – command-line runner for filtering and scheduling tests
- **PeasyPilot.Unit** – builder-oriented utilities and shared unit-test helpers
- **PeasyPilot.Integration** – integration testing support and fixtures
- **PeasyPilot.Bogus** – fake data generation via Bogus
- **PeasyPilot.Moq** – mock factory abstractions for Moq
- **PeasyPilot.BDD** – BDD-style feature and scenario model
- **PeasyPilot.Coverage** – coverage reporting support
- **PeasyPilot.Generator** – reflection-based test class scaffolding for xUnit, NUnit, TUnit
- **PeasyPilot.XUnit** – xUnit base class integration
- **PeasyPilot.NUnit** – NUnit base class integration
- **PeasyPilot.TUnit** – TUnit base class integration

## Features

- Unified test context and execution pipeline
- Fluent assertions through `Assert.That(...)`
- Builder pattern support for test object creation
- Fake data generation with Bogus
- Mock creation abstractions with Moq
- xUnit, NUnit, and TUnit lifecycle integration
- CLI execution with filter and impact-analysis flags
- CI-friendly JSON and JUnit report output

## Installation

Add the core packages to your test project:

```bash
dotnet add package PeasyPilot.Core
dotnet add package PeasyPilot.Unit
```

For specific frameworks or tooling:

```bash
# xUnit support
dotnet add package PeasyPilot.XUnit

# NUnit support
dotnet add package PeasyPilot.NUnit

# TUnit support
dotnet add package PeasyPilot.TUnit

# Mocking
dotnet add package PeasyPilot.Moq

# Bogus data factory
dotnet add package PeasyPilot.Bogus

# BDD support
dotnet add package PeasyPilot.BDD

# Test scaffolding with Generator
dotnet add package PeasyPilot.Generator
```

## Test Class Scaffolding (Generator)

Generate test class skeletons by reflecting on public constructors and methods:

```bash
peasypilot generate --assembly ./bin/Release/net10.0/MyApp.dll --type MyApp.Services.UserService
```

This creates a test class with:
- Constructor dependency injection (mocked interfaces, faked concrete types)
- Happy-path tests for each public method
- Edge-case variants (null, empty string, zero, enum members, empty collections)
- Placeholder assertions marked `// TODO` for manual implementation

Output defaults to `Tests/Generated/UserServiceTests.Generated.cs` — customize with `--output`.

## Local packaging watcher

Rebuild and repack NuGet packages automatically after source or configuration changes:

```powershell
./scripts/version-watch.ps1
```

On Bash environments:

```bash
./scripts/version-watch.sh
```

Generated packages are written to `artifacts/` and the watcher stops with `Ctrl+C`.

## Quick start

### xUnit example

```csharp
using Xunit;
using PeasyPilot.XUnit;
using Assert = PeasyPilot.Core.Assertions.Assert;

public class CalculatorTests : PeasyPilotTestBase
{
    [Fact]
    public void Add_WithValidNumbers_ReturnsCorrectSum()
    {
        var calculator = new Calculator();

        var result = calculator.Add(2, 3);

        Assert.That(result)
            .IsEqualTo(5)
            .IsNotNull();
    }
}
```

### NUnit example

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

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
    public void GetUser_WithValidId_ReturnsUser()
    {
        var userId = 1;

        var user = _service.GetUser(userId);

        Assert.That(user, Is.Not.Null);
        Assert.That(user.Id, Is.EqualTo(userId));
    }
}
```

### TUnit example

```csharp
using PeasyPilot.TUnit;

public class SampleTests : PeasyPilotTUnitTestBase
{
    public async Task Example()
    {
        var value = 42;
        await Assert.That(value).IsEqualTo(42);
    }
}
```

### BDD example

```csharp
using Xunit;
using PeasyPilot.BDD;

public class UserRegistrationBddTests
{
    [Fact]
    public async Task UserRegistration_ValidData_SuccessfullyRegisters()
    {
        var feature = new Feature("User Registration");
        var scenario = feature.AddScenario("Register a new user");

        scenario
            .Given("a new user with valid email", async () => await Task.CompletedTask)
            .When("the user submits the registration form", async () => await Task.CompletedTask)
            .Then("the registration should succeed", () => true);

        await scenario.ExecuteAsync();

        Assert.True(scenario.Validate());
    }
}
```

### Test data generation

```csharp
using PeasyPilot.Bogus;

var dataFactory = new TestDataFactory();

var user = dataFactory.Create<User>();
var users = dataFactory.CreateMany<User>(5);
```

### Builder pattern

```csharp
using PeasyPilot.Unit.Builders;

public class UserBuilder : BuilderBase<User>
{
    public UserBuilder WithName(string name)
    {
        Instance.Name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        Instance.Email = email;
        return this;
    }
}

var user = new UserBuilder()
    .WithName("John Doe")
    .WithEmail("john@example.com")
    .Build();
```

### Mocking

```csharp
using PeasyPilot.Moq;

var mockFactory = new MockFactory();
var userRepository = mockFactory.Create(typeof(IUserRepository));
```

### Fluent assertions

```csharp
using PeasyPilot.Core.Assertions;

var result = Calculate(5, 3);

Assert.That(result)
    .IsEqualTo(8)
    .IsNotNull();
```

When the current test framework also exposes an `Assert` type, use the alias below to keep the PeasyPilot API accessible as `Assert.That(...)`:

```csharp
using Assert = PeasyPilot.Core.Assertions.Assert;
```

## Dependency injection

Register PeasyPilot services in the container:

```csharp
services.AddPeasyPilotCore();
```

Or with custom options:

```csharp
services.AddPeasyPilotCore(options =>
{
    options.Environment = "Testing";
    options.EnableLogging = false;
});
```

The package also exposes a full pipeline registration helper:

```csharp
services.AddPeasyPilotPipeline();
```

## CLI usage

```bash
# Show help
peasypilot --help

# Run only matching tests
peasypilot --filter Customer

# Run impact analysis on a changed file set
peasypilot --changed-files CustomerService.cs,OrderService.cs

# Export JUnit XML results
peasypilot --format junit --output test-results.xml

# View execution history
peasypilot history
```

## Project structure

```text
src/
├── PeasyPilot.Core/          # core abstractions and orchestration
├── PeasyPilot.CLI/           # CLI runner
├── PeasyPilot.Unit/          # builders and helpers
├── PeasyPilot.Integration/   # integration fixtures
├── PeasyPilot.Bogus/         # fake data generation
├── PeasyPilot.Moq/           # mocking support
├── PeasyPilot.BDD/           # BDD model and execution
├── PeasyPilot.Coverage/      # coverage support
├── PeasyPilot.XUnit/         # xUnit adapter
├── PeasyPilot.NUnit/         # NUnit adapter
├── PeasyPilot.TUnit/         # TUnit adapter
└── Extensions/
    └── ...

tests/
└── PeasyPilot.Core.Tests/
```

## Target frameworks

- **.NET 8.0**
- **.NET 9.0**
- **.NET 10.0**

The repository is configured with multi-targeting in the project files and uses nullable reference types plus async-friendly testing patterns.

## Dependencies

- `Microsoft.Extensions.DependencyInjection`
- `Bogus` (for fake data generation)
- `Moq` (for mocking)
- `xUnit`, `NUnit`, and `TUnit` for test framework adapters
- ASP.NET Core testing support for integration scenarios

## Contributing

Contributions are welcome. Areas of interest include:

- additional framework adapters
- coverage improvements
- helper utilities and assertions
- CLI enhancements
- documentation and sample projects

## License

PeasyPilot is distributed under the MIT license.

## Support

For issues or questions:

1. Review the project documentation and examples
2. Check the existing samples under `samples/`
3. Open an issue in the repository with a minimal repro

## Version

Current package version: **0.1.2**
