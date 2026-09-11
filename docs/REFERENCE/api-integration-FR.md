# Référence API PeasyPilot.Integration

## Aperçu

`PeasyPilot.Integration` fournit un support complet pour les tests d'intégration, notamment la gestion de fixtures de base de données, les assistants de client HTTP, les factories d'application ASP.NET Core et la gestion de l'authentification. Il permet une configuration et un démontage transparents d'environnements de test complexes avec injection de dépendances et gestion du cycle de vie de la base de données.

**Responsabilités principales :**
- Gestion de fixtures de base de données en mémoire et externes
- Fixtures de test d'intégration de base avec DI
- Factories d'application de test ASP.NET Core
- Assistants de client HTTP de test
- Gestion de fixtures d'authentification
- Abstraction de factory de base de données
- Gestion du cycle de vie des services (IResettable)

**Cibles :** .NET 8.0, 9.0, 10.0

---

## Abstractions Principales

### ITestDatabase

Interface de gestion des opérations de base de données de test.

```csharp
namespace PeasyPilot.Integration.Abstractions;

/// <summary>
/// Interface de gestion des opérations de base de données de test.
/// </summary>
public interface ITestDatabase
{
    /// <summary>
    /// Initialise la base de données de manière asynchrone.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Nettoie la base de données de manière asynchrone.
    /// </summary>
    Task CleanupAsync();

    /// <summary>
    /// Remplit la base de données avec des données de test de manière asynchrone.
    /// </summary>
    Task SeedAsync();

    /// <summary>
    /// Réinitialise la base de données à son état initial de manière asynchrone.
    /// </summary>
    Task ResetAsync();
}
```

**Objectif :** Fournit la gestion du cycle de vie (Initialize → Seed → Reset → Cleanup) pour les bases de données de test.

**Exemple : Implémenter ITestDatabase**
```csharp
using PeasyPilot.Integration.Abstractions;

public class BaseDeDonnéesTest : ITestDatabase
{
    private readonly DbContext _context;
    private readonly IEnumerable<TestDataSeed> _seeds;

    public BaseDeDonnéesTest(DbContext context, IEnumerable<TestDataSeed> seeds)
    {
        _context = context;
        _seeds = seeds;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task SeedAsync()
    {
        foreach (var seed in _seeds)
        {
            await seed.ExecuteAsync(_context);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ResetAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await InitializeAsync();
        await SeedAsync();
    }

    public async Task CleanupAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }
}
```

### ITestDatabaseFactory

Interface de factory pour créer des instances de base de données de test.

```csharp
namespace PeasyPilot.Integration.Abstractions;

/// <summary>
/// Interface de factory pour créer des instances de base de données de test.
/// </summary>
public interface ITestDatabaseFactory
{
    /// <summary>
    /// Crée une nouvelle instance de base de données de test.
    /// </summary>
    /// <returns>Une instance ITestDatabase configurée.</returns>
    ITestDatabase CreateDatabase();
}
```

**Objectif :** Active l'injection de dépendances et l'échange de différentes implémentations de base de données (en mémoire, SQLite, véritable BD).

**Exemple : Implémenter ITestDatabaseFactory**
```csharp
using PeasyPilot.Integration.Abstractions;
using Microsoft.EntityFrameworkCore;

public class FactoryBaseDeDonnéesPersonnalisée : ITestDatabaseFactory
{
    private readonly string _connectionString;
    private readonly IEnumerable<TestDataSeed> _seeds;

    public FactoryBaseDeDonnéesPersonnalisée(string connectionString, IEnumerable<TestDataSeed> seeds)
    {
        _connectionString = connectionString;
        _seeds = seeds;
    }

    public ITestDatabase CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connectionString)
            .Options;

        var context = new ApplicationDbContext(options);
        return new BaseDeDonnéesTest(context, _seeds);
    }
}
```

### IResettable

Interface pour les services qui peuvent être réinitialisés lors de l'exécution du test.

```csharp
namespace PeasyPilot.Integration.Abstractions;

/// <summary>
/// Interface pour les services qui doivent être réinitialisés entre les séries de tests.
/// </summary>
public interface IResettable
{
    /// <summary>
    /// Réinitialise le service à son état initial de manière asynchrone.
    /// </summary>
    Task ResetAsync();
}
```

