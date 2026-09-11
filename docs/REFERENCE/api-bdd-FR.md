# Référence API PeasyPilot.BDD

## Aperçu

`PeasyPilot.BDD` fournit un framework Behavior-Driven Development (BDD) complet pour écrire des spécifications exécutables en syntaxe Gherkin. Il vous permet d'exprimer des scénarios de test en format Given-When-Then lisible par les humains, de charger des fichiers de fonctionnalités depuis le disque, et d'exécuter des scénarios avec support complet de la liaison d'étapes et intégration d'injection de dépendances.

**Responsabilités principales :**
- Modélisation des fonctionnalités et scénarios Gherkin
- Chargement des fichiers de fonctionnalités depuis le disque (fichiers uniques et répertoires)
- Analyse de texte Gherkin en graphes d'objets Feature
- Exécution de scénarios avec résolution de liaison d'étapes
- Découverte de définitions d'étapes et correspondance de motifs via attributs
- Exportation de documentation vivante (Markdown)
- Intégration avec les modèles de test PeasyPilot.Core

**Cibles :** .NET 8.0, 9.0, 10.0

---

## Abstractions Principales

### IFeatureFileLoader

Charge les fichiers de fonctionnalités Gherkin depuis le disque et les convertit en objets Feature.

```csharp
namespace PeasyPilot.BDD.FileLoading;

/// <summary>
/// Charge les fichiers de fonctionnalités Gherkin depuis le disque et les convertit en objets Feature.
/// </summary>
public interface IFeatureFileLoader
{
    /// <summary>
    /// Charge tous les fichiers .feature d'un répertoire récursivement.
    /// </summary>
    /// <param name="directoryPath">Chemin vers le répertoire des fonctionnalités.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Collection des fonctionnalités chargées.</returns>
    Task<IReadOnlyList<Feature>> LoadFromDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Charge un fichier .feature unique.
    /// </summary>
    /// <param name="filePath">Chemin vers le fichier de fonctionnalité.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Fonctionnalité chargée.</returns>
    Task<Feature> LoadFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}
```

**Objectif :** Fournit un chargement de fichiers de fonctionnalités indépendant du framework. L'implémentation (`GherkinFeatureFileLoader`) analyse les répertoires récursivement et analyse les fichiers .feature en graphes d'objets Feature.

---

### IScenarioExecutor

Exécute les scénarios BDD étape par étape avec résolution optionnelle de liaison d'étapes.

```csharp
namespace PeasyPilot.BDD.Execution;

/// <summary>
/// Exécute les scénarios BDD avec support d'injection de dépendances.
/// </summary>
public interface IScenarioExecutor
{
    /// <summary>
    /// Exécute un scénario en utilisant le fournisseur de services fourni pour la résolution d'étapes.
    /// </summary>
    /// <param name="scenario">Scénario à exécuter.</param>
    /// <param name="serviceProvider">Fournisseur de services pour les définitions d'étapes.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Résultat d'exécution avec détails des étapes.</returns>
    Task<ScenarioExecutionResult> ExecuteAsync(
        Scenario scenario,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);
}
```

**Objectif :** Exécute les scénarios étape par étape, résout les liaisons d'étapes via les attributs, gère les erreurs élégamment et suit les résultats d'exécution.

---

### IStepBindingResolver

Découvre et résout les attributs de définition d'étapes en actions exécutables.

```csharp
namespace PeasyPilot.BDD.StepDefinitions;

/// <summary>
/// Résout les attributs de liaison d'étapes en actions exécutables.
/// </summary>
public interface IStepBindingResolver
{
    /// <summary>
    /// Résout un texte d'étape en action exécutable.
    /// </summary>
    /// <param name="stepType">Le type d'étape (Given, When, Then).</param>
    /// <param name="stepText">Le texte d'étape à faire correspondre.</param>
    /// <param name="serviceProvider">Fournisseur de services pour la résolution de dépendances.</param>
    /// <returns>Une Func&lt;Task&gt; exécutable ou null si aucune correspondance n'est trouvée.</returns>
    Func<Task>? ResolveStep(StepType stepType, string stepText, IServiceProvider serviceProvider);
}
```

