# PeasyPilot TestAssistant — Guide Complet

Documentation claire et pédagogique sur le fonctionnement et l'utilisation de TestAssistant.

---

## Table des matières

1. [Qu'est-ce que TestAssistant ?](#quest-ce-que-testassistant-)
2. [Comment ça marche - Pipeline complet](#comment-ça-marche---pipeline-complet)
3. [Étape 1 : Analyse (Analysis)](#étape-1--analyse-analysis)
4. [Étape 2 : Génération de propositions](#étape-2--génération-de-propositions)
5. [Étape 3 : Composition](#étape-3--composition)
6. [Étape 4 : Rendu (Rendering)](#étape-4--rendu-rendering)
7. [Comment l'utiliser - Guides pratiques](#comment-lutiliser---guides-pratiques)
8. [Concepts clés](#concepts-clés)
9. [Exemples concrets](#exemples-concrets)

---

## Qu'est-ce que TestAssistant ?

**TestAssistant** est un système intelligent qui génère automatiquement des cas de test pour vos classes C#.

### Ce qu'il fait :

1. **Examine** votre classe via réflexion .NET
2. **Identifie** les constructeurs et les méthodes
3. **Propose** des cas de test (nominal + limites)
4. **Génère** du code de test (xUnit/NUnit/TUnit)

### Pourquoi c'est utile ?

- ⚡ Génère les cas de test en quelques secondes
- 🎯 Détecte automatiquement les cas limites (edge cases)
- 🔄 Supporte 3 frameworks (xUnit, NUnit, TUnit)
- 📝 Code généré = point de départ pour vous, pas de solution clés en main

---

## Comment ça marche - Pipeline complet

```
Votre classe (Calculator.cs)
        ↓
    [ANALYZE]
    Examiner via réflexion
        ↓
  [GENERATE VALUES]
  Créer les cas de test
        ↓
    [COMPOSE]
  Organiser les cas
        ↓
    [RENDER]
  Générer du C# xUnit/NUnit/TUnit
        ↓
    TestFile.cs (code généré)
```

Détaillons chaque étape.

---

## Étape 1 : Analyse (Analysis)

### Que se passe-t-il ?

TestAssistant utilise **la réflexion .NET** pour examiner votre classe :

1. **Trouve le constructeur**
2. **Identifie les paramètres du constructeur**
3. **Scanne les méthodes publiques**
4. **Résout les types des paramètres**

### Exemple concret

```csharp
// Votre classe
public class Calculator
{
    // Constructeur
    public Calculator(ILogger logger)
    {
    }

    // Méthodes
    public int Add(int a, int b) => a + b;
    public int Divide(int a, int b) => a / b;
}
```

**Ce que TestAssistant découvre :**

```
Classe: Calculator

Constructeur:
  - Paramètre 1: "logger" de type "ILogger"
    → Résolution: Interface → besoin d'une mock

Méthodes publiques:
  - Méthode 1: Add(int, int) → retourne int
  - Méthode 2: Divide(int, int) → retourne int
```

### Résolution des paramètres

Pour chaque paramètre, TestAssistant décide **comment le générer** :

| Type | Résolution | Exemple |
| --- |-----------|---------|
| `int` | Valeur primitive | Génère: 0, 1, -1, int.MaxValue |
| `string` | Valeur primitive | Génère: "", "a", "texte long" |
| `ILogger` | Interface → Mock | `MockFactory.Create(typeof(ILogger))` |
| `MyRepository` | Classe concrète | `new MyRepository()` |
| `DateTime` | Valeur primitive | Génère: min, epoch, now, max |

---

## Étape 2 : Génération de propositions

### Que se passe-t-il ?

Pour chaque type de paramètre, TestAssistant génère **plusieurs valeurs** :

### Valeurs générées par type

**Pour `int` :**
```
Nominal:  0
Limites:  -1, 1, int.MinValue, int.MaxValue
```

**Pour `string` :**
```
Nominal:  "sample"
Limites:  "", "a", "texte très long avec 100 caractères..."
```

**Pour `bool` :**
```
Nominal:  true
Limites:  false
```

**Pour `DateTime` :**
```
Nominal:  DateTime.UtcNow
Limites:  DateTime.MinValue, DateTime.MaxValue, new DateTime(1970, 1, 1)
```

**Pour `Enum` :**
```
Génère TOUTES les valeurs de l'enum
Si Color = { Red, Green, Blue } → génère 3 cas
```

**Pour `Decimal` :**
```
Nominal:  0m
Limites:  -0.01m, 0.01m, decimal.MinValue, decimal.MaxValue
```

### Matrice de combinaisons

Si votre constructeur a **2 paramètres** :

```csharp
public Calculator(int precision, string format)
```

TestAssistant crée une **matrice** de cas de test :

```
Cas 1: precision=0,        format=""           → Cas limites
Cas 2: precision=1,        format="a"          → Cas limites
Cas 3: precision=int.Max,  format="très long"  → Cas limites
```

### Structure de chaque cas généré

```json
{
  "testName": "Calculator_CanInstantiate",
  "methodName": "Calculator",
  "description": "Cas nominal : peut instancier la classe",
  "category": "nominal",
  "parameterValues": {
    "logger": {
      "name": "logger",
      "type": "ILogger",
      "expression": "MockFactory.Create(...)"
    }
  }
}
```

---

## Étape 3 : Composition

### Que se passe-t-il ?

TestAssistant **organise** tous les cas générés dans une structure :

```csharp
public class TestBatteryProposal
{
    public string TargetType { get; set; }           // "Calculator"
    public string TargetNamespace { get; set; }      // "MyApp.Math"
    public string Framework { get; set; }            // "xunit"
    public List<TestCaseProposal> TestCases { get; set; }  // Tous les cas
    public DateTime GeneratedAt { get; set; }
}
```

### Exemple complet

```json
{
  "targetType": "Calculator",
  "targetNamespace": "MyApp.Math",
  "framework": "xunit",
  "generatedAt": "2026-09-09T10:30:00Z",
  "testCases": [
    {
      "testName": "Calculator_CanInstantiate",
      "description": "Cas nominal",
      "category": "nominal",
      "parameterValues": { ... }
    },
    {
      "testName": "Add_HappyPath",
      "description": "Chemin heureux pour Add",
      "category": "nominal",
      "parameterValues": { ... }
    }
  ]
}
```

Cette structure est **prête pour le rendu**.

---

## Étape 4 : Rendu (Rendering)

### Que se passe-t-il ?

TestAssistant convertit la `TestBatteryProposal` en **code C# réel**.

Il utilise un **renderer différent** selon le framework :

- `XUnitTestBatteryRenderer` → xUnit
- `NUnitTestBatteryRenderer` → NUnit
- `TUnitTestBatteryRenderer` → TUnit

### Exemple : Rendu xUnit

**Entrée :** TestBatteryProposal

**Sortie (C# généré) :**

```csharp
using Xunit;
using PeasyPilot.XUnit;
using PeasyPilot.Moq;

namespace MyApp.Math.Tests;

public class CalculatorTests : PeasyPilotTestBase
{
    private Calculator _subject = null!;

    public override void Setup()
    {
        base.Setup();
        var logger = new MockFactory().Create(typeof(ILogger));
        _subject = new Calculator(logger);
    }

    [Fact]
    public void Calculator_CanInstantiate()
    {
        // Cas nominal : peut instancier la classe
        // TODO: Implémenter le test
        Assert.NotNull(_subject);
    }

    [Fact]
    public void Add_HappyPath()
    {
        // Chemin heureux pour Add
        // TODO: Implémenter le test
        Assert.NotNull(_subject);
    }
}
```

### Exemple : Rendu NUnit

**Même logique, syntaxe NUnit :**

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;
using PeasyPilot.Moq;

namespace MyApp.Math.Tests;

[TestFixture]
public class CalculatorTests : PeasyPilotTestBase
{
    private Calculator _subject = null!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        var logger = new MockFactory().Create(typeof(ILogger));
        _subject = new Calculator(logger);
    }

    [Test]
    public void Calculator_CanInstantiate()
    {
        // Cas nominal : peut instancier la classe
        // TODO: Implémenter le test
        Assert.NotNull(_subject);
    }

    [Test]
    public void Add_HappyPath()
    {
        // Chemin heureux pour Add
        // TODO: Implémenter le test
        Assert.NotNull(_subject);
    }
}
```

---

## Comment l'utiliser - Guides pratiques

### Utilisation basique

```csharp
using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Rendering;
using PeasyPilot.TestAssistant.Models;

// Étape 1 : Créer un analyseur
var analyzer = new ReflectionTestScenarioAnalyzer();

// Étape 2 : Définir les options
var options = new TestBatteryAnalysisOptions
{
    TargetFramework = "xunit",    // ou "nunit", "tunit"
    MaxEnumCases = 10,
    IncludeBoundaryTests = true
};

// Étape 3 : Analyser votre classe
var proposal = analyzer.Analyze(typeof(Calculator), options);

// Étape 4 : Rendre le code
var registry = new TestBatteryRendererRegistry();
var renderer = registry.GetRenderer("xunit");

var renderOptions = new RenderOptions
{
    OutputNamespace = "MyApp.Tests",
    Indent = "    "
};

string generatedCode = renderer.Render(proposal, renderOptions);

// Étape 5 : Écrire dans un fichier
File.WriteAllText("CalculatorTests.cs", generatedCode);
```

### Génération sans fichier

```csharp
// Récupérer directement le code généré
string testCode = renderer.Render(proposal, renderOptions);

// Utiliser ou afficher
Console.WriteLine(testCode);
```

### Guide complet : Du zéro à la génération

**Étape 1 : Installer le package**

```bash
dotnet add package PeasyPilot.TestAssistant
```

**Étape 2 : Créer une classe à tester**

```csharp
public class UserValidator
{
    public UserValidator(ILogger logger) { }

    public bool ValidateEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@");
    }
}
```

**Étape 3 : Générer les tests**

```csharp
var analyzer = new ReflectionTestScenarioAnalyzer();
var options = new TestBatteryAnalysisOptions { TargetFramework = "xunit" };

var proposal = analyzer.Analyze(typeof(UserValidator), options);

var renderer = new TestBatteryRendererRegistry().GetRenderer("xunit");
var code = renderer.Render(proposal, new RenderOptions 
{ 
    OutputNamespace = "MyApp.Tests" 
});

File.WriteAllText("UserValidatorTests.cs", code);
```

**Étape 4 : Implémenter les tests générés**

Le fichier généré contient des TODOs. Vous complétez :

```csharp
[Fact]
public void ValidateEmail_WithValidEmail_ReturnsTrue()
{
    // TODO: Implémenter le test
    var result = _subject.ValidateEmail("user@example.com");
    Assert.True(result);
}
```

---

## Concepts clés

### Catégories de test

Chaque cas généré a une **catégorie** :

| Catégorie | Signification | Exemple |
| --- |--------------|---------|
| **nominal** | Cas heureux (chemin heureux) | `Add(2, 3)` |
| **boundary** | Cas limites (edge cases) | `Add(0, int.MaxValue)` |
| **exception** | Cas d'erreur | Paramètre null |

### Stratégies de résolution

Pour chaque paramètre, TestAssistant applique une **stratégie** :

```csharp
public enum ParameterResolutionKind
{
    Primitive,                    // int, string, bool, DateTime, etc.
    ConcreteNewable,             // MyClass : peut faire new
    InterfaceOrAbstractNeedsMock, // ILogger : besoin mock
    Unresolvable                 // Type complexe, dépendance manquante
}
```

### Règles de génération (Value Generation Rules)

TestAssistant utilise des **règles** pour générer les valeurs :

- `NumericValueRule` → `int`, `long`, `decimal`, `double`, `float`
- `StringValueRule` → `string`
- `BooleanValueRule` → `bool`
- `DateTimeValueRule` → `DateTime`
- `GuidValueRule` → `Guid`
- `EnumValueRule` → Tous les `Enum`
- `CollectionValueRule` → `IEnumerable<T>`, `List<T>`, etc.
- `NullableValueRule` → Types `Nullable<T>`
- `FallbackValueRule` → Autres types

---

## Exemples concrets

### Exemple 1 : Classe simple sans dépendances

**Votre classe :**

```csharp
public class StringUtils
{
    public string Reverse(string text) => 
        new string(text.Reverse().ToArray());
    
    public bool IsEmpty(string text) => 
        string.IsNullOrEmpty(text);
}
```

**Code généré (xUnit) :**

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class StringUtilsTests : PeasyPilotTestBase
{
    private StringUtils _subject = null!;

    public override void Setup()
    {
        base.Setup();
        _subject = new StringUtils();
    }

    [Fact]
    public void StringUtils_CanInstantiate()
    {
        Assert.NotNull(_subject);
    }

    [Fact]
    public void Reverse_HappyPath()
    {
        // TODO: Implémenter le test
        Assert.NotNull(_subject);
    }

    [Fact]
    public void IsEmpty_HappyPath()
    {
        // TODO: Implémenter le test
        Assert.NotNull(_subject);
    }
}
```

**Ce que vous devez implémenter :**

```csharp
[Fact]
public void Reverse_HappyPath()
{
    var result = _subject.Reverse("hello");
    Assert.Equal("olleh", result);
}

[Fact]
public void IsEmpty_HappyPath()
{
    var result = _subject.IsEmpty("");
    Assert.True(result);
}
```

### Exemple 2 : Classe avec dépendances

**Votre classe :**

```csharp
public class UserService
{
    public UserService(IUserRepository repository, IEmailSender emailSender)
    {
    }

    public async Task<User> CreateUserAsync(string email)
    {
        // ...
    }
}
```

**Code généré (xUnit) :**

```csharp
using Xunit;
using PeasyPilot.XUnit;
using PeasyPilot.Moq;

namespace MyApp.Tests;

public class UserServiceTests : PeasyPilotTestBase
{
    private UserService _subject = null!;

    public override void Setup()
    {
        base.Setup();
        var repository = new MockFactory().Create(typeof(IUserRepository));
        var emailSender = new MockFactory().Create(typeof(IEmailSender));
        _subject = new UserService(repository, emailSender);
    }

    [Fact]
    public void UserService_CanInstantiate()
    {
        Assert.NotNull(_subject);
    }

    [Fact]
    public async Task CreateUserAsync_HappyPath()
    {
        // TODO: Implémenter le test
        Assert.NotNull(_subject);
    }
}
```

### Exemple 3 : Classe avec paramètres complexes

**Votre classe :**

```csharp
public class PriceCalculator
{
    public decimal Calculate(
        decimal basePrice, 
        int quantity, 
        bool applyDiscount)
    {
        var total = basePrice * quantity;
        return applyDiscount ? total * 0.9m : total;
    }
}
```

**TestAssistant génère ces cas :**

```
Cas 1: basePrice=0m,          quantity=0, applyDiscount=true
Cas 2: basePrice=0.01m,       quantity=1, applyDiscount=false
Cas 3: basePrice=decimal.Max, quantity=int.Max, applyDiscount=true
```

**Vous implémentez :**

```csharp
[Fact]
public void Calculate_WithDiscount_AppliesPercentage()
{
    var result = _subject.Calculate(100m, 2, true);
    Assert.Equal(180m, result);  // 100 * 2 * 0.9
}
```

---

## Résumé : Le cycle complet

```
1️⃣  ANALYZE
    └─ Utiliser la réflexion pour examiner la classe
    └─ Identifier constructeur, paramètres, méthodes
    └─ Résoudre les types (primitive, interface, classe)

2️⃣  GENERATE
    └─ Pour chaque paramètre, générer plusieurs valeurs
    └─ Valeurs nominales (cas heureux)
    └─ Valeurs limites (edge cases)

3️⃣  COMPOSE
    └─ Organiser tous les cas dans une structure
    └─ Créer une TestBatteryProposal

4️⃣  RENDER
    └─ Choisir le framework (xUnit/NUnit/TUnit)
    └─ Générer du C# réel
    └─ Écrire dans un fichier

5️⃣  REVIEW & IMPLEMENT
    └─ Lire le code généré
    └─ Remplir les TODOs avec de vrais tests
    └─ Exécuter et valider
```

---

## Points clés à retenir

✅ **TestAssistant génère l'échafaudage** — pas une solution complète
✅ **Vous complétez les TODOs** — avec votre logique métier
✅ **Cas nominaux + limites** — couverture automatique des edge cases
✅ **3 frameworks** — xUnit, NUnit, TUnit, même logique
✅ **Réflexion intelligente** — identifie interfaces, types primitifs, classes

---

**Conclusion :** TestAssistant vous fait gagner du temps en générant l'échafaudage. Vous vous concentrez sur la logique métier des tests. C'est une collaboration ! 🤝