**Objectif :** Permet aux services singleton d'être réinitialisés entre les tests sans élimination complète.

**Exemple : Implémenter IResettable**
```csharp
using PeasyPilot.Integration.Abstractions;

public class ServiceCache : IResettable
{
    private Dictionary<string, object> _cache = new();

    public void Set(string key, object value)
    {
        _cache[key] = value;
    }

    public object? Get(string key)
    {
        return _cache.TryGetValue(key, out var value) ? value : null;
    }

    public async Task ResetAsync()
    {
        _cache.Clear();
        await Task.CompletedTask;
    }
}
```

---

## Classes Principales

### IntegrationTestFixture

Classe de base pour les tests d'intégration avec injection de dépendances et gestion du cycle de vie de la base de données.

```csharp
namespace PeasyPilot.Integration.Fixtures;

public abstract class IntegrationTestFixture : IAsyncDisposable
{
    /// <summary>
    /// Obtient le fournisseur de services pour l'injection de dépendances.
    /// </summary>
    protected IServiceProvider Services { get; }

    /// <summary>
    /// Obtient l'instance de base de données de test.
    /// </summary>
    protected ITestDatabase Database { get; }

    /// <summary>
    /// Configure le conteneur d'injection de dépendances.
    /// Remplacez cette méthode pour enregistrer vos services.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Implémentation vide par défaut
    }

    /// <summary>
    /// Crée la factory de base de données pour les instances de base de données de test.
    /// </summary>
    protected virtual ITestDatabaseFactory CreateDatabaseFactory()
    {
        return new InMemoryDatabaseFactory();
    }

    /// <summary>
    /// Initialise la fixture : configure le conteneur DI et la base de données.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Implémentation...
    }

    /// <summary>
    /// Nettoie les ressources : réinitialise la base de données et élimine les services.
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        // Implémentation...
    }

    /// <summary>
    /// Réinitialise la base de données à son état initial.
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        // Implémentation...
    }
}
```

**Objectif :** Fournit un environnement de test d'intégration complet avec DI, gestion de base de données et cycle de vie async.

**Exemple : Utiliser IntegrationTestFixture**
```csharp
using PeasyPilot.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class TestsRépositoireUtilisateur : IntegrationTestFixture, IAsyncLifetime
{
    private IRépositoireUtilisateur _repository;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IRépositoireUtilisateur, RépositoireUtilisateur>();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await InitializeAsync();
        _repository = Services.GetRequiredService<IRépositoireUtilisateur>();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
    }

    [Fact]
    public async Task CreateUser_AvecDonnéesValides_RetourneId()
    {
        var user = new Utilisateur { Name = "John", Email = "john@example.com" };
        var id = await _repository.CreateAsync(user);

        Assert.True(id > 0);
    }

    [Fact]
    public async Task GetUser_AvecIdExistant_RetourneUtilisateur()
    {
        var user = new Utilisateur { Name = "Jane", Email = "jane@example.com" };
        var id = await _repository.CreateAsync(user);

        var retrieved = await _repository.GetAsync(id);

        Assert.NotNull(retrieved);
        Assert.Equal("Jane", retrieved.Name);
    }

    [Fact]
    public async Task ResetBetweenTests_VidelaBase()
    {
        await ResetDatabaseAsync();
        
        var users = await _repository.GetAllAsync();
        
        Assert.Empty(users);
    }
}
```

### InMemoryTestDatabase

Implémentation de base de données en mémoire sauvegardée par ITestStore.

```csharp
namespace PeasyPilot.Integration.Fixtures;

public class InMemoryTestDatabase : ITestDatabase
{
    /// <summary>
    /// Obtient le magasin de test en mémoire sous-jacent.
    /// </summary>
    public ITestStore Store { get; }

    /// <summary>
    /// Initialise une nouvelle instance avec un magasin personnalisé optionnel.
    /// </summary>
    public InMemoryTestDatabase(ITestStore? store = null)
    {
        Store = store ?? new InMemoryTestStore();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task CleanupAsync() => Store.ResetAsync();

    public Task SeedAsync() => Task.CompletedTask;

    public Task ResetAsync() => Store.ResetAsync();
}
```

**Objectif :** Base de données de test en mémoire rapide pour les tests unitaires et les tests d'intégration légers.