**Objectif :** Découvre les classes de définition d'étapes via la réflexion, fait correspondre les motifs de texte d'étape contre les attributs [Given], [When], [Then], et retourne les actions d'étapes exécutables avec extraction de paramètres.

---

### GherkinFeatureParser

Analyseur de syntaxe Gherkin natif sans dépendances externes.

```csharp
namespace PeasyPilot.BDD;

/// <summary>
/// Analyseur natif de texte de fonctionnalité Gherkin convertissant les spécifications Gherkin en instances Feature PeasyPilot.
/// </summary>
public static class GherkinFeatureParser
{
    /// <summary>
    /// Analyse une chaîne de spécification Gherkin en graphe d'objets Feature.
    /// </summary>
    /// <param name="gherkinContent">Contenu texte Gherkin.</param>
    /// <returns>Instance Feature analysée.</returns>
    public static Feature Parse(string gherkinContent);
}
```

**Objectif :** Analyse la syntaxe Gherkin en graphes Feature → Scenario → Step sans dépendances externes. Gère les mots-clés Feature, Scenario, Given, When, Then, And, But.

---

## Modèles Principaux

### Feature

Conteneur pour les scénarios BDD connexes.

```csharp
public class Feature
{
    /// <summary>
    /// Obtient le nom de la fonctionnalité.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Obtient la liste en lecture seule des scénarios.
    /// </summary>
    public IReadOnlyList<Scenario> Scenarios { get; }

    /// <summary>
    /// Initialise une nouvelle Feature.
    /// </summary>
    public Feature(string name);

    /// <summary>
    /// Ajoute un scénario à cette fonctionnalité.
    /// </summary>
    public Scenario AddScenario(string name);

    /// <summary>
    /// Exécute tous les scénarios de la fonctionnalité de manière asynchrone.
    /// </summary>
    public Task ExecuteAsync();

    /// <summary>
    /// Convertit les scénarios en TestRunResult.
    /// </summary>
    public Task<TestRunResult> ExecuteAndAsTestRunResultAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convertit les scénarios en objets TestCase pour l'orchestration.
    /// </summary>
    public IReadOnlyCollection<TestCase> ToTestCases();
}
```

---

### Scenario

Scénario de test individuel avec étapes Given-When-Then.

```csharp
public class Scenario
{
    /// <summary>
    /// Obtient le nom du scénario.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Obtient les étapes dans l'ordre d'exécution.
    /// </summary>
    public IReadOnlyList<Step> Steps { get; }

    /// <summary>
    /// Initialise un nouveau Scenario.
    /// </summary>
    public Scenario(string name);

    /// <summary>
    /// Ajoute une étape Given (configuration du contexte).
    /// </summary>
    public Scenario Given(string stepText);

    /// <summary>
    /// Ajoute une étape When (action).
    /// </summary>
    public Scenario When(string stepText);

    /// <summary>
    /// Ajoute une étape Then (assertion).
    /// </summary>
    public Scenario Then(string stepText);

    /// <summary>
    /// Ajoute une étape And (continuation).
    /// </summary>
    public Scenario And(string stepText);

    /// <summary>
    /// Ajoute une étape But (négation).
    /// </summary>
    public Scenario But(string stepText);

    /// <summary>
    /// Exécute toutes les étapes de manière asynchrone.
    /// </summary>
    public Task ExecuteAsync();

    /// <summary>
    /// Exécute et retourne TestResult pour l'intégration.
    /// </summary>
    public Task<TestResult> ExecuteAndAsTestResultAsync(string featureName);
}
```

---

### Step

Étape individuelle avec logique d'exécution et validation.

