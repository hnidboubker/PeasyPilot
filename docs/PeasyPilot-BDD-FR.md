# PeasyPilot BDD — Guide Complet

Guide complet du Behavior-Driven Development (BDD) avec PeasyPilot : support Gherkin et binding automatique des étapes.

---

## Table des matières

1. [Vue d'ensemble](#vue-densemble)
2. [Démarrage rapide](#démarrage-rapide)
3. [Fichiers de fonctionnalité](#fichiers-de-fonctionnalité)
4. [Définitions d'étapes](#définitions-détapes)
5. [Résolveur de binding d'étapes](#résolveur-de-binding-détapes)
6. [Tests d'intégration](#tests-dintégration)
7. [Workflow complet](#workflow-complet)
8. [Bonnes pratiques](#bonnes-pratiques)
9. [Dépannage](#dépannage)

---

## Vue d'ensemble

PeasyPilot offre **un support Gherkin complet** avec :

- ✅ Chargement de fichiers de fonctionnalité (`.feature`)
- ✅ Découverte automatique des étapes via réflexion
- ✅ Correspondance de motif pour le texte des étapes
- ✅ Extraction de paramètres (string, int, decimal, etc.)
- ✅ Intégration avec les fixtures de test
- ✅ Gestion du cycle de vie de la base de données

### Composants principaux

```
Fichier de fonctionnalité (.feature)
        ↓
GherkinFeatureFileLoader
        ↓
Feature + Scenarios + Steps
        ↓
StepBindingResolver (réflexion + correspondance de motif)
        ↓
ScenarioExecutor (exécution étape par étape)
        ↓
ScenarioExecutionResult (Réussi/Échoué/Ignoré)
```

---

## Démarrage rapide

### 1. Créer un fichier de fonctionnalité

**Fichier :** `features/calculator.feature`

```gherkin
Feature: Calculatrice
  Scenario: Ajouter deux nombres
    Given j'ai entré 50 dans la calculatrice
    And j'ai entré 70 dans la calculatrice
    When j'appuie sur ajouter
    Then le résultat doit être 120 à l'écran
```

### 2. Définir les bindings d'étapes

**Fichier :** `StepDefinitions/CalculatorSteps.cs`

```csharp
using PeasyPilot.BDD.StepDefinitions;

public class CalculatorSteps : BddStepDefinition
{
    private int _input1;
    private int _input2;
    private int _result;

    [Given("j'ai entré {number} dans la calculatrice")]
    public async Task EnterNumber(string number)
    {
        if (int.TryParse(number, out var num))
        {
            if (_input1 == 0)
                _input1 = num;
            else
                _input2 = num;
        }
        await Task.CompletedTask;
    }

    [When("j'appuie sur ajouter")]
    public async Task PressAdd()
    {
        _result = _input1 + _input2;
        await Task.CompletedTask;
    }

    [Then("le résultat doit être {expected} à l'écran")]
    public async Task VerifyResult(string expected)
    {
        if (int.TryParse(expected, out var exp))
        {
            Assert.Equal(exp, _result);
        }
        await Task.CompletedTask;
    }
}
```

### 3. Écrire le test

**Fichier :** `Tests/CalculatorBddTests.cs`

```csharp
using PeasyPilot.BDD.FileLoading;
using PeasyPilot.BDD.Execution;
using PeasyPilot.BDD.StepDefinitions;
using Xunit;

public class CalculatorBddTests
{
    [Fact]
    public async Task CalculatorScenarios_ExecuteSuccessfully()
    {
        // Charger le fichier de fonctionnalité
        var loader = new GherkinFeatureFileLoader();
        var feature = await loader.LoadFromFileAsync("features/calculator.feature");

        // Configurer le résolveur de binding d'étapes
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(CalculatorSteps));

        // Exécuter les scénarios
        var executor = new ScenarioExecutor(resolver);
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        foreach (var scenario in feature.Scenarios)
        {
            var result = await executor.ExecuteAsync(scenario, serviceProvider);
            Assert.True(result.Status == ScenarioStatus.Passed, 
                $"Scénario échoué: {scenario.Name}");
        }
    }
}
```

### 4. Exécuter les tests

```bash
dotnet test
```

---

## Comment ça marche

### Pipeline BDD

```
1. Charger le fichier .feature
   ↓
2. Analyser la syntaxe Gherkin
   ↓
3. Découvrir les définitions d'étapes via réflexion
   ↓
4. Créer une matrice de bindings (motif → méthode)
   ↓
5. Pour chaque scénario :
   - Pour chaque étape :
     a. Faire correspondre le texte de l'étape au motif
     b. Extraire les paramètres
     c. Appeler la méthode avec les paramètres
   ↓
6. Retourner le résultat (réussi/échoué/ignoré)
```

### Extraction de paramètres

TestAssistant identifie automatiquement les paramètres :

```csharp
// Pattern
[Given("j'ai entré {number} dans la calculatrice")]

// Texte de l'étape
Given j'ai entré 50 dans la calculatrice

// Extraction
number = "50" → converti en int 50
```

### Types de paramètres supportés

| Type | Exemple | Conversion |
| --- |---------|-----------|
| `string` | `{text}` | Pas de conversion |
| `int` | `{count}` | `int.Parse()` |
| `decimal` | `{amount}` | `decimal.Parse()` |
| `double` | `{value}` | `double.Parse()` |
| `bool` | `{flag}` | `bool.Parse()` |
| `DateTime` | `{date}` | `DateTime.Parse()` |

---

## Fichiers de fonctionnalité

### Structure Gherkin

```gherkin
Feature: Nom de la fonctionnalité
  Description optionnelle de ce que fait cette fonctionnalité

  Background:
    Given une condition préalable commune à tous les scénarios

  Scenario: Nom du scénario
    Given une condition initiale
    And une autre condition
    When une action
    Then un résultat attendu
    And un autre résultat

  Scenario: Un autre scénario
    Given différente condition initiale
    When l'action
    Then le résultat
```

### Exemple complet

```gherkin
Feature: Gestion des utilisateurs
  Permettre aux administrateurs de gérer les utilisateurs du système

  Background:
    Given une base de données vide

  Scenario: Créer un nouvel utilisateur
    Given aucun utilisateur n'existe
    When je crée un utilisateur avec email "alice@example.com"
    Then l'utilisateur doit être dans la base de données
    And le nombre d'utilisateurs doit être 1

  Scenario: Dupliquer un email échoue
    Given un utilisateur "bob@example.com" existe
    When je tente de créer un utilisateur avec email "bob@example.com"
    Then une erreur doit être levée
    And le nombre d'utilisateurs doit rester 1
```

---

## Définitions d'étapes

### Attributs disponibles

```csharp
public class MySteps : BddStepDefinition
{
    [Given("...")]   // Condition initiale
    public async Task GivenStep() { }

    [When("...")]    // Action
    public async Task WhenStep() { }

    [Then("...")]    // Assertion
    public async Task ThenStep() { }

    [And("...")]     // Continuation de Given/When/Then
    public async Task AndStep() { }

    [But("...")]     // Négation/Alternative
    public async Task ButStep() { }
}
```

### Bonnes pratiques pour les étapes

```csharp
// ✅ BON : Étape claire et réutilisable
[Given("un utilisateur avec email {email} existe")]
public async Task UserExists(string email)
{
    var user = new User { Email = email };
    await _repository.AddAsync(user);
}

// ❌ MAUVAIS : Étape trop générale
[Given("des données")]
public async Task SetupData() { }
```

### État entre étapes

Utilisez des variables d'instance pour partager l'état :

```csharp
public class UserSteps : BddStepDefinition
{
    private User _currentUser = null!;
    private string _errorMessage = "";

    [Given("un nouvel utilisateur")]
    public async Task NewUser()
    {
        _currentUser = new User();
        await Task.CompletedTask;
    }

    [When("je définis son email à {email}")]
    public async Task SetEmail(string email)
    {
        _currentUser.Email = email;
        await Task.CompletedTask;
    }

    [Then("l'email doit être {expected}")]
    public async Task VerifyEmail(string expected)
    {
        Assert.Equal(expected, _currentUser.Email);
        await Task.CompletedTask;
    }
}
```

---

## Résolveur de binding d'étapes

### Enregistrer les définitions

```csharp
var resolver = new StepBindingResolver();

// Enregistrer une classe de définitions
resolver.RegisterStepDefinition(typeof(UserSteps));
resolver.RegisterStepDefinition(typeof(OrderSteps));

// Ou enregistrer via le conteneur DI
resolver.RegisterStepDefinition(typeof(UserSteps), serviceProvider);
```

### Fonctionnement interne

Le résolveur :
1. Scanne la classe via réflexion
2. Trouve tous les attributs (`[Given]`, `[When]`, `[Then]`)
3. Extrait les motifs (patterns)
4. Crée une matrice regex pour la correspondance rapide
5. À l'exécution, fait correspondre le texte au motif et extrait les paramètres

---

## Tests d'intégration

### Avec fixtures de base de données

```csharp
public class UserSteps : BddStepDefinition
{
    private readonly IUserRepository _repository;

    public UserSteps(IUserRepository repository)
    {
        _repository = repository;
    }

    [Given("un utilisateur {email} existe")]
    public async Task UserExists(string email)
    {
        var user = new User { Email = email };
        await _repository.AddAsync(user);
    }

    [Then("{email} doit être dans la base de données")]
    public async Task UserInDatabase(string email)
    {
        var user = await _repository.FindByEmailAsync(email);
        Assert.NotNull(user);
    }
}
```

### Utiliser avec IntegrationTestFixture

```csharp
public class UserBddTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        var repository = new InMemoryUserRepository();
        services.AddSingleton<IUserRepository>(repository);
        RegisterResettableService(repository);
    }

    [Fact]
    public async Task UserScenarios()
    {
        var loader = new GherkinFeatureFileLoader();
        var feature = await loader.LoadFromFileAsync("features/users.feature");

        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(UserSteps), Services);

        var executor = new ScenarioExecutor(resolver);

        foreach (var scenario in feature.Scenarios)
        {
            var result = await executor.ExecuteAsync(scenario, Services);
            Assert.True(result.Status == ScenarioStatus.Passed);
        }
    }
}
```

---

## Workflow complet

### De la fonctionnalité au test

```
1. Écrire le fichier .feature
   ↓
2. Implémenter les définitions d'étapes (StepDefinitions)
   ↓
3. Configurer le test (charger fichier, enregistrer étapes)
   ↓
4. Exécuter via xUnit/NUnit/TUnit
   ↓
5. Itérer sur les résultats
```

### Exemple complet

**Feature file:**
```gherkin
Feature: Validation d'email
  Scenario: Email valide
    Given une adresse email "user@example.com"
    When je valide l'email
    Then le résultat doit être valide
```

**Étapes:**
```csharp
public class EmailSteps : BddStepDefinition
{
    private string _email = "";
    private bool _isValid;

    [Given("une adresse email {email}")]
    public async Task GivenEmail(string email)
    {
        _email = email;
        await Task.CompletedTask;
    }

    [When("je valide l'email")]
    public async Task ValidateEmail()
    {
        _isValid = !string.IsNullOrEmpty(_email) && _email.Contains("@");
        await Task.CompletedTask;
    }

    [Then("le résultat doit être valide")]
    public async Task VerifyValid()
    {
        Assert.True(_isValid);
        await Task.CompletedTask;
    }
}
```

**Test:**
```csharp
[Fact]
public async Task EmailValidationScenarios()
{
    var loader = new GherkinFeatureFileLoader();
    var feature = await loader.LoadFromFileAsync("features/email.feature");

    var resolver = new StepBindingResolver();
    resolver.RegisterStepDefinition(typeof(EmailSteps));

    var executor = new ScenarioExecutor(resolver);
    var sp = new ServiceCollection().BuildServiceProvider();

    foreach (var scenario in feature.Scenarios)
    {
        var result = await executor.ExecuteAsync(scenario, sp);
        Assert.True(result.Status == ScenarioStatus.Passed);
    }
}
```

---

## Bonnes pratiques

### ✅ À faire

- Écrire des scénarios en langage métier
- Utiliser des étapes réutilisables
- Partager l'état via variables d'instance
- Tester avec des fixtures d'intégration
- Utiliser Background pour les conditions communes

### ❌ À éviter

- Étapes trop génériques ou trop spécifiques
- Dépendances externes dans les étapes
- Scénarios sans assertions
- Oublier d'attendre les tâches async

---

## Dépannage

### Problème : Étape non trouvée

**Cause :** Définition manquante ou motif mal formé.

**Solution :**
```csharp
// Vérifier que l'étape est bien enregistrée
resolver.RegisterStepDefinition(typeof(YourSteps));

// Vérifier que le motif correspond
[Given("exact text matching")]
public async Task YourStep() { }
```

### Problème : Extraction de paramètre échouée

**Cause :** Type non supporté ou conversion échouée.

**Solution :**
```csharp
// Utiliser int.Parse manuellement
[Given("j'ai {count} éléments")]
public async Task HaveItems(string countText)
{
    int count = int.Parse(countText);  // Conversion explicite
    // ...
}
```

---

## Résumé

```
1️⃣  Créer .feature file
2️⃣  Implémenter les étapes
3️⃣  Configurer le test
4️⃣  Exécuter et valider
5️⃣  Itérer
```

BDD avec PeasyPilot = tests lisibles en langage métier + implémentation robuste. 🎯