**Exemple : Utiliser InMemoryTestDatabase**
```csharp
using PeasyPilot.Integration.Fixtures;

[Fact]
public async Task InMemoryDatabase_RapidePourLesTests()
{
    var database = new InMemoryTestDatabase();
    await database.InitializeAsync();

    // Utiliser la base de données pour les tests...

    await database.CleanupAsync();
}
```

### WebApplicationTestFactory<TStartup>

Factory de test pour les applications ASP.NET Core avec services et middleware configurables.

```csharp
namespace PeasyPilot.Integration.Fixtures;

public class WebApplicationTestFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    /// <summary>
    /// Configure les services pour l'application de test.
    /// </summary>
    public WebApplicationTestFactory<TStartup> WithServices(
        Action<IServiceCollection> configure)
    {
        // Implémentation...
        return this;
    }

    /// <summary>
    /// Configure le pipeline middleware de l'application.
    /// </summary>
    public WebApplicationTestFactory<TStartup> WithApp(
        Action<IApplicationBuilder> configure)
    {
        // Implémentation...
        return this;
    }

    /// <summary>
    /// Configure le constructeur d'hôte web directement.
    /// </summary>
    public WebApplicationTestFactory<TStartup> WithWebHostBuilder(
        Action<IWebHostBuilder> configure)
    {
        // Implémentation...
        return this;
    }

    /// <summary>
    /// Crée un client HTTP pour les demandes de test.
    /// </summary>
    public HttpClient CreateTestClient()
    {
        return CreateClient();
    }

    /// <summary>
    /// Crée un client HTTP avec des assistants d'assertion.
    /// </summary>
    public HttpTestClient CreateHttpTestClient()
    {
        return new HttpTestClient(CreateTestClient());
    }
}
```

**Objectif :** API fluente pour configurer les tests d'application ASP.NET Core avec remplacements de services et injection de middleware.

**Exemple : Utiliser WebApplicationTestFactory**
```csharp
using PeasyPilot.Integration.Fixtures;
using Xunit;

public class TestsIntégrationApi : IAsyncLifetime
{
    private WebApplicationTestFactory<Startup> _factory;
    private HttpClient _client;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationTestFactory<Startup>()
            .WithServices(services =>
            {
                // Remplacer les services pour les tests
                services.AddScoped<IServiceUtilisateur, ServiceUtilisateurMock>();
            })
            .WithApp(app =>
            {
                // Ajouter le middleware de test
                app.UseMiddleware<TestAuthenticationMiddleware>();
            });

        _client = _factory.CreateTestClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetApi_RetourneOk()
    {
        var response = await _client.GetAsync("/api/users");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostApi_CrééeRessource()
    {
        var content = new StringContent("{\"name\":\"John\"}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/users", content);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }
}
```

### HttpTestClient

Client HTTP avec assistants d'assertion pour les tests d'intégration.

```csharp
namespace PeasyPilot.Integration.Helpers;

public class HttpTestClient
{
    private readonly HttpClient _httpClient;

    public HttpTestClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Obtient le client HTTP sous-jacent.
    /// </summary>
    public HttpClient Client => _httpClient;

    /// <summary>
    /// Fait une demande GET et affirme l'état de succès.
    /// </summary>
    public async Task<T> GetAsJsonAsync<T>(string requestUri)
    {
        var response = await _httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content)!;
    }

    /// <summary>
    /// Fait une demande POST et affirme l'état de succès.
    /// </summary>
    public async Task<T> PostAsJsonAsync<T>(string requestUri, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseContent)!;
    }
}
```

**Objectif :** Simplifie les opérations de test HTTP courantes avec sérialisation JSON automatique et assertions de code de statut.

**Exemple : Utiliser HttpTestClient**
```csharp
using PeasyPilot.Integration.Helpers;

public class TestsClientApi
{
    [Fact]
    public async Task GetUser_RetourneObjetAnalysé()
    {
        var client = new HttpTestClient(new HttpClient { BaseAddress = new Uri("http://api.example.com") });

        var user = await client.GetAsJsonAsync<Utilisateur>("/api/users/1");

        Assert.NotNull(user);
        Assert.Equal("John", user.Name);
    }

    [Fact]
    public async Task CreateUser_RetourneUtilisateurCréé()
    {
        var client = new HttpTestClient(new HttpClient { BaseAddress = new Uri("http://api.example.com") });
        var newUser = new Utilisateur { Name = "Jane", Email = "jane@example.com" };

        var created = await client.PostAsJsonAsync<Utilisateur>("/api/users", newUser);

        Assert.NotNull(created.Id);
    }
}

public class Utilisateur
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

---

## Motifs Courants

### Motif 1 : Test d'Intégration De Bout en Bout

```csharp
using PeasyPilot.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;