```csharp
public class Step
{
    /// <summary>
    /// Obtient le type d'étape (Given, When, Then, And, But).
    /// </summary>
    public StepType Type { get; }

    /// <summary>
    /// Obtient le texte de l'étape.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Obtient ou définit l'action d'exécution.
    /// </summary>
    public Func<Task>? Execute { get; set; }

    /// <summary>
    /// Obtient ou définit la fonction de validation pour les assertions.
    /// </summary>
    public Func<bool>? ExecuteValidation { get; set; }

    /// <summary>
    /// Exécute l'action d'étape de manière asynchrone.
    /// </summary>
    public Task ExecuteAsync();
}
```

---

### ScenarioExecutionResult

Résultat de l'exécution d'un scénario incluant tous les résultats des étapes.

```csharp
public class ScenarioExecutionResult
{
    /// <summary>
    /// Obtient ou définit le nom du scénario.
    /// </summary>
    public string ScenarioName { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit le statut global.
    /// </summary>
    public ScenarioStatus Status { get; set; }

    /// <summary>
    /// Obtient ou définit les résultats d'exécution individuels des étapes.
    /// </summary>
    public List<StepExecutionResult> Steps { get; set; } = new();

    /// <summary>
    /// Obtient ou définit la durée d'exécution.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Obtient ou définit le message d'erreur si le scénario a échoué.
    /// </summary>
    public string? Error { get; set; }
}
```

---

### BddStepRegistry

Registre central pour lier les motifs de texte d'étape aux actions exécutables.

```csharp
public sealed class BddStepRegistry
{
    /// <summary>
    /// Enregistre un motif d'étape avec une action exécutable.
    /// </summary>
    public void RegisterStep(string pattern, Func<Task> action);

    /// <summary>
    /// Trouve une action correspondante pour un texte d'étape donné.
    /// </summary>
    public Func<Task>? FindMatch(string stepText);
}
```

---

## Attributs de Définition d'Étapes

Les définitions d'étapes sont déclarées à l'aide d'attributs sur les sous-classes BddStepDefinition :

```csharp
namespace PeasyPilot.BDD.StepDefinitions;

/// <summary>
/// Marque une méthode comme étape Given (configuration du contexte).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class GivenAttribute : Attribute
{
    /// <summary>
    /// Obtient le motif d'étape (supporte les espaces réservés {param}).
    /// </summary>
    public string Pattern { get; }

    public GivenAttribute(string pattern);
}

/// <summary>
/// Marque une méthode comme étape When (action).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class WhenAttribute : Attribute
{
    /// <summary>
    /// Obtient le motif d'étape (supporte les espaces réservés {param}).
    /// </summary>
    public string Pattern { get; }

    public WhenAttribute(string pattern);
}

/// <summary>
/// Marque une méthode comme étape Then (assertion).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ThenAttribute : Attribute
{
    /// <summary>
    /// Obtient le motif d'étape (supporte les espaces réservés {param}).
    /// </summary>
    public string Pattern { get; }

    public ThenAttribute(string pattern);
}
```

---

## Configuration et Motifs

### Format de Fichier de Fonctionnalité (Gherkin)

Les fichiers de fonctionnalités utilisent la syntaxe Gherkin standard :

```gherkin
Feature: Gestion des Comptes Utilisateur

Scenario: L'utilisateur peut créer un nouveau compte
    Given un formulaire de nouvel utilisateur est affiché
    When l'utilisateur entre le nom "Jean Dupont"
    And l'utilisateur entre l'email "jean@example.com"
    And l'utilisateur soumet le formulaire
    Then le compte est créé avec succès
    And l'utilisateur reçoit un email de confirmation
```

### Motifs de Définition d'Étapes

Les motifs d'étapes supportent l'extraction de paramètres via des accolades :

```csharp
[Given("a user with name {name}")]
public void CreateUser(string name) { }

[When("the user enters {count:int} items")]
public void AddItems(int count) { }

[Then("the total is {amount:decimal}")]
public void ValidateTotal(decimal amount) { }
```

