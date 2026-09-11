# Référence API PeasyPilot.Core

## Aperçu

`PeasyPilot.Core` est le package fondateur fournissant des abstractions indépendantes du framework pour la découverte, l'exécution et la génération de rapports de tests. Il vous permet de construire des moteurs de test unifiés qui fonctionnent de manière transparente sur xUnit, NUnit, TUnit et les frameworks de test personnalisés.

**Responsabilités principales :**
- Interface de découverte de tests indépendante du framework
- Modèle d'exécution de tests unifié
- Génération de rapports de résultats standardisée
- Gestion du contexte de test (stockage thread-safe)
- Intégration d'injection de dépendances
- Filtrage des tests et manipulation des métadonnées

**Cibles :** .NET 8.0, 9.0, 10.0

---

## Abstractions Principales

### ITestDiscovery

Découvre les tests disponibles de manière indépendante du framework.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestDiscovery
{
    /// <summary>
    /// Découvre les tests disponibles.
    /// </summary>
    /// <param name="cancellationToken">Le jeton d'annulation.</param>
    /// <returns>Les tests découverts.</returns>
    Task<IReadOnlyCollection<TestCase>> DiscoverAsync(
        CancellationToken cancellationToken = default);
}
```

**Objectif :** Fournit une découverte de tests indépendante du framework. Des implémentations existent pour xUnit, NUnit, TUnit.

**Exemple : Utiliser ITestDiscovery**
```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

public class ServiceDécouverte
{
    private readonly ITestDiscovery _discovery;

    public ServiceDécouverte(ITestDiscovery discovery)
    {
        _discovery = discovery;
    }

    public async Task<int> ComptabiliserTestsAsync()
    {
        var tests = await _discovery.DiscoverAsync();
        return tests.Count;
    }

    public async Task<IEnumerable<string>> ObtenirNomsDesTestsAsync()
    {
        var tests = await _discovery.DiscoverAsync();
        return tests.Select(t => t.Name);
    }
}
```

### ITestEngine

Exécute les séries de tests et retourne les résultats unifiés.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestEngine
{
    /// <summary>
    /// Exécute la demande de série de tests fournie.
    /// </summary>
    /// <param name="request">La demande à exécuter.</param>
    /// <param name="cancellationToken">Le jeton d'annulation.</param>
    /// <returns>Le résultat d'exécution unifié.</returns>
    Task<TestRunResult> RunAsync(
        TestRunRequest request,
        CancellationToken cancellationToken = default);
}
```

**Objectif :** Exécute les tests et fournit des résultats unifiés, quel que soit le framework de test sous-jacent.

**Exemple : Exécuter des Tests avec ITestEngine**
```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

public class ExécuteurDeTests
{
    private readonly ITestEngine _engine;

    public ExécuteurDeTests(ITestEngine engine)
    {
        _engine = engine;
    }

    public async Task<TestRunResult> ExécuterTousLesTestsAsync()
    {
        var request = new TestRunRequest
        {
            Name = "Suite de Tests Complète",
            Metadata = new() { ["environment"] = "ci" }
        };

        // Ajouter les cas de test à la demande...
        return await _engine.RunAsync(request);
    }

    public async Task<bool> ADesÉchecsAsync(TestRunResult result)
    {
        return result.Failed > 0;
    }
}
```

### ITestReporter

Génère des rapports de tests de manière indépendante du framework.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestReporter
{
    /// <summary>
    /// Écrit le résultat d'exécution vers une destination.
    /// </summary>
    /// <param name="result">Le résultat de la série de tests.</param>
    /// <param name="cancellationToken">Le jeton d'annulation.</param>
    /// <returns>La sortie sérialisée.</returns>
    Task<string> ReportAsync(
        TestRunResult result,
        CancellationToken cancellationToken = default);
}
```

**Objectif :** Fournit plusieurs formats de sortie (console, JSON, XML, HTML) pour les résultats de tests.

**Exemple : Générer des Rapports de Tests**
```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;
using System.IO;

public class GénérateurDeRapports
{
    private readonly ITestReporter _reporter;

    public GénérateurDeRapports(ITestReporter reporter)
    {
        _reporter = reporter;
    }

    public async Task<string> GénérerRapportAsync(TestRunResult result)
    {
        return await _reporter.ReportAsync(result);
    }

    public async Task EnregistrerRapportAsync(TestRunResult result, string filePath)
    {
        var report = await _reporter.ReportAsync(result);
        await File.WriteAllTextAsync(filePath, report);
    }
}
```

### ITestContext

Contexte thread-safe pour stocker et récupérer les données de test lors de l'exécution.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestContext
{
    /// <summary>
    /// Récupère ou ajoute une valeur au contexte de test.
    /// </summary>
    /// <typeparam name="T">Le type de la valeur.</typeparam>
    /// <param name="key">La clé de cache.</param>
    /// <param name="factory">La fonction factory pour créer la valeur.</param>
    /// <returns>La valeur du cache ou la valeur nouvellement créée.</returns>
    T GetOrAdd<T>(string key, Func<T> factory);
}
```