public class TestsTraitementCommande : IntegrationTestFixture, IAsyncLifetime
{
    private IServiceCommande _orderService;
    private IServicePaiement _paymentService;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IServiceCommande, ServiceCommande>();
        services.AddScoped<IServicePaiement, ServicePaiement>();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("OrderDb"));
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await InitializeAsync();
        _orderService = Services.GetRequiredService<IServiceCommande>();
        _paymentService = Services.GetRequiredService<IServicePaiement>();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
    }

    [Fact]
    public async Task ProcessCommande_ValideEtPaie()
    {
        var commande = new Commande { ItemCount = 3, Total = 99.99m };
        
        await _orderService.CreateAsync(commande);
        var paid = await _paymentService.ProcessAsync(commande.Id);

        Assert.True(paid);
    }
}
```

### Motif 2 : Pattern Factory de Base de Données

```csharp
using PeasyPilot.Integration.Abstractions;

public class FactorySqlite : ITestDatabaseFactory
{
    private readonly string _dbFile;

    public FactorySqlite(string dbFile = "test.db")
    {
        _dbFile = dbFile;
    }

    public ITestDatabase CreateDatabase()
    {
        var connectionString = $"Data Source={_dbFile};";
        var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connectionString)
                .Options);

        return new BaseDeDonnéesSqlite(context);
    }
}

public class BaseDeDonnéesSqlite : ITestDatabase
{
    private readonly DbContext _context;

    public BaseDeDonnéesSqlite(DbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task SeedAsync()
    {
        // Ajouter les données de test
        await _context.SaveChangesAsync();
    }

    public async Task ResetAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await InitializeAsync();
        await SeedAsync();
    }

    public async Task CleanupAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }
}
```

### Motif 3 : ASP.NET Core avec Remplacement de Service

```csharp
using PeasyPilot.Integration.Fixtures;

public class TestsIntégrationAuthentification : IAsyncLifetime
{
    private WebApplicationTestFactory<Startup> _factory;
    private HttpClient _client;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationTestFactory<Startup>()
            .WithServices(services =>
            {
                services.AddScoped<IServiceAuthentification, ServiceAuthenticationMock>();
                services.AddScoped<IRépositoireUtilisateur, RépositoireUtilisateurMock>();
            })
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            });

        _client = _factory.CreateTestClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Login_AvecIdentifiantsValides_RetourneJeton()
    {
        var loginRequest = new { email = "test@example.com", password = "password" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

## Configuration

### Configuration de l'Injection de Dépendances

```csharp
using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Integration.Abstractions;
using PeasyPilot.Integration.Fixtures;

public class ConfigurationTestIntégration
{
    public static IServiceCollection AddIntegrationTestSupport(
        this IServiceCollection services)
    {
        // Enregistrer la factory de base de données
        services.AddScoped<ITestDatabaseFactory, InMemoryDatabaseFactory>();

        return services;
    }
}
```

---

## Résumé de Référence

| Composant | Objectif | Utilisation |
|-----------|----------|------------|
| ITestDatabase | Gestion du cycle de vie (init, seed, reset, cleanup) | Interface pour implémentations |
| ITestDatabaseFactory | Créer des instances de base de données | Pattern Factory, DI |
| IResettable | Réinitialiser les services singleton entre les tests | Interface pour services |
| IntegrationTestFixture | Classe de base pour les tests d'intégration | Héritage dans les classes de test |
| InMemoryTestDatabase | Base de données de test rapide en mémoire | Instanciation directe |
| WebApplicationTestFactory<T> | Test d'application ASP.NET Core | Héritage ou composition |
| HttpTestClient | Assistants de test HTTP | Instanciation directe |

---

## Voir Aussi

- **GETTING-STARTED-FR.md** — Guide de démarrage rapide
- **integration-testing-guide-FR.md** — Guide complet des tests d'intégration
- **api-core-FR.md** — API PeasyPilot.Core
- **api-unit-FR.md** — API PeasyPilot.Unit