Types de paramètres supportés :
- `{name}` - string (défaut)
- `{count:int}` - entier
- `{amount:decimal}` - décimal
- `{flag:bool}` - booléen

---

## Exemples Concrets

### Exemple 1 : Définition et Exécution Basique de Fonctionnalité

```csharp
using PeasyPilot.BDD;

public class BasicFeatureTest
{
    [Fact]
    public async Task TestBasicFeature()
    {
        var feature = new Feature("User Login");
        
        feature.AddScenario("Successful login")
            .Given("user has valid credentials")
            .When("user submits login form")
            .Then("user is redirected to dashboard");

        feature.AddScenario("Invalid password")
            .Given("user has invalid password")
            .When("user submits login form")
            .Then("error message is displayed");

        // Execute all scenarios
        await feature.ExecuteAsync();

        // Convert to test cases for orchestration
        var testCases = feature.ToTestCases();
        Assert.NotEmpty(testCases);
    }
}
```

### Exemple 2 : Analyse de Texte Gherkin

```csharp
using PeasyPilot.BDD;

public class GherkinParsingTest
{
    [Fact]
    public void TestParseGherkinFeature()
    {
        var gherkinText = @"
Feature: Shopping Cart
    Scenario: Add item to cart
        Given the user is on the product page
        When the user clicks add to cart
        Then the item appears in the cart
        And the cart count increases
";

        var feature = GherkinFeatureParser.Parse(gherkinText);

        Assert.Equal("Shopping Cart", feature.Name);
        Assert.Single(feature.Scenarios);
        
        var scenario = feature.Scenarios.First();
        Assert.Equal("Add item to cart", scenario.Name);
        Assert.Equal(4, scenario.Steps.Count);
    }
}
```

### Exemple 3 : Chargement de Fichiers de Fonctionnalités

```csharp
using PeasyPilot.BDD.FileLoading;

public class FeatureFileLoaderTest
{
    private readonly IFeatureFileLoader _loader = new GherkinFeatureFileLoader();

    [Fact]
    public async Task TestLoadFeaturesFromDirectory()
    {
        var featuresPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Features");

        var features = await _loader.LoadFromDirectoryAsync(featuresPath);

        Assert.NotEmpty(features);
        
        foreach (var feature in features)
        {
            Assert.NotNull(feature.Name);
            Assert.NotEmpty(feature.Scenarios);
        }
    }

    [Fact]
    public async Task TestLoadSingleFeatureFile()
    {
        var featurePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Features",
            "UserManagement.feature");

        var feature = await _loader.LoadFromFileAsync(featurePath);

        Assert.NotNull(feature);
        Assert.NotEmpty(feature.Scenarios);
    }
}
```

### Exemple 4 : Définitions d'Étapes avec Attributs

```csharp
using PeasyPilot.BDD.StepDefinitions;

public class UserStepDefinitions : BddStepDefinition
{
    private string? _currentUserName;
    private bool _creationSucceeded;

    [Given("a user database is empty")]
    public Task DatabaseIsEmpty()
    {
        // Setup code
        return Task.CompletedTask;
    }

    [Given("a user with name {name}")]
    public Task CreateUser(string name)
    {
        _currentUserName = name;
        return Task.CompletedTask;
    }

    [When("the system validates the name")]
    public Task ValidateName()
    {
        _creationSucceeded = !string.IsNullOrEmpty(_currentUserName) 
            && _currentUserName.Length > 2;
        return Task.CompletedTask;
    }

    [Then("the user is accepted")]
    public void UserIsAccepted()
    {
        Assert.True(_creationSucceeded);
    }
}
```

### Exemple 5 : Exécution de Scénarios avec Liaison d'Étapes