**Objectif :** Fournit un stockage thread-safe pour les données de test, utile pour partager l'état entre les tests ou les phases de test.

**Exemple : Utiliser TestContext**
```csharp
using PeasyPilot.Core.Context;
using PeasyPilot.Core.Abstractions;

public class TestAvecContexte
{
    private readonly ITestContext _context;

    public TestAvecContexte()
    {
        _context = new TestContext();
    }

    [Fact]
    public void TestAvecDonnéesPartagées()
    {
        // Récupérer ou créer une connexion à la base de données
        var connection = _context.GetOrAdd("db_connection", () =>
        {
            return new DatabaseConnection("Server=.;Database=test");
        });

        // La connexion est réutilisée si elle est accédée à nouveau
        var sameConnection = _context.GetOrAdd("db_connection", () =>
        {
            throw new InvalidOperationException("Doit réutiliser l'existant");
        });

        Assert.Same(connection, sameConnection);
    }
}
```

---

## Classes Principales

### TestContext

Dictionnaire thread-safe pour stocker les données de test.

```csharp
namespace PeasyPilot.Core.Context;

public class TestContext : ITestContext
{
    /// <summary>
    /// Récupère ou ajoute une valeur au contexte de test.
    /// </summary>
    public T GetOrAdd<T>(string key, Func<T> factory)
    {
        return (T)_data.GetOrAdd(key, _ => factory()!);
    }
}
```

**Détails d'implémentation :**
- Utilise `ConcurrentDictionary<string, object>` en interne
- Thread-safe pour les scénarios de test multi-threadés
- Type-safe avec la méthode générique GetOrAdd
- Initialisation paresseuse via le pattern factory

**Exemple : Créer et Utiliser TestContext**
```csharp
using PeasyPilot.Core.Context;

public class ExempleContexte
{
    public void DémontrerLaMise()
    {
        var context = new TestContext();

        // Le premier appel exécute la factory
        int count1 = context.GetOrAdd("counter", () =>
        {
            Console.WriteLine("Création du compteur");
            return 42;
        });

        // Le deuxième appel retourne la valeur en cache
        int count2 = context.GetOrAdd("counter", () =>
        {
            throw new InvalidOperationException("La factory ne doit pas s'exécuter");
        });

        Assert.Equal(42, count1);
        Assert.Equal(42, count2);
    }
}
```

---

## Modèles de Données

### TestCase

Représente un cas de test unique dans une demande de série de tests unifié.

```csharp
namespace PeasyPilot.Core.Models;

public class TestCase
{
    /// <summary>Obtient ou définit le nom du cas de test.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Obtient ou définit le nom de la catégorie ou de la suite.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Obtient ou définit le type de classification du test.</summary>
    public TestKind Kind { get; set; } = TestKind.Unit;

    /// <summary>Obtient ou définit les métadonnées optionnelles attachées au cas de test.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum TestKind
{
    Unit = 0,
    Integration = 1,
    E2E = 2,
    Performance = 3,
    Security = 4
}
```

**Exemple : Créer les Instances de TestCase**
```csharp
using PeasyPilot.Core.Models;

var testUnitaire = new TestCase
{
    Name = "Calculator_Add_ReturnsCorrectSum",
    Category = "MathTests",
    Kind = TestKind.Unit,
    Metadata = new()
    {
        ["priority"] = "high",
        ["author"] = "john.doe"
    }
};

var testIntégration = new TestCase
{
    Name = "Database_Connection_ValidatesSchema",
    Category = "DatabaseTests",
    Kind = TestKind.Integration
};
```

### TestRunRequest

Représente une demande pour exécuter un ou plusieurs cas de test.

```csharp
namespace PeasyPilot.Core.Models;

public class TestRunRequest
{
    /// <summary>Obtient ou définit le nom de la séries de tests.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Obtient ou définit les métadonnées utilisées par le moteur.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>Obtient ou définit la liste des cas de test à exécuter.</summary>
    public List<TestCase> TestCases { get; set; } = new();
}
```

