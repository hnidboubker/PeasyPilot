# PeasyPilot.TUnit

TUnit framework adapter for PeasyPilot.

```csharp
using PeasyPilot.TUnit;
using TUnit.Core;

public class CalculatorTests : PeasyPilotTUnitTestBase
{
    [Test]
    public async Task Add_ReturnsCorrectSum()
    {
        var result = new Calculator().Add(2, 3);
        await Assert.That(result).IsEqualTo(5);
    }
}
```

## Features
- ✅ TUnit async support
- ✅ DI integration
- ✅ Modern assertions
- ✅ Integration testing

## Install
```bash
dotnet add package PeasyPilot.TUnit
```

**See:** PeasyPilot-Core