```csharp
using PeasyPilot.BDD.Execution;
using PeasyPilot.BDD.StepDefinitions;
using Microsoft.Extensions.DependencyInjection;

public class ScenarioExecutionTest
{
    [Fact]
    public async Task TestExecuteScenarioWithBindings()
    {
        // Setup DI container
        var services = new ServiceCollection();
        services.AddScoped<UserStepDefinitions>();
        var serviceProvider = services.BuildServiceProvider();

        // Create scenario
        var scenario = new Scenario("Create valid user")
            .Given("a user database is empty")
            .Given("a user with name John Doe")
            .When("the system validates the name")
            .Then("the user is accepted");

        // Setup step binding resolver
        var resolver = new StepBindingResolver();
        var executor = new ScenarioExecutor(resolver);

        // Execute with step binding resolution
        var result = await executor.ExecuteAsync(
            scenario,
            serviceProvider);

        Assert.Equal(ScenarioStatus.Passed, result.Status);
        Assert.All(result.Steps, step => Assert.True(step.Passed));
    }
}
```

### Exemple 6 : Exportation de Documentation Vivante

```csharp
using PeasyPilot.BDD;

public class LivingDocumentationTest
{
    [Fact]
    public void TestExportAsLivingDoc()
    {
        var feature = new Feature("Order Management");
        
        feature.AddScenario("Create new order")
            .Given("customer has items in cart")
            .When("customer proceeds to checkout")
            .Then("order is created with unique ID")
            .And("confirmation email is sent");

        feature.AddScenario("Cancel order")
            .Given("order exists with status pending")
            .When("customer cancels order")
            .Then("order status changes to cancelled");

        var exporter = new LivingDocExporter();
        var markdown = exporter.Export(feature);

        Assert.Contains("Order Management", markdown);
        Assert.Contains("Create new order", markdown);
        Assert.Contains("Given", markdown);
        Assert.Contains("When", markdown);
        Assert.Contains("Then", markdown);
    }
}
```

### Exemple 7 : Intégration avec PeasyPilot.Core

```csharp
using PeasyPilot.BDD;
using PeasyPilot.Core.Models;

public class BddCoreIntegrationTest
{
    [Fact]
    public async Task TestConvertScenarioToTestCase()
    {
        var feature = new Feature("API Endpoint Testing");
        
        feature.AddScenario("GET request returns 200")
            .Given("API server is running")
            .When("GET request is sent")
            .Then("response status is 200");

        // Convert to unified test case model
        var testCases = feature.ToTestCases();

        Assert.Single(testCases);
        var testCase = testCases.First();
        Assert.Equal("GET request returns 200", testCase.Name);
    }

    [Fact]
    public async Task TestConvertFeatureToTestRunResult()
    {
        var feature = new Feature("Payment Processing");
        
        feature.AddScenario("Process valid payment")
            .Given("customer has valid payment method")
            .When("customer submits payment")
            .Then("payment is processed");

        feature.AddScenario("Decline invalid payment")
            .Given("customer has invalid payment method")
            .When("customer submits payment")
            .Then("payment is declined");

        // Execute and get unified result
        var result = await feature.ExecuteAndAsTestRunResultAsync();

        Assert.NotNull(result);
        Assert.True(result.Passed > 0 || result.Failed > 0);
    }
}
```

### Exemple 8 : Correspondance de Motifs d'Étapes Personnalisés

```csharp
using PeasyPilot.BDD;

public class CustomPatternMatchingTest
{
    [Fact]
    public void TestBddStepRegistry()
    {
        var registry = new BddStepRegistry();

        // Register steps with regex patterns
        registry.RegisterStep(
            @"^user enters name (.+)$",
            async () => { await Task.CompletedTask; });

        registry.RegisterStep(
            @"^(\d+) items are added$",
            async () => { await Task.CompletedTask; });

        // Match steps
        var match1 = registry.FindMatch("user enters name John Doe");
        Assert.NotNull(match1);

        var match2 = registry.FindMatch("5 items are added");
        Assert.NotNull(match2);

        var noMatch = registry.FindMatch("unknown step");
        Assert.Null(noMatch);
    }
}
```

### Exemple 9 : Exécution de Scénarios Multiples dans une Fonctionnalité