**Exemple : Construire les Demandes de Tests**
```csharp
using PeasyPilot.Core.Models;

// Créer une demande de série de tests
var request = new TestRunRequest
{
    Name = "Tests de la Build Nocturne",
    Metadata = new()
    {
        ["build_id"] = "build_12345",
        ["branch"] = "main",
        ["environment"] = "staging"
    },
    TestCases = new()
    {
        new TestCase
        {
            Name = "UserService_CreateUser_Success",
            Category = "UserTests",
            Kind = TestKind.Unit
        },
        new TestCase
        {
            Name = "PaymentService_ProcessPayment_ValidatesAmount",
            Category = "PaymentTests",
            Kind = TestKind.Integration
        }
    }
};
```

### TestRunResult

Représente le résultat d'une série de tests unifiée.

```csharp
namespace PeasyPilot.Core.Models;

using PeasyPilot.Core.Eums;

public class TestRunResult
{
    /// <summary>Obtient ou définit le nombre total de tests réussis.</summary>
    public int Passed { get; set; }

    /// <summary>Obtient ou définit le nombre total de tests échoués.</summary>
    public int Failed { get; set; }

    /// <summary>Obtient ou définit le nombre total de tests ignorés.</summary>
    public int Skipped { get; set; }

    /// <summary>Obtient ou définit la durée totale de la séries de tests.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Obtient ou définit l'état final de la séries de tests.</summary>
    public TestRunStatus Status { get; set; }
}

public enum TestRunStatus
{
    Passed = 0,
    Failed = 1,
    Skipped = 2,
    Incomplete = 3
}
```

**Exemple : Analyser TestRunResult**
```csharp
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Eums;

public class AnalyseurDeRésultats
{
    public void AnalyserRésultatsDesSéries(TestRunResult result)
    {
        Console.WriteLine($"Résultats de la Série de Tests :");
        Console.WriteLine($"  Réussis :   {result.Passed}");
        Console.WriteLine($"  Échoués :   {result.Failed}");
        Console.WriteLine($"  Ignorés :  {result.Skipped}");
        Console.WriteLine($"  Durée : {result.Duration.TotalSeconds:F2}s");
        Console.WriteLine($"  État :   {result.Status}");

        int total = result.Passed + result.Failed + result.Skipped;
        double successRate = (double)result.Passed / total * 100;
        Console.WriteLine($"  Taux de Réussite : {successRate:F1}%");
    }

    public bool EstUnSuccès(TestRunResult result)
    {
        return result.Status == TestRunStatus.Passed && result.Failed == 0;
    }
}
```

### TestResult

Représente le résultat d'exécution d'un cas de test unique.

```csharp
namespace PeasyPilot.Core.Models;

public class TestResult
{
    /// <summary>Obtient ou définit le nom du test.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Obtient ou définit la catégorie ou la suite.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Obtient ou définit l'état d'exécution.</summary>
    public TestRunStatus Status { get; set; } = TestRunStatus.Passed;

    /// <summary>Obtient ou définit un message d'exécution optionnel.</summary>
    public string? Message { get; set; }

    /// <summary>Obtient ou définit les détails d'échec standardisés quand l'état est Failed.</summary>
    public TestFailure? Failure { get; set; }

    /// <summary>Obtient ou définit la durée d'exécution.</summary>
    public TimeSpan Duration { get; set; }
}

public class TestFailure
{
    /// <summary>Obtient ou définit le type d'exception.</summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>Obtient ou définit le message d'échec.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Obtient ou définit la trace de la pile.</summary>
    public string StackTrace { get; set; } = string.Empty;
}
```

**Exemple : Gérer TestResult avec Échecs**
```csharp
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Eums;

public class GestionnaireD'Échecs
{
    public void TraiterRésultatDuTest(TestResult result)
    {
        if (result.Status == TestRunStatus.Failed && result.Failure != null)
        {
            Console.WriteLine($"Test Échoué : {result.Name}");
            Console.WriteLine($"Exception : {result.Failure.ExceptionType}");
            Console.WriteLine($"Message : {result.Failure.Message}");
            Console.WriteLine($"Trace de Pile :\n{result.Failure.StackTrace}");
        }
        else if (result.Status == TestRunStatus.Passed)
        {
            Console.WriteLine($"Test Réussi : {result.Name} ({result.Duration.TotalMilliseconds}ms)");
        }
    }
}
```

---

## Configuration

### Configuration de l'Injection de Dépendances

Enregistrez les services PeasyPilot.Core dans votre conteneur DI :

```csharp
using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Discovery;
using PeasyPilot.Core.Engines;
using PeasyPilot.Core.Reporting;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Enregistrer la découverte des tests pour votre framework
        services.AddScoped<ITestDiscovery, ReflectionTestDiscovery>();

        // Enregistrer le moteur de test
        services.AddScoped<ITestEngine, TestEngine>();

        // Enregistrer les rapporteurs (ajouter plusieurs pour différents formats)
        services.AddScoped<ITestReporter, ConsoleReporter>();
        services.AddScoped<ITestReporter, JsonFileReporter>();

        // Enregistrer le contexte de test
        services.AddScoped<ITestContext, TestContext>();
    }
}
```

