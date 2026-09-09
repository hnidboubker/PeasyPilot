# PeasyPilot Integration Testing — Guide Complet

Infrastructure unifié pour écrire des tests d'intégration avec les applications ASP.NET Core et les bases de données en mémoire.

---

## Table des matières

1. [Vue d'ensemble](#vue-densemble)
2. [Composants principaux](#composants-principaux)
3. [Fixture de test d'intégration](#fixture-de-test-dintégration)
4. [Base de données en mémoire](#base-de-données-en-mémoire)
5. [Tests ASP.NET Core](#tests-aspnet-core)
6. [Guides pratiques](#guides-pratiques)
7. [Bonnes pratiques](#bonnes-pratiques)

---

## Vue d'ensemble

**PeasyPilot.Integration** fournit :

- ✅ IntegrationTestFixture — Fixture de base avec DI
- ✅ InMemoryTestDatabase — Base de données en mémoire rapide
- ✅ WebApplicationTestFactory — Factory ASP.NET Core
- ✅ HttpTestClient — Helpers de test API
- ✅ IResettable — Interface de réinitialisation

### Pipeline d'intégration

```
Test démarre
    ↓
InitializeAsync()
    ↓
ServiceProvider créé
    ↓
Base de données initialisée
    ↓
Test s'exécute
    ↓
DisposeAsync()
    ↓
Base de données nettoyée
    ↓
ServiceProvider disposé
```

---

## Composants principaux

### 1. IntegrationTestFixture — Fondation

Classe de base pour tous les tests d'intégration avec support DI et gestion du cycle de vie.

```csharp
public class MyIntegrationTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Enregistrer vos dépendances de test
        services.AddScoped<IMyService, TestMyService>();
        services.AddSingleton<ILogger, NullLogger>();
    }

    [Fact]
    public async Task MyTest()
    {
        var service = GetService<IMyService>();
        var result = await service.DoSomethingAsync();
        Assert.NotNull(result);
    }
}
```

**Cycle de vie :**
- `InitializeAsync()` — Avant chaque test
  - Crée ServiceProvider
  - Initialise la base de données
  - Alimente avec données de test
- `DisposeAsync()` — Après chaque test
  - Nettoie la base de données
  - Dispose ServiceProvider

**Méthodes disponibles :**
```csharp
GetService<T>()              // Résoudre une dépendance
ResetDatabaseAsync()         // Réinitialiser la BD
RegisterResettableService()  // Enregistrer un service réinitialisable
Database                     // Accéder à ITestDatabase
Services                     // Accéder à ServiceProvider
```

### 2. InMemoryTestDatabase — Tests rapides

Implémentation en mémoire pour les tests d'intégration rapides.

```csharp
protected override ITestDatabaseFactory CreateDatabaseFactory()
{
    return new InMemoryDatabaseFactory();  // Défaut
}
```

**Méthodes de cycle de vie :**
- `InitializeAsync()` — Configuration de la BD
- `SeedAsync()` — Remplir avec données de test
- `ResetAsync()` — Effacer les données entre les tests
- `CleanupAsync()` — Nettoyage final

**Exemple avec seed :**

```csharp
public class UserRepositoryTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        var repository = new InMemoryUserRepository();
        services.AddSingleton<IUserRepository>(repository);
        RegisterResettableService(repository);
    }

    [Fact]
    public async Task GetUser_WithExistingId_ReturnsUser()
    {
        var repository = GetService<IUserRepository>();
        
        var user = await repository.GetByIdAsync(1);
        
        Assert.NotNull(user);
        Assert.Equal("John", user.Name);
    }
}
```

### 3. WebApplicationTestFactory — ASP.NET Core

Factory de test pour les applications ASP.NET Core avec configuration fluide.

```csharp
var factory = new WebApplicationTestFactory<Program>()
    .WithServices(services =>
    {
        // Remplacer les services pour les tests
        services.AddScoped<IUserRepository, MockUserRepository>();
    })
    .WithApp(app =>
    {
        app.UseRouting();
        app.UseEndpoints(e => e.MapControllers());
    });

var client = factory.CreateHttpTestClient();
var response = await client.GetJsonAsync<User>("/api/users/1");
```

**Méthodes de configuration :**
```csharp
WithServices(Action<IServiceCollection>)     // Remplacer dépendances
WithApp(Action<IApplicationBuilder>)         // Configurer middleware
WithWebHostBuilder(Action<IWebHostBuilder>)  // Configuration avancée
CreateTestClient()                           // HttpClient brut
CreateHttpTestClient()                       // HttpTestClient avec helpers
```

### 4. HttpTestClient — Helpers API

Wrapper autour d'HttpClient avec support JSON.

```csharp
var client = factory.CreateHttpTestClient();

// GET avec désérialisation automatique
var user = await client.GetJsonAsync<User>("/api/users/1");

// POST avec sérialisation automatique
var newUser = new User { Name = "Alice" };
var response = await client.PostJsonAsync("/api/users", newUser);

// PUT
await client.PutJsonAsync("/api/users/1", updatedUser);

// DELETE
await client.DeleteAsync("/api/users/1");

// Accéder au HttpResponseMessage
var httpResponse = response.HttpResponse;
Assert.Equal(System.Net.HttpStatusCode.OK, httpResponse.StatusCode);
```

---

## Fixture de test d'intégration

### Exemple complet : UserRepository

```csharp
using PeasyPilot.Integration.Fixtures;
using Xunit;

public class UserRepositoryIntegrationTests : XUnitIntegrationTestFixture
{
    // Configuration du conteneur DI
    protected override void ConfigureServices(IServiceCollection services)
    {
        var repository = new InMemoryUserRepository();
        services.AddSingleton<IUserRepository>(repository);
        
        // Enregistrer pour réinitialisation automatique
        RegisterResettableService(repository);
    }

    [Fact]
    public async Task AddUser_WithNewUser_Succeeds()
    {
        // Arrange
        var repository = GetService<IUserRepository>();
        var user = new User { Name = "Alice", Email = "alice@example.com" };

        // Act
        await repository.AddAsync(user);

        // Assert
        var users = await repository.GetAllAsync();
        Assert.Single(users);
        Assert.Equal("Alice", users[0].Name);
    }

    [Fact]
    public async Task ResetDatabase_ClearsAllData()
    {
        var repository = GetService<IUserRepository>();
        await repository.AddAsync(new User { Name = "Bob" });

        // Réinitialiser
        await ResetDatabaseAsync();

        // Vérifier
        var users = await repository.GetAllAsync();
        Assert.Empty(users);
    }
}
```

---

## Base de données en mémoire

### Créer un repository en mémoire

```csharp
public class InMemoryUserRepository : IUserRepository, IResettable
{
    private readonly List<User> _users = new();

    public Task AddAsync(User user)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public Task<List<User>> GetAllAsync()
    {
        return Task.FromResult(new List<User>(_users));
    }

    // Implémentation de IResettable
    public async Task ResetAsync()
    {
        _users.Clear();
        await Task.CompletedTask;
    }
}
```

### Utiliser avec ResetDatabaseAsync

```csharp
[Fact]
public async Task Test1_AddsUser()
{
    var repo = GetService<IUserRepository>();
    await repo.AddAsync(new User { Name = "Alice" });
    var all = await repo.GetAllAsync();
    Assert.Single(all);
}

[Fact]
public async Task Test2_StartsEmpty()
{
    // Test1 a été réinitialisé automatiquement
    var repo = GetService<IUserRepository>();
    var all = await repo.GetAllAsync();
    Assert.Empty(all);
}
```

---

## Tests ASP.NET Core

### Exemple : Tests API

```csharp
public class UserApiTests : XUnitIntegrationTestFixture
{
    private WebApplicationTestFactory<Program> _factory = null!;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, InMemoryUserRepository>();
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyList()
    {
        var client = _factory.CreateHttpTestClient();
        
        var users = await client.GetJsonAsync<List<User>>("/api/users");
        
        Assert.NotNull(users);
        Assert.Empty(users);
    }

    [Fact]
    public async Task CreateUser_WithValidData_Returns201()
    {
        var client = _factory.CreateHttpTestClient();
        var newUser = new { Name = "Charlie", Email = "charlie@example.com" };

        var response = await client.PostJsonAsync("/api/users", newUser);

        Assert.Equal(System.Net.HttpStatusCode.Created, 
            response.HttpResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WithValidId_Returns200()
    {
        var client = _factory.CreateHttpTestClient();
        var update = new { Name = "Updated Name" };

        var response = await client.PutJsonAsync("/api/users/1", update);

        Assert.Equal(System.Net.HttpStatusCode.OK, 
            response.HttpResponse.StatusCode);
    }
}
```

---

## Guides pratiques

### How To : Configurer une fixture simple

```csharp
public class MyTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Ajouter vos services de test
        services.AddScoped<IMyService, TestMyService>();
    }

    [Fact]
    public async Task MyTest()
    {
        var service = GetService<IMyService>();
        var result = await service.DoWorkAsync();
        Assert.True(result);
    }
}
```

### How To : Tester avec une API

```csharp
public class ApiTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task CreateResource_Success()
    {
        var client = _factory.CreateHttpTestClient();
        var data = new { Title = "New Item" };

        var result = await client.PostJsonAsync<ItemResponse>("/api/items", data);

        Assert.NotNull(result);
        Assert.Equal("New Item", result.Title);
    }
}
```

### How To : Réinitialiser entre les tests

```csharp
protected override void ConfigureServices(IServiceCollection services)
{
    var repo = new MyResettableRepository();
    services.AddSingleton(repo);
    RegisterResettableService(repo);  // ← Important !
}

// ResetDatabaseAsync() sera appelé automatiquement après chaque test
```

---

## Bonnes pratiques

### ✅ À faire

- Utiliser InMemoryTestDatabase pour les tests rapides
- Enregistrer les services réinitialisables avec `RegisterResettableService()`
- Utiliser `GetService<T>()` pour accéder aux dépendances
- Utiliser async/await pour toutes les opérations DB
- Tester le comportement métier, pas l'implémentation

### ❌ À éviter

- Créer plusieurs instances de la fixture dans un test
- Ne pas réinitialiser l'état entre les tests
- Utiliser une vraie base de données en mémoire (lente)
- Dépendre de l'ordre d'exécution des tests
- Tester trop de choses en un seul test

---

## Résumé

```
1️⃣  Créer une fixture (hériter de XUnitIntegrationTestFixture)
2️⃣  Configurer les services
3️⃣  Enregistrer les services réinitialisables
4️⃣  Écrire les tests
5️⃣  Utiliser GetService<T>() et ResetDatabaseAsync()
```

Tests d'intégration = vérifier que tous les composants fonctionnent ensemble correctement. 🎯
