# PeasyPilot.TUnit

Adapter TUnit pour PeasyPilot.

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

## Fonctionnalités
- ✅ Support async
- ✅ Intégration DI
- ✅ Assertions modernes
- ✅ Tests d'intégration

## Installation
```bash
dotnet add package PeasyPilot.TUnit
```

**Voir :** PeasyPilot-Core
