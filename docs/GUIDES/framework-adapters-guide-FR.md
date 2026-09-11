# Guide des Adaptateurs de Framework

## Aperçu

PeasyPilot fonctionne avec trois frameworks de test : **xUnit**, **NUnit** et **TUnit**. Choisissez selon la préférence de votre équipe.

**Durée:** 15 minutes  

---

## Comparaison

| Fonctionnalité | xUnit | NUnit | TUnit |
|---|---|---|---|
| Async-native | ✅ | ✅ | ✅✅ |
| Setup/Teardown | `IAsyncLifetime` | `[SetUp]` | `Hooks` |
| Assertions | Personnalisées | Intégrées | Intégrées |
| Parallélisation | ✅ | ✅ | ✅✅ |
| .NET Moderne | ✅✅ | ✅ | ✅✅ |

---

## xUnit (Recommandé pour .NET Moderne)

**Meilleur pour:** Projets modernes greenfield

```csharp
using Xunit;
using PeasyPilot.XUnit;

public class CalculatorTests : PeasyPilotTestBase
{
    private Calculator _calculator = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _calculator = new Calculator();
    }
    
    [Fact]
    public void Add_ReturnSum() => Assert.Equal(8, _calculator.Add(5, 3));
    
    [Theory]
    [InlineData(5, 3, 8)]
    [InlineData(0, 0, 0)]
    public void Add_WithData(int a, int b, int expected) 
        => Assert.Equal(expected, _calculator.Add(a, b));
}
```

**Installer:**
```bash
dotnet add package PeasyPilot.XUnit
```

---

## NUnit (Familier pour Enterprise)

**Meilleur pour:** Projets existants, équipes connaissant NUnit

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

[TestFixture]
public class CalculatorTests : PeasyPilotNUnitTestBase
{
    private Calculator _calculator = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _calculator = new Calculator();
    }
    
    [Test]
    public void Add_ReturnSum() => Assert.That(_calculator.Add(5, 3), Is.EqualTo(8));
    
    [TestCase(5, 3, 8)]
    [TestCase(0, 0, 0)]
    public void Add_WithData(int a, int b, int expected)
        => Assert.That(_calculator.Add(a, b), Is.EqualTo(expected));
}
```

**Installer:**
```bash
dotnet add package PeasyPilot.NUnit
```

---

## TUnit (Plus Moderne)

**Meilleur pour:** Projets modernes prioritarisant la performance

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

public class CalculatorTests : PeasyPilotTUnitTestBase
{
    private Calculator _calculator = null!;
    
    [Before(Test)]
    public async Task Setup()
    {
        await InitializeAsync();
        _calculator = new Calculator();
    }
    
    [Test]
    public async Task Add_ReturnSum()
    {
        var result = _calculator.Add(5, 3);
        await Assert.That(result).IsEqualTo(8);
    }
}
```

**Installer:**
```bash
dotnet add package PeasyPilot.TUnit
```

---

## Comparaison du Cycle de Vie

### xUnit (IAsyncLifetime)
```csharp
public override async Task InitializeAsync()  // Avant chaque test
{
    await base.InitializeAsync();
}

public override async Task DisposeAsync()     // Après chaque test
{
    await base.DisposeAsync();
}
```

### NUnit ([SetUp] / [TearDown])
```csharp
[SetUp]
public override void Setup()      // Avant chaque test
{
    base.Setup();
}

[TearDown]
public override void TearDown()   // Après chaque test
{
    base.TearDown();
}
```

### TUnit (Hooks)
```csharp
[Before(Test)]
public async Task SetupAsync()    // Avant chaque test
{
}

[After(Test)]
public async Task CleanupAsync()  // Après chaque test
{
}
```

---

## Différences d'Assertions

### xUnit
```csharp
Assert.Equal(expected, actual);
Assert.NotNull(value);
Assert.True(condition);
```

### NUnit
```csharp
Assert.That(actual, Is.EqualTo(expected));
Assert.That(value, Is.Not.Null);
Assert.That(condition, Is.True);
```

### TUnit
```csharp
await Assert.That(actual).IsEqualTo(expected);
await Assert.That(value).IsNotNull();
await Assert.That(condition).IsTrue();
```

---

## Migration Entre Frameworks

Pour passer de xUnit à NUnit :

1. **Changer la classe de base**
   ```csharp
   // De
   public class Tests : PeasyPilotTestBase
   
   // À
   public class Tests : PeasyPilotNUnitTestBase
   ```

2. **Changer les attributs de test**
   ```csharp
   // De
   [Fact]
   
   // À
   [Test]
   ```

3. **Mettre à jour les méthodes de cycle de vie**
   ```csharp
   // De
   public override async Task InitializeAsync()
   
   // À
   [SetUp]
   public override void Setup()
   ```

4. **Mettre à jour les assertions**
   ```csharp
   // De
   Assert.Equal(8, result);
   
   // À
   Assert.That(result, Is.EqualTo(8));
   ```

---

## Meilleures Pratiques

✅ **FAIRE**
- Choisir un framework par projet
- Utiliser les patterns du framework consistemment
- Exploiter les fonctionnalités async
- Utiliser les tests paramétrés

❌ **NE PAS FAIRE**
- Mélanger les frameworks dans un même projet
- Ignorer les meilleures pratiques du framework
- Utiliser les patterns synchrones quand async est disponible
- Oublier d'appeler `base.Setup()` / `base.InitializeAsync()`

---

## Choisir un Framework

**→ xUnit** si :
- Projets modernes greenfield
- Équipe préfère la configuration minimale
- Utilisation d'ASP.NET Core

**→ NUnit** si :
- Migration de projets existants
- Équipe connaît NUnit
- Besoin de logic setup/teardown complexe

**→ TUnit** si :
- Priorité à la vitesse d'exécution
- Patterns async modernes
- Test suites haute performance

---

## Prochaines Étapes

📖 **[Guide des Tests Unitaires](./unit-testing-guide-FR.md)** – Écrire les tests  
📖 **[Guide des Tests d'Intégration](./integration-testing-guide-FR.md)** – Patterns d'intégration  
📖 **[Guide BDD](./bdd-testing-guide-FR.md)** – Tests comportementaux  
📖 **[Guide de Génération de Tests](./test-generation-guide-FR.md)** – Générer les cas de tests  

Tous les trois frameworks fonctionnent magnifiquement avec PeasyPilot! Choisissez votre préféré. 🚀

---

**[← Retour aux Guides d'Apprentissage](./README.md)** | **[← Retour au Hub Documentation](../README.md)**

**Version:** Français | **[English](./framework-adapters-guide.md)**
