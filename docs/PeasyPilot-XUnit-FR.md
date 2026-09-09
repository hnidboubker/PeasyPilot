# PeasyPilot.XUnit — Guide Complet

Adapter xUnit pour PeasyPilot avec intégration de classe de base.

## Démarrage rapide

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

## Classes de base

### PeasyPilotTestBase
Base pour tests unitaires avec support DI.

### XUnitIntegrationTestFixture
Pour tests d'intégration avec cycle de vie complet.

## Fonctionnalités

- ✅ Intégration DI
- ✅ Hooks de cycle de vie
- ✅ Assertions fluentes
- ✅ Support fixtures BD

## Installation

```bash
dotnet add package PeasyPilot.XUnit
```

**Voir aussi :** PeasyPilot-Core, PeasyPilot-Integration
