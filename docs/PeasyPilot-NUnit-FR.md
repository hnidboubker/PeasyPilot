# PeasyPilot.NUnit

Adapter NUnit pour PeasyPilot.

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

## Fonctionnalités
- ✅ Intégration NUnit
- ✅ Support DI
- ✅ Assertions fluentes
- ✅ Tests d'intégration

## Installation
```bash
dotnet add package PeasyPilot.NUnit
```

**Voir :** PeasyPilot-Core
