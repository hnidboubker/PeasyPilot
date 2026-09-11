# Guide d'extensibilité et de contribution

## Aperçu

Ce guide vous montre comment étendre PeasyPilot avec des composants personnalisés et comment contribuer au projet.

**Vous apprendrez :**
- L'architecture des plugins et les points d'extension
- Créer des rapporteurs personnalisés pour des formats de sortie spécialisés
- Implémenter des analyseurs de code personnalisés
- Écrire des définitions d'étapes BDD personnalisées
- Créer des stratégies de moquage personnalisées
- Enregistrer les composants avec l'injection de dépendances
- Contribuer du code, des tests et de la documentation

**Prérequis :** Compléter [Démarrage rapide](../GETTING-STARTED-FR.md) et consulter [Patterns de test avancés](testing-patterns-FR.md)  
**Durée estimée :** 60 minutes  
**Exemples de code :** 10+ exemples fonctionnels  
**Frameworks :** xUnit, NUnit, TUnit

---

## Architecture des plugins

PeasyPilot est construit sur des abstractions extensibles. Chaque composant majeur a une interface que vous pouvez implémenter :

```
Abstractions principales (ITestReporter, ICodeAnalyzer, IMockFactory, etc.)
           ↓
Votre implémentation
           ↓
Enregistrer dans l'injection de dépendances
           ↓
PeasyPilot charge et utilise votre composant
```

### Modèle de découverte

Les composants de PeasyPilot sont découverts via l'injection de dépendances. Enregistrez votre implémentation, et le framework détecte et l'utilise automatiquement.

**Points d'extension clés :**

| Composant | Interface | Objectif |
|-----------|-----------|---------|
| **Rapporteurs** | `ITestReporter` | Formats personnalisés des résultats (JSON, XML, HTML) |
| **Analyseurs de code** | `ICodeAnalyzer` | Analyser les méthodes pour la génération de tests |
| **Fabriques de mocks** | `IMockFactory` | Stratégies personnalisées de création de mocks |
| **Contextes de test** | `ITestContext` | Stockage de contexte personnalisé |
| **Liaisons d'étapes** | `BddStepDefinition` (dérivée) | Implémentations d'étapes BDD |

---

## Rapporteurs personnalisés

### Comprendre ITestReporter

L'interface `ITestReporter` définit comment les résultats des tests sont formatés et affichés :

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestReporter
{
    Task<string> ReportAsync(
        TestRunResult result, 
        CancellationToken cancellationToken = default
    );
}
```

### Exemple 1 : Rapporteur JSON personnalisé

Créez un rapporteur qui sort un JSON détaillé avec formatage personnalisé :

```csharp
using System.Text.Json;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace MonApp.Testing.Reports;

public class RapporteurJsonPersonnalise : ITestReporter
{
    private readonly JsonSerializerOptions _optionsJson;

