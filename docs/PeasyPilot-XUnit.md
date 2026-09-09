# PeasyPilot.XUnit

xUnit framework adapter for PeasyPilot with base class integration.

## Quick Start

```csharp
using PeasyPilot.XUnit;
using Xunit;

public class CalculatorTests : PeasyPilotTestBase
{
    [Fact]
    public void Add_WithValidNumbers_ReturnsCorrectSum()
    {
        var calc = new Calculator();
        var result = calc.Add(2, 3);
        Assert.Equal(5, result);
    }
}
```

## Base Classes

### PeasyPilotTestBase

Standard base class for unit tests with DI support.

```csharp
public class MyTests : PeasyPilotTestBase
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IMyService, MyService>();
    }

    public override void Setup() { /* runs before each test */ }
}
```

### XUnitIntegrationTestFixture

For integration tests with database and full lifecycle management.

```csharp
public class IntegrationTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task TestDatabase()
    {
        var db = Database;
        await db.InitializeAsync();
    }
}
```

## Features

- ✅ DI container integration
- ✅ Test lifecycle hooks (Setup, Teardown)
- ✅ Fluent assertions
- ✅ Database fixture support
- ✅ Integration testing helpers

## Installation

```bash
dotnet add package PeasyPilot.XUnit
```

## Learn More

- [PeasyPilot-Core](./PeasyPilot-Core.md)
- [PeasyPilot-Integration](./PeasyPilot-Integration.md)
