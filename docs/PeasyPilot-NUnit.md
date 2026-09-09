# PeasyPilot.NUnit

NUnit framework adapter for PeasyPilot.

```csharp
using PeasyPilot.NUnit;
using NUnit.Framework;

[TestFixture]
public class CalculatorTests : PeasyPilotNUnitTestBase
{
    [Test]
    public void Add_ReturnsCorrectSum()
    {
        var result = new Calculator().Add(2, 3);
        Assert.That(result, Is.EqualTo(5));
    }
}
```

## Features
- ✅ NUnit lifecycle integration
- ✅ DI container support
- ✅ Fluent assertions
- ✅ Integration testing

## Install
```bash
dotnet add package PeasyPilot.NUnit
```

**See:** PeasyPilot-Core
