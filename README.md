

![PeasyPilot](./assets/images/banner.png height="250")

# PeasyPilot - A Comprehensive .NET Testing Framework

PeasyPilot is a modern, modular testing framework for .NET applications designed to simplify test creation, management, and execution. It provides abstractions, utilities, and framework-specific adapters for unit, integration, and BDD testing.

## Overview

PeasyPilot consists of multiple focused modules that work together seamlessly:

- **PeasyPilot.Core** - Core abstractions and utilities
- **PeasyPilot.Unit** - Unit testing utilities and builders
- **PeasyPilot.Integration** - Integration testing fixtures
- **PeasyPilot.Bogus** - Fake data generation
- **PeasyPilot.Moq** - Mocking support
- **PeasyPilot.BDD** - Behavior-driven development
- **PeasyPilot.Coverage** - Code coverage tracking
- **PeasyPilot.XUnit** - XUnit framework integration
- **PeasyPilot.NUnit** - NUnit framework integration
- **PeasyPilot.TUnit** - TUnit framework integration

## Features

### 🎯 Core Features
- **Unified Test Context** - Manage test state across your test suite
- **Fluent Assertions** - Write readable, chainable assertions
- **Builder Pattern Support** - Create complex test objects easily
- **Test Data Factories** - Generate realistic test data with Bogus
- **Mocking Framework** - Create mocks using Moq
- **BDD Support** - Write scenarios in Gherkin format

### 🔧 Framework Integration
- Native support for xUnit, NUnit, and TUnit
- Base classes for each framework with common functionality
- Collection fixtures and setup/teardown hooks
- Async-friendly lifecycle management

### 📊 Testing Utilities
- Code coverage reporting
- Integration test fixtures with database support
- Web application testing with ASP.NET Core TestHost
- Comprehensive helper utilities

## Installation

Add PeasyPilot to your test project:

```bash
dotnet add package PeasyPilot.Core
dotnet add package PeasyPilot.Unit
```

For specific frameworks and features:

```bash
# For xUnit tests
dotnet add package PeasyPilot.XUnit

# For mocking
dotnet add package PeasyPilot.Moq

# For fake data
dotnet add package PeasyPilot.Bogus

# For BDD
dotnet add package PeasyPilot.BDD
```

## Quick Start

### Unit Tests with xUnit

```csharp
using Xunit;
using PeasyPilot.XUnit;
using PeasyPilot.Bogus;

public class CalculatorTests : PeasyPilotTestBase
{
    private readonly TestDataFactory _dataFactory = new();

    [Fact]
    public void Add_WithValidNumbers_ReturnsCorrectSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(2, 3);
        
        // Assert
        Assert.That(result)
            .IsEqualTo(5)
            .IsNotNull();
    }
}
```

### Unit Tests with NUnit

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
        // Arrange
        var userId = 1;

        // Act
        var user = _service.GetUser(userId);

        // Assert
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Id, Is.EqualTo(userId));
    }
}
```

### Integration Tests

```csharp
using Xunit;
using PeasyPilot.Integration.Fixtures;

public class ApiIntegrationTests : IClassFixture<WebApplicationTestFactory<Startup>>
{
    private readonly WebApplicationTestFactory<Startup> _factory;

    public ApiIntegrationTests(WebApplicationTestFactory<Startup> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetEndpoint_ReturnsOkResult()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }
}
```

### BDD Tests

```csharp
using Xunit;
using PeasyPilot.BDD;

public class UserRegistrationBddTests
{
    [Fact]
    public async Task UserRegistration_ValidData_SuccessfullyRegisters()
    {
        // Arrange
        var feature = new Feature("User Registration");
        var scenario = feature.AddScenario("Register a new user");

        scenario
            .Given("a new user with valid email", async () =>
            {
                // Setup user data
                await Task.CompletedTask;
            })
            .When("the user submits the registration form", async () =>
            {
                // Perform registration
                await Task.CompletedTask;
            })
            .Then("the registration should succeed", () =>
            {
                // Verify success
                return true;
            });

        // Act
        await scenario.ExecuteAsync();

        // Assert
        Assert.True(scenario.Validate());
    }
}
```

### Test Data Generation

```csharp
using PeasyPilot.Bogus;
using PeasyPilot.Core.Abstractions;

var dataFactory = new TestDataFactory();

// Create a single object
var user = dataFactory.Create<User>();

// Create multiple objects
var users = dataFactory.CreateMany<User>(5);
```

### Builder Pattern

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

// Usage
var user = new UserBuilder()
    .WithName("John Doe")
    .WithEmail("john@example.com")
    .Build();
```

### Mocking

```csharp
using PeasyPilot.Moq;
using Moq;

var mockFactory = new MockFactory();
var userRepository = mockFactory.Create(typeof(IUserRepository));
```

### Fluent Assertions

```csharp
using PeasyPilot.Core.Assertions;

var result = Calculate(5, 3);

Assert.That(result)
    .IsEqualTo(8)
    .IsNotNull();

Assert.That(errorMessage)
    .IsNotEqualTo("Success")
    .IsNotNull();
```

## Module Details

### PeasyPilot.Core

The foundation of the framework providing:

- `ITestContext` - Thread-safe test data storage
- `ITestDataFactory` - Test data creation abstraction
- `IMockFactory` - Mock creation abstraction
- Fluent assertion API (`Assert.That<T>`)
- Test configuration (`TestOptions`)
- Exception types for testing scenarios

### PeasyPilot.Unit

Unit testing utilities including:

- `BuilderBase<T>` - Abstract builder for the builder pattern
- `UnitTestFixture` - Base fixture class
- Extension methods for fluent configuration
- Helper utilities for test data and object operations

### PeasyPilot.Integration

Integration testing support:

- `IntegrationTestFixture` - Base class with IAsyncLifetime support
- `WebApplicationTestFactory<T>` - ASP.NET Core test host factory
- `ITestDatabase` - Database operations abstraction
- Startup configuration for test applications

### PeasyPilot.Bogus

Fake data generation using the Bogus library:

- `TestDataFactory` - Creates realistic test data
- Implements `ITestDataFactory`
- Generates single or multiple objects

### PeasyPilot.Moq

Mocking framework integration:

- `MockFactory` - Creates mock objects using Moq
- Implements `IMockFactory`
- Type-based mock creation

### PeasyPilot.BDD

Behavior-driven development support:

- `Scenario` - Represents a BDD scenario with Given/When/Then
- `Feature` - Organizes scenarios
- Gherkin format output
- Async execution support

### PeasyPilot.Coverage

Code coverage analysis:

- `CoverageReport` - Tracks line and branch coverage
- `ICoverageProvider` - Provider abstraction
- Coverage percentage calculations

### Framework Adapters (XUnit, NUnit, TUnit)

Framework-specific base classes:

- **XUnit**: `PeasyPilotTestBase` with `IAsyncLifetime`
- **NUnit**: `PeasyPilotNUnitTestBase` with setup/teardown
- **TUnit**: `PeasyPilotTUnitTestBase` with async hooks

All provide:
- Automatic `TestContext` initialization
- Support for test data factories and mocks
- Framework-specific lifecycle management

## Best Practices

### 1. Use Test Fixtures
Inherit from framework-specific base classes to get automatic context setup.

### 2. Leverage Builders
Use the builder pattern for complex object construction:

```csharp
new UserBuilder()
    .WithName("Jane")
    .WithEmail("jane@example.com")
    .Build();
```

### 3. Isolate Tests
Use `TestContext` to isolate test data:

```csharp
var testData = GetOrCreateTestData("user", () => new User { ... });
```

### 4. Use Fluent Assertions
Make assertions more readable:

```csharp
Assert.That(result).IsEqualTo(expected).IsNotNull();
```

### 5. Organize with BDD
Use scenarios for complex business logic testing.

## Configuration

### Service Collection Extension

Register PeasyPilot services:

```csharp
services.AddPeasyPilotCore();
```

With custom options:

```csharp
services.AddPeasyPilotCore(options =>
{
    options.Environment = "Testing";
    options.EnableLogging = false;
});
```

## Project Structure

```
src/
├── PeasyPilot.Core/          # Core abstractions
│   ├── Abstractions/         # Interfaces
│   ├── Assertions/           # Fluent assertion API
│   ├── Context/              # Test context implementation
│   ├── Exceptions/           # Custom exceptions
│   ├── Extensions/           # DI extensions
│   ├── Helpers/              # Type helpers
│   └── Configuration/        # Test configuration
├── PeasyPilot.Unit/          # Unit testing utilities
├── PeasyPilot.Integration/   # Integration testing
├── PeasyPilot.Bogus/         # Fake data generation
├── PeasyPilot.Moq/           # Mocking support
├── PeasyPilot.BDD/           # BDD scenarios
├── PeasyPilot.Coverage/      # Coverage reporting
├── PeasyPilot.XUnit/         # xUnit adapter
├── PeasyPilot.NUnit/         # NUnit adapter
└── PeasyPilot.TUnit/         # TUnit adapter
tests/
└── PeasyPilot.Core.Tests/    # Core tests
```

## Target Framework

- **.NET 10.0** - Built on the latest .NET platform
- C# 13 with nullable reference types enabled
- Full async/await support

## Dependencies

- **Core**: Microsoft.Extensions.DependencyInjection
- **Bogus**: Bogus (for fake data)
- **Moq**: Moq (for mocking)
- **Integration**: Microsoft.AspNetCore.Mvc.Testing
- **XUnit**: xUnit
- **NUnit**: NUnit
- **TUnit**: TUnit

## Contributing

Contributions are welcome! Areas for improvement:

- Additional test framework adapters
- Performance improvements
- Extended helper utilities
- Documentation and examples

## License

PeasyPilot is provided as-is for testing purposes.

## Support

For issues or questions:
1. Check existing documentation
2. Review example tests in the repository
3. Consult individual module documentation

## Roadmap

- [ ] Additional framework adapters (TestNG-style)
- [ ] Performance profiling utilities
- [ ] Distributed testing support
- [ ] Advanced mocking scenarios
- [ ] Integration with popular CI/CD platforms
- [ ] Visual test reporting
- [ ] Property-based testing support

## Versioning

PeasyPilot follows semantic versioning:
- Major: Breaking changes
- Minor: New features
- Patch: Bug fixes

Current version: 1.0.0