---

## Motifs Courants

### Motif 1 : Exécution Complète des Tests

```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

public class ChaîneD'ExécutionDesTests
{
    private readonly ITestDiscovery _discovery;
    private readonly ITestEngine _engine;
    private readonly ITestReporter _reporter;

    public ChaîneD'ExécutionDesTests(
        ITestDiscovery discovery,
        ITestEngine engine,
        ITestReporter reporter)
    {
        _discovery = discovery;
        _engine = engine;
        _reporter = reporter;
    }

    public async Task<string> ExécuterEtRapporterAsync()
    {
        // 1. Découvrir les tests
        var tests = await _discovery.DiscoverAsync();
        
        // 2. Construire la demande
        var request = new TestRunRequest
        {
            Name = "Suite Complète",
            TestCases = tests.Cast<TestCase>().ToList()
        };

        // 3. Exécuter
        var result = await _engine.RunAsync(request);

        // 4. Rapporter
        return await _reporter.ReportAsync(result);
    }
}
```

### Motif 2 : Exécution des Tests Filtrés

```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

public class ExécuteurDuTestFiltré
{
    private readonly ITestDiscovery _discovery;
    private readonly ITestEngine _engine;

    public async Task<TestRunResult> ExécuterLesTestsUnitairesUniquementAsync()
    {
        var allTests = await _discovery.DiscoverAsync();
        
        // Filtrer uniquement les tests unitaires
        var unitTests = allTests
            .Where(t => t.Kind == TestKind.Unit)
            .ToList();

        var request = new TestRunRequest
        {
            Name = "Tests Unitaires",
            TestCases = unitTests
        };

        return await _engine.RunAsync(request);
    }

    public async Task<TestRunResult> ExécuterLesTestsPrioritairesAsync()
    {
        var allTests = await _discovery.DiscoverAsync();
        
        // Filtrer par métadonnées de priorité
        var highPriority = allTests
            .Where(t => t.Metadata.ContainsKey("priority") &&
                       t.Metadata["priority"] == "high")
            .ToList();

        var request = new TestRunRequest
        {
            Name = "Tests de Haute Priorité",
            TestCases = highPriority
        };

        return await _engine.RunAsync(request);
    }
}
```

### Motif 3 : Gestion de l'État du Test Basée sur le Contexte

```csharp
using PeasyPilot.Core.Context;

public class GestionnaireD'ÉtatDuTest
{
    private readonly TestContext _context;

    public GestionnaireD'ÉtatDuTest()
    {
        _context = new TestContext();
    }

    public void InitialiserL'EnvironnementDuTest()
    {
        // Initialiser les ressources partagées une fois
        var database = _context.GetOrAdd("database", () =>
            new TestDatabase("connection_string"));

        var httpClient = _context.GetOrAdd("http_client", () =>
            new HttpClient { BaseAddress = new Uri("http://localhost:5000") });

        var config = _context.GetOrAdd("config", () =>
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build());
    }

    public T ObtenirOuCréerLe Service<T>(string key, Func<T> factory) where T : class
    {
        return _context.GetOrAdd(key, factory);
    }
}
```

---

## Résumé de Référence

| Composant | Objectif | Stabilité |
|-----------|----------|-----------|
| ITestDiscovery | Découvrir les tests de manière indépendante du framework | ✅ Stable |
| ITestEngine | Exécuter les tests et retourner les résultats | ✅ Stable |
| ITestReporter | Formater et rapporter les résultats de tests | ✅ Stable |
| ITestContext | Stockage d'état de test thread-safe | ✅ Stable |
| TestContext | Implémentation de ITestContext | ✅ Stable |
| TestCase | Représente un test unique | ✅ Stable |
| TestRunRequest | Représente une demande de série de tests | ✅ Stable |
| TestRunResult | Représente le résultat d'exécution global | ✅ Stable |
| TestResult | Représente le résultat d'un test unique | ✅ Stable |
| TestFailure | Détails d'échec | ✅ Stable |

---

## Voir Aussi

- **GETTING-STARTED-FR.md** — Guide de démarrage rapide
- **unit-testing-guide-FR.md** — Motifs de test unitaire
- **integration-testing-guide-FR.md** — Configuration des tests d'intégration
- **api-unit-FR.md** — API PeasyPilot.Unit
- **api-integration-FR.md** — API PeasyPilot.Integration

---

**[← Retour aux Références API](./README.md)** | **[← Retour au Hub Documentation](../README.md)**

**Version:** Français | **[English](./api-core.md)**