```csharp
using PeasyPilot.BDD;

public class MultiScenarioTest
{
    [Fact]
    public async Task TestExecuteMultipleScenariosInFeature()
    {
        var feature = new Feature("User Registration");

        // Scenario 1: Happy path
        feature.AddScenario("Register with valid data")
            .Given("registration page is open")
            .When("user enters valid email and password")
            .Then("account is created successfully");

        // Scenario 2: Invalid email
        feature.AddScenario("Register with invalid email")
            .Given("registration page is open")
            .When("user enters invalid email")
            .Then("validation error is shown");

        // Scenario 3: Password mismatch
        feature.AddScenario("Register with mismatched passwords")
            .Given("registration page is open")
            .When("user enters mismatched passwords")
            .Then("validation error is shown");

        // Execute all scenarios
        await feature.ExecuteAsync();

        Assert.Equal(3, feature.Scenarios.Count);
    }
}
```

### Exemple 10 : Scénario avec Configuration Complexe

```csharp
using PeasyPilot.BDD;
using PeasyPilot.Integration.Abstractions;

public class ComplexScenarioTest
{
    [Fact]
    public async Task TestScenarioWithComplexSetup()
    {
        var feature = new Feature("Inventory Management");

        var scenario = feature.AddScenario("Update inventory after sale")
            .Given("product exists with quantity 100")
            .And("product has minimum stock 10")
            .And("recent sale of 25 units recorded")
            .When("inventory is updated from sale")
            .And("minimum stock check is performed")
            .Then("new quantity is 75")
            .And("stock is above minimum")
            .And("stock warning is not triggered");

        Assert.Equal(8, scenario.Steps.Count);

        var thenSteps = scenario.Steps
            .Where(s => s.Type == StepType.Then)
            .ToList();

        Assert.Equal(3, thenSteps.Count);
    }
}
```

---

## Motifs Courants

### Scénario Outline (Tests Pilotés par les Données)

Bien que PeasyPilot.BDD ne fournisse pas de support intégré pour les scénarios outline comme Cucumber, vous pouvez obtenir des résultats similaires en utilisant les théories xUnit :

```csharp
public class ScenarioOutlineExample
{
    private readonly GherkinFeatureParser _parser = new();

    [Theory]
    [InlineData("John", true)]
    [InlineData("Jane", true)]
    [InlineData("", false)]
    public async Task TestUserCreationWithVariousNames(string name, bool shouldSucceed)
    {
        var scenario = new Scenario($"Create user {name}")
            .Given($"user with name {name}");
        // ... continue building scenario
        // This pattern allows data-driven BDD execution
    }
}
```

### Antécédent (Configuration Partagée)

Créez des méthodes d'aide pour les étapes de configuration courantes :

```csharp
public class SharedSetupExample
{
    private Scenario CreateScenarioWithCommonBackground(string scenarioName)
    {
        return new Scenario(scenarioName)
            .Given("database is initialized")
            .Given("test user exists")
            .Given("authentication is enabled");
    }

    [Fact]
    public async Task TestScenario1()
    {
        var scenario = CreateScenarioWithCommonBackground("Test scenario 1");
        // ... add specific steps
    }
}
```

---

## Considérations de Performances

- **Chargement de Fichiers de Fonctionnalités :** L'analyse récursive des répertoires s'adapte à des milliers de fonctionnalités
- **Analyse :** L'analyseur natif gère efficacement les grands fichiers Gherkin
- **Liaison d'Étapes :** La découverte basée sur la réflexion s'exécute une fois lors de la configuration, l'exécution est rapide
- **Async/Await :** Toutes les opérations supportent les motifs asynchrones pour une exécution non-bloquante

---

## Voir Aussi

- [Guide de Test BDD](../GUIDES/bdd-testing-guide-FR.md)
- [API PeasyPilot.Core](api-core-FR.md)
- [API PeasyPilot.Integration](api-integration-FR.md)
- [Guide de Résolution de Liaison d'Étapes](../GUIDES/bdd-testing-guide-FR.md#step-binding)