    public RapporteurJsonPersonnalise()
    {
        _optionsJson = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<string> ReportAsync(
        TestRunResult result, 
        CancellationToken cancellationToken = default)
    {
        var donneeRapport = new
        {
            resume = new
            {
                result.TotalTests,
                result.PassedTests,
                result.FailedTests,
                result.SkippedTests,
                Duree = result.Duration?.TotalMilliseconds
            },
            tests = result.Results.Select(r => new
            {
                r.TestName,
                r.Status,
                r.Message,
                DureeMs = r.Duration?.TotalMilliseconds
            }).ToList(),
            horodatage = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(
            donneeRapport, 
            _optionsJson
        );

        return await Task.FromResult(json);
    }
}
```

### Exemple 2 : Rapporteur HTML

Créez un rapporteur qui génère un rapport HTML :

```csharp
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace MonApp.Testing.Reports;

public class RapporteurHtml : ITestReporter
{
    public async Task<string> ReportAsync(
        TestRunResult result, 
        CancellationToken cancellationToken = default)
    {
        var pourcentageReussite = result.TotalTests > 0 
            ? (result.PassedTests / (decimal)result.TotalTests) * 100 
            : 0;

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Rapport de test</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .resume {{ background: #f0f0f0; padding: 15px; border-radius: 5px; }}
        .liste-tests {{ margin-top: 20px; }}
        .reussi {{ color: green; }}
        .echoue {{ color: red; }}
        .saute {{ color: orange; }}
    </style>
</head>
<body>
    <h1>Rapport d'exécution des tests</h1>
    <div class='resume'>
        <h2>Résumé</h2>
        <p>Tests totaux : {result.TotalTests}</p>
        <p class='reussi'>Réussis : {result.PassedTests}</p>
        <p class='echoue'>Échoués : {result.FailedTests}</p>
        <p class='saute'>Sautés : {result.SkippedTests}</p>
        <p>Taux de réussite : {pourcentageReussite:F2}%</p>
    </div>
    <div class='liste-tests'>
        <h2>Résultats des tests</h2>
        <ul>
            {string.Join("", result.Results.Select(r => 
                $"<li class='{r.Status.ToString().ToLower()}'>{r.TestName} : {r.Status}</li>"
            ))}
        </ul>
    </div>
</body>
</html>";

        return await Task.FromResult(html);
    }
}
```

### Enregistrement des rapporteurs personnalisés

Enregistrez votre rapporteur dans l'injection de dépendances :

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Enregistrer votre rapporteur personnalisé
services.AddSingleton<ITestReporter, RapporteurJsonPersonnalise>();

var serviceProvider = services.BuildServiceProvider();
var reporter = serviceProvider.GetRequiredService<ITestReporter>();
```

---

## Analyseurs de code personnalisés

### Comprendre ICodeAnalyzer

L'interface `ICodeAnalyzer` analyse les méthodes pour la génération de tests :

```csharp
namespace PeasyPilot.TestAssistant.Abstractions;

public interface ICodeAnalyzer
{
    Task<IReadOnlyList<MethodTestModel>> AnalyzeFileAsync(string filePath);
    Task<IReadOnlyList<MethodTestModel>> AnalyzeTypeAsync(Type type);
    Task<MethodTestModel?> AnalyzeMethodAsync(Type? type, string methodName);
}
```

### Exemple 3 : Analyseur personnalisé Python-vers-CSharp

Imaginez que vous voulez analyser du code Python et suggérer des patterns de test CSharp :

```csharp
using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;

namespace MonApp.Analysis;

public class AnalyseurPatternPython : ICodeAnalyzer
{
    public async Task<IReadOnlyList<MethodTestModel>> AnalyzeFileAsync(string filePath)
    {
        if (!filePath.EndsWith(".py"))
            throw new ArgumentException("Fichier .py attendu");

        var codePython = await File.ReadAllTextAsync(filePath);
        var methodes = ExtraireMethodesPython(codePython);
        
        return methodes
            .Select(m => AnalyserMethodePython(m))
            .ToList()
            .AsReadOnly();
    }

    public Task<IReadOnlyList<MethodTestModel>> AnalyzeTypeAsync(Type type)
    {
        throw new NotSupportedException("Utilisez AnalyzeFileAsync pour les fichiers Python");
    }

    public Task<MethodTestModel?> AnalyzeMethodAsync(Type? type, string methodName)
    {
        throw new NotSupportedException("Utilisez AnalyzeFileAsync pour les fichiers Python");
    }

    private MethodTestModel AnalyserMethodePython(string codeMethode)
    {
        // Extraire le nom de la méthode, les paramètres, les exceptions, etc. du code Python
        var nom = ExtraireNomMethode(codeMethode);
        var parametres = ExtraireParametres(codeMethode);
        var exceptions = ExtraireExceptions(codeMethode);

        return new MethodTestModel
        {
            MethodName = nom,
            Parameters = parametres,
            Exceptions = exceptions,
            IsAsync = codeMethode.Contains("async def"),
            TestableScenarios = GenererScenarios(codeMethode)
        };
    }

    private string ExtraireNomMethode(string code) => "nom_methode";
    private List<MethodParameterInfo> ExtraireParametres(string code) => new();
    private List<ExceptionInfo> ExtraireExceptions(string code) => new();
    private List<TestableScenario> GenererScenarios(string code) => new();
}
```

---

## Définitions d'étapes personnalisées (BDD)

### Comprendre BddStepDefinition

Les définitions d'étapes lient les étapes Gherkin au code C#. Créez une classe qui hérite de `BddStepDefinition` :

```csharp
namespace PeasyPilot.BDD.StepDefinitions;

public abstract class BddStepDefinition
{
    // Vos méthodes d'étapes avec les attributs [Given], [When], [Then]
}
```

### Exemple 4 : Définitions d'étapes pour test d'API

```csharp
using PeasyPilot.BDD.StepDefinitions;
using System.Net.Http;

namespace MonApp.BddTests;

public class EtapesApi : BddStepDefinition
{
    private HttpClient? _httpClient;
    private HttpResponseMessage? _derniereReponse;
    private string? _urlBase;

    [Given("une API sur {url}")]
    public void ConfigurerEndpointApi(string url)
    {
        _urlBase = url;
        _httpClient = new HttpClient { BaseAddress = new Uri(url) };
    }

    [When("je fais une requête GET vers {endpoint}")]
    public async Task FaireRequeteGet(string endpoint)
    {
        if (_httpClient == null)
            throw new InvalidOperationException("API non configurée");

        _derniereReponse = await _httpClient.GetAsync(endpoint);
    }

    [When("j'envoie une requête POST avec {json}")]
    public async Task FaireRequetePost(string json)
    {
        if (_httpClient == null)
            throw new InvalidOperationException("API non configurée");

        var contenu = new StringContent(
            json, 
            System.Text.Encoding.UTF8, 
            "application/json"
        );
        _derniereReponse = await _httpClient.PostAsync("/api/endpoint", contenu);
    }

    [Then("le code de statut de la réponse doit être {statusCode:int}")]
    public void AssertionCodeStatut(int statusCode)
    {
        if (_derniereReponse == null)
            throw new InvalidOperationException("Aucune réponse enregistrée");

        var actual = (int)_derniereReponse.StatusCode;
        if (actual != statusCode)
            throw new AssertionException(
                $"Code statut attendu {statusCode}, obtenu {actual}"
            );
    }

    [Then("la réponse doit contenir {texteAttendu}")]
    public async Task AssertionContenuReponse(string texteAttendu)
    {
        if (_derniereReponse == null)
            throw new InvalidOperationException("Aucune réponse enregistrée");

        var contenu = await _derniereReponse.Content.ReadAsStringAsync();
        if (!contenu.Contains(texteAttendu))
            throw new AssertionException(
                $"La réponse ne contient pas '{texteAttendu}'"
            );
    }
}
```

### Exemple 5 : Définitions d'étapes pour test de base de données

```csharp
using PeasyPilot.BDD.StepDefinitions;

namespace MonApp.BddTests;

public class EtapesBaseDonnees : BddStepDefinition
{
    private ITestDatabase? _baseDonnees;
    private List<(string NomTable, object Donnees)> _donneesInserees = new();

    [Given("une base de données vierge")]
    public async Task InitialiserBaseDonnees()
    {
        _baseDonnees = new InMemoryTestDatabase();
        await _baseDonnees.InitializeAsync();
    }

    [Given("la table users contient {count:int} utilisateurs")]
    public async Task EnsemencerUtilisateurs(int count)
    {
        if (_baseDonnees == null)
            throw new InvalidOperationException("Base de données non initialisée");

        for (int i = 1; i <= count; i++)
        {
            var utilisateur = new { Id = i, Name = $"User{i}", Email = $"user{i}@test.com" };
            await _baseDonnees.InsertAsync("Users", utilisateur);
            _donneesInserees.Add(("Users", utilisateur));
        }
    }

    [When("je fais une requête pour l'utilisateur {userId:int}")]
    public async Task RequeteUtilisateur(int userId)
    {
        if (_baseDonnees == null)
            throw new InvalidOperationException("Base de données non initialisée");

        var utilisateur = await _baseDonnees.QueryAsync("Users", userId);
        // Stocker dans le contexte d'étape
    }

    [Then("l'utilisateur doit exister")]
    public void AssertionUtilisateurExiste()
    {
        if (_baseDonnees == null)
            throw new InvalidOperationException("Base de données non initialisée");
        // Vérifier que l'utilisateur a été trouvé
    }
}
```

---

## Stratégies de moquage personnalisées

### Comprendre IMockFactory

L'interface `IMockFactory` crée des objets mock :

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface IMockFactory
{
    object Create(Type type);
}
```

### Exemple 6 : Fabrique de mock avancée avec enregistrement

Créez une fabrique de mock qui enregistre les appels de méthode :

```csharp
using Moq;
using PeasyPilot.Core.Abstractions;

namespace MonApp.Testing.Mocks;

public class FabriqueMockAvecEnregistrement : IMockFactory
{
    private readonly Dictionary<Type, List<(string Methode, object?[] Args)>> _historiqueLAppels;

    public FabriqueMockAvecEnregistrement()
    {
        _historiqueLAppels = new();
    }

    public object Create(Type type)
    {
        var typeMock = typeof(Mock<>).MakeGenericType(type);
        var instanceMock = (Mock)Activator.CreateInstance(typeMock)!;

        // Configurer l'enregistrement de tous les appels de méthode
        EnregistrerAppelsMethode(instanceMock, type);

        return instanceMock.GetType().GetProperty("Object")!.GetValue(instanceMock)!;
    }

    private void EnregistrerAppelsMethode(Mock instanceMock, Type typeInterface)
    {
        if (!_historiqueLAppels.ContainsKey(typeInterface))
            _historiqueLAppels[typeInterface] = new();

        // Configurer le mock pour suivre les appels
        var proprieteAppels = instanceMock.GetType().GetProperty("Calls");
        if (proprieteAppels != null)
        {
            var appels = proprieteAppels.GetValue(instanceMock);
            // Enregistrer les appels au fur et à mesure
        }
    }

    public IReadOnlyList<(string Methode, object?[] Args)> ObtenirHistoriqueAppels(Type type)
    {
        return _historiqueLAppels.ContainsKey(type) 
            ? _historiqueLAppels[type].AsReadOnly() 
            : new List<(string, object?[])>().AsReadOnly();
    }

    public void Reinitialiser(Type type)
    {
        if (_historiqueLAppels.ContainsKey(type))
            _historiqueLAppels[type].Clear();
    }
}
```

### Exemple 7 : Fabrique de mock Stub

Créez une fabrique simple pour des mocks rapides :

```csharp
using PeasyPilot.Core.Abstractions;

namespace MonApp.Testing.Mocks;

public class FabriqueMockStub : IMockFactory
{
    public object Create(Type type)
    {
        if (!type.IsInterface)
            throw new ArgumentException("Seules les interfaces peuvent être stubifiées", nameof(type));

        // Créer un objet proxy en utilisant DispatchProxy
        return DispatchProxy.Create(type, typeof(IntercepteurStub<>)
            .MakeGenericType(type));
    }

    private class IntercepteurStub<T> : DispatchProxy where T : class
    {
        protected override object? Invoke(
            System.Reflection.MethodInfo? methodeTarget, 
            object?[]? args)
        {
            // Retourner les valeurs par défaut pour tous les appels
            return methodeTarget?.ReturnType == typeof(void)
                ? null
                : Activator.CreateInstance(methodeTarget?.ReturnType ?? typeof(object));
        }
    }
}
```

---

## Intégration de l'injection de dépendances

### Enregistrement des composants

PeasyPilot utilise Microsoft.Extensions.DependencyInjection. Enregistrez vos composants personnalisés :

### Exemple 8 : Configuration DI complète

```csharp
using Microsoft.Extensions.DependencyInjection;
using MonApp.Testing.Reports;
using MonApp.Testing.Mocks;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Integration.Fixtures;

namespace MonApp.Testing;

public class ConfigurationServiceTest : IntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Enregistrer les rapporteurs personnalisés
        services.AddSingleton<ITestReporter, RapporteurJsonPersonnalise>();
        
        // Ou enregistrer avec un pattern factory
        services.AddSingleton<ITestReporter>(sp => 
            new RapporteurHtml()
        );

        // Enregistrer les fabriques de mock personnalisées
        services.AddSingleton<IMockFactory, FabriqueMockAvecEnregistrement>();

        // Enregistrer vos services d'application
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IOrderService, OrderService>();
    }
}
```

### Durées de vie des services

Choisissez la durée de vie appropriée pour vos composants :

```csharp
// Singleton : Créé une fois, partagé entre tous les tests
services.AddSingleton<IUserRepository, UserRepository>();

// Transient : Créé frais à chaque demande
services.AddTransient<IOrderService, OrderService>();

// Scoped : Créé une fois par scope (utile pour le contexte de test)
services.AddScoped<ITestContext, TestContext>();
```

---

## Contribuer à PeasyPilot

### Directives de contribution de code

#### 1. Forker et créer une branche

```bash
# Cloner le référentiel
git clone https://github.com/Houssine/PeasyPilot.git
cd PeasyPilot

# Créer une branche de fonctionnalité
git checkout -b feature/nom-de-ma-fonction
```

#### 2. Style de code

Suivez le style de code existant :

```csharp
// ✅ BON : Formatage et nommage appropriés
namespace PeasyPilot.NouvelleFonctionnalite.Abstractions;

public interface IMaNovelleAbstraction
{
    Task<string> TraiterAsync(string entree, CancellationToken cancellationToken = default);
}

// ❌ MAUVAIS : Style inconsistent
namespace PeasyPilot.NouvelleFonctionnalite;
public interface IMaNovelleAbstraction{Task<string> TraiterAsync(string entree);}
```

#### 3. Écrire les tests de votre fonctionnalité

Créez des tests exhaustifs dans le projet de test approprié :

```csharp
using Xunit;
using PeasyPilot.NouvelleFonctionnalite;

namespace PeasyPilot.Tests.NouvelleFonctionnalite;

public class TestMaNouvelleFonctionnalite
{
    [Fact]
    public async Task TraiterAsync_AvecEntreeValide_RetourneResultatAttendu()
    {
        // Arrange
        var fonctionnalite = new MaNouvelleFonctionnalite();
        var entree = "entrée de test";

        // Act
        var resultat = await fonctionnalite.TraiterAsync(entree);

        // Assert
        Assert.Equal("sortie attendue", resultat);
    }

    [Fact]
    public async Task TraiterAsync_AvecEntreeNull_LevException ArgumentNullException()
    {
        // Arrange
        var fonctionnalite = new MaNouvelleFonctionnalite();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => fonctionnalite.TraiterAsync(null!)
        );
    }
}
```

#### 4. Ajouter la documentation XML

Documentez les types publics et les membres :

```csharp
/// <summary>
/// Traite les données d'entrée selon la logique métier personnalisée.
/// </summary>
/// <param name="entree">La chaîne d'entrée à traiter.</param>
/// <param name="cancellationToken">Le jeton d'annulation.</param>
/// <returns>Le résultat traité.</returns>
/// <exception cref="ArgumentNullException">Levée si l'entrée est null.</exception>
public async Task<string> TraiterAsync(
    string entree, 
    CancellationToken cancellationToken = default)
{
}
```

#### 5. Mettre à jour la documentation

Si votre fonctionnalité s'adresse aux utilisateurs, ajoutez la documentation :

- Créer ou mettre à jour un guide dans `docs/GUIDES/`
- Ajouter des exemples avec du code fonctionnel
- Inclure les versions anglaise et française
- Lier depuis `README.md`

#### 6. Soumettre une demande de fusion

```bash
# Valider vos modifications
git add .
git commit -m "feat: Ajouter ma nouvelle fonctionnalité avec tests et docs"

# Pousser vers votre fork
git push origin feature/nom-de-ma-fonction
```

Ensuite, créez une PR sur GitHub avec :
- Description claire de la fonctionnalité
- Lien vers les problèmes connexes
- Liste de contrôle de test
- Exemple d'utilisation

### Exigences de test

Toutes les contributions doivent réussir :

```bash
# Exécuter tous les tests sur tous les frameworks
dotnet test

# Exécuter les tests pour un framework spécifique
dotnet test --framework net10.0

# Vérifier la couverture de code (le cas échéant)
dotnet test /p:CollectCoverage=true
```

### Normes de documentation

**Exigences du guide en anglais :**
- ~2 000-3 000 mots
- 5+ exemples de code fonctionnels
- Structure progressive (basique → avancée)
- Titres de sections clairs
- Liens croisés vers les documents connexes

**Exigences de traduction en français :**
- Français professionnel (pas traduit par machine)
- Maintient la structure et les exemples originaux
- Tous les commentaires de code traduits
- Tous les titres et étiquettes traduits

---

## Configuration du développement

### Prérequis

- SDK .NET 8.0 ou version ultérieure (ciblant actuellement 10.0)
- Git
- IDE : Visual Studio 2022, VS Code ou Rider

### Cloner et créer

```bash
# Cloner le référentiel
git clone https://github.com/Houssine/PeasyPilot.git
cd PeasyPilot

# Restaurer les paquets NuGet
dotnet restore

# Créer tous les projets
dotnet build

# Exécuter tous les tests
dotnet test
```

### Structure du projet

```
src/                      # Code de production (paquets NuGet)
├── PeasyPilot.Core/      # Abstractions et modèles principaux
├── PeasyPilot.BDD/       # Behavior-Driven Development
├── PeasyPilot.XUnit/     # Adaptateur xUnit
├── PeasyPilot.NUnit/     # Adaptateur NUnit
└── [autres paquets]

tests/                    # Tests unitaires et d'intégration
├── PeasyPilot.Core.Tests/
└── [tests spécifiques au framework]

samples/                  # Exemples fonctionnels
├── PeasyPilot.XUnit.Samples/
└── [autres exemples]

docs/                     # Documentation
├── GETTING-STARTED.md
├── GUIDES/
├── MCP/
├── REFERENCE/
└── ADVANCED/
```

### Effectuer des modifications

1. Créer une branche : `git checkout -b feature/description`
2. Faire vos modifications dans `src/`
3. Écrire des tests dans `tests/`
4. Mettre à jour les docs si nécessaire
5. Exécuter `dotnet build && dotnet test`
6. Valider et pousser

### Exécuter les tests spécifiques

```bash
# Exécuter les tests pour une classe spécifique
dotnet test --filter "NomClasse"

# Exécuter les tests correspondant à un motif
dotnet test --filter "TestMethod*"

# Exécuter avec sortie détaillée
dotnet test -v detailed
```

---

## Scénarios d'extension courants

### Scénario 1 : Ajouter le support d'un nouveau format de sortie

1. Implémenter `ITestReporter`
2. Enregistrer dans la configuration DI
3. Ajouter les tests
4. Documenter l'utilisation

### Scénario 2 : Analyseur de code personnalisé pour langage de domaine

1. Implémenter `ICodeAnalyzer`
2. Analyser votre langage spécifique au domaine
3. Générer des scénarios de test
4. Enregistrer et tester

### Scénario 3 : Nouvel adaptateur de framework

1. Implémenter `ITestDiscovery` et `ITestOrchestrator`
2. Gérer les attributs spécifiques au framework
3. Créer le paquet adaptateur correspondant
4. Ajouter des exemples et des tests

---

## Support et questions

- **Problèmes :** https://github.com/Houssine/PeasyPilot/issues
- **Discussions :** https://github.com/Houssine/PeasyPilot/discussions
- **Documentation :** Voir le dossier docs/

---

[← Retour aux guides avancés](README.md)
