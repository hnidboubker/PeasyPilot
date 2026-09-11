# Guide de Génération de Tests

## Aperçu

TestAssistant analyse votre code et génère automatiquement des cas de test. Gagnez du temps en écrivant les tests boilerplate.

**Prérequis:** [Guide des Tests Unitaires](./unit-testing-guide-FR.md)  
**Durée:** 20 minutes  

---

## Comment Ça Fonctionne

1. **Analyser** – La réflexion scanne vos types (méthodes, paramètres, dépendances)
2. **Générer** – Crée des cas de test (chemin heureux, limites, erreurs)
3. **Rendre** – Produit du code spécifique au framework (xUnit/NUnit/TUnit)
4. **Personnaliser** – Modifiez les tests générés selon vos besoins

---

## Utilisation Basique

```csharp
using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Rendering;

// Votre classe à tester
public class Calculator
{
    public int Add(int a, int b) => a + b;
    public decimal Divide(decimal a, decimal b) => a / b;
}

// Étape 1: Analyser
var analyzer = new ReflectionTestScenarioAnalyzer();
var options = new TestBatteryAnalysisOptions
{
    TargetFramework = "xunit",
    IncludeBoundaryTests = true,
    MaxEnumCases = 10
};
var proposal = analyzer.Analyze(typeof(Calculator), options);

// Étape 2: Rendre
var registry = new TestBatteryRendererRegistry();
var renderer = registry.GetRenderer("xunit");
var renderOptions = new RenderOptions { OutputNamespace = "MonApp.Tests" };
string generatedCode = renderer.Render(proposal, renderOptions);

// Étape 3: Écrire dans un fichier
File.WriteAllText("CalculatorTests.cs", generatedCode);
```

---

## Résultat Généré Exemple

```csharp
using Xunit;
using PeasyPilot.XUnit;
using MonApp;

namespace MonApp.Tests;

public class CalculatorTests : PeasyPilotTestBase
{
    private Calculator _subject = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _subject = new Calculator();
    }
    
    [Fact]
    public void Add_HappyPath()
    {
        var result = _subject.Add(5, 3);
        Assert.Equal(8, result);
    }
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(int.MinValue, 0)]
    public void Add_BoundaryValues(int a, int b)
    {
        var result = _subject.Add(a, b);
        Assert.NotNull(result);
    }
    
    [Fact]
    public void Divide_ByZero_ThrowsException()
    {
        Assert.Throws<DivideByZeroException>(() => _subject.Divide(10, 0));
    }
}
```

---

## Scénarios Supportés

TestAssistant détecte et génère :

- **Chemin heureux** – Opération normale
- **Valeurs limites** – Min/max, zéro, chaînes vides
- **Entrées nulles** – Gestion des paramètres null
- **Exceptions** – Conditions d'erreur et messages
- **Méthodes asynchrones** – Patterns async/await
- **Dépendances** – Injection de constructeur, mocking

---

## Options de Personnalisation

### Sélection du Framework
```csharp
var renderer = registry.GetRenderer("xunit");   // xUnit
var renderer = registry.GetRenderer("nunit");   // NUnit
var renderer = registry.GetRenderer("tunit");   // TUnit
```

### Options d'Analyse
```csharp
var options = new TestBatteryAnalysisOptions
{
    TargetFramework = "xunit",
    IncludeBoundaryTests = true,    // Générer les tests limites
    IncludeNullTests = true,         // Tester les entrées null
    IncludeExceptionTests = true,    // Tester les conditions d'erreur
    MaxEnumCases = 10,               // Limiter les cas enum
    MaxCollectionSize = 5            // Limiter la taille des collections
};
```

---

## Intégration dans le Workflow

### Dans Votre Build
```bash
dotnet run --project TestAssistant.Generator -- --input "src/MyClass.cs" --output "tests/"
```

### Dans CI/CD
```yaml
- name: Générer les tests
  run: dotnet run --project TestAssistant.Generator -- --input "src/" --output "tests/"

- name: Exécuter tous les tests
  run: dotnet test
```

---

## Meilleures Pratiques

✅ **FAIRE**
- Revoir les tests générés avant de committer
- Personnaliser les données de test
- Utiliser les tests générés comme point de départ
- Garder les tests manuels et générés synchronisés
- Régénérer au fur et à mesure des changements de code

❌ **NE PAS FAIRE**
- Dépendre entièrement des tests générés
- Ignorer les cas limites que TestAssistant a manqués
- Régénérer sans revoir les changements
- Committer du code généré non testé

---

## Prochaines Étapes

📖 **[Adaptateurs de Framework](./framework-adapters-guide-FR.md)** – Choisir votre framework  
📖 **[Guide des Tests Unitaires](./unit-testing-guide-FR.md)** – Écriture manuelle  

Les tests générés accélèrent le développement tout en maintenant la qualité! ⚡
