# Référence API PeasyPilot.XUnit

Référence API complète pour le package adaptateur `PeasyPilot.XUnit`.

**Frameworks cibles :** .NET 8, 9, 10  
**Package :** [PeasyPilot.XUnit sur NuGet](https://www.nuget.org/packages/PeasyPilot.XUnit)  
**Dépôt :** [GitHub: PeasyPilot](https://github.com/houssinedev/PeasyPilot)

---

## Table des matières

1. [Vue d'ensemble](#vue-densemble)
2. [Abstractions principales](#abstractions-principales)
3. [Cycle de vie IAsyncLifetime](#cycle-de-vie-iasynclifetime)
4. [Classe et membres clés](#classe-et-membres-clés)
5. [Attributs et modèles](#attributs-et-modèles)
6. [Exemples fonctionnels](#exemples-fonctionnels)
7. [Bonnes pratiques](#bonnes-pratiques)

---

## Vue d'ensemble

L'adaptateur `PeasyPilot.XUnit` intègre xUnit 2.x avec l'infrastructure de test de PeasyPilot. Il fournit :

- **Classe de base native asynchrone** implémentant `IAsyncLifetime`
- **Gestion du contexte de test** pour partager l'état au sein d'un test
- **Mise en commun des fixtures** via les fixtures de collection xUnit
- **Intégration des usines de mock et de données** pour les modèles d'injection de dépendances
- **Intégration transparente** avec les fonctionnalités de PeasyPilot.Core

### Quand choisir xUnit

Utilisez PeasyPilot.XUnit si :
- Votre projet est moderne (.NET 8+)
- Vous préférez une conception orientée asynchrone
- Vous souhaitez le modèle de découverte et d'exécution de xUnit
- Vous avez besoin d'attributs de test fluides et composables ([Theory], [InlineData], etc.)

### Installation

```bash
dotnet add package PeasyPilot.XUnit
```

---

## Abstractions principales

### PeasyPilotTestBase

**Espace de noms :** `PeasyPilot.XUnit`

Classe de base pour les classes de test xUnit. Implémente `Xunit.IAsyncLifetime` pour l'initialisation et le nettoyage asynchrones.

#### Déclaration

```csharp
public abstract class PeasyPilotTestBase : IAsyncLifetime
{
    protected ITestContext TestContext { get; private set; }
    protected ITestDataFactory? TestDataFactory { get; set; }
    protected IMockFactory? MockFactory { get; set; }
    
    public virtual Task InitializeAsync();
    public virtual Task DisposeAsync();
    
    protected T GetOrCreateTestData<T>(string key, Func<T> factory) 
        where T : class;
}
```

#### Membres

| Membre | Type | Objectif |
|--------|------|---------|
| `TestContext` | `ITestContext` | Contexte par test pour la gestion d'état |
| `TestDataFactory` | `ITestDataFactory?` | Usine optionnelle pour la génération de données de test |
| `MockFactory` | `IMockFactory?` | Usine optionnelle pour créer des mocks |
| `InitializeAsync()` | Task | Initialisation asynchrone avant chaque test |
| `DisposeAsync()` | Task | Nettoyage asynchrone après chaque test |
| `GetOrCreateTestData<T>()` | T | Obtenir ou créer des données de test mises en cache par clé |

### XUnitAdapter

**Espace de noms :** `PeasyPilot.XUnit`

Adaptateur implémentant `ITestFrameworkAdapter` pour la découverte et l'exécution de xUnit.

#### Déclaration

```csharp
public sealed class XUnitAdapter : ITestFrameworkAdapter
{
    public string Name { get; }
    public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default);
    public Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default);
}
```

#### Propriétés

| Propriété | Retour | Description |
|-----------|--------|-------------|
| `Name` | string | Retourne toujours `"xUnit"` |

#### Méthodes

| Méthode | Retour | Objectif |
|---------|--------|---------|
| `DiscoverAsync(CancellationToken)` | `Task<IReadOnlyCollection<TestCase>>` | Découvrir les cas de test dans les assemblies xUnit |
| `RunAsync(TestRunRequest, CancellationToken)` | `Task<TestRunResult>` | Exécuter des tests avec résultats détaillés |

### PeasyPilotCollection

**Espace de noms :** `PeasyPilot.XUnit`

Définition de collection pour partager les fixtures `PeasyPilotTestBase` entre les classes de test.

#### Déclaration

```csharp
[CollectionDefinition("PeasyPilot Collection")]
public class PeasyPilotCollection : ICollectionFixture<PeasyPilotTestBase>
{
    // Classe marqueur - aucune implémentation nécessaire
}
```

#### Utilisation

Affectez les classes de test à la collection à l'aide de l'attribut `[Collection]` :

```csharp
[Collection("PeasyPilot Collection")]
public class MyTests : PeasyPilotTestBase
{
    // Tous les tests de cette classe partagent la fixture PeasyPilotTestBase
}
```

---

## Cycle de vie IAsyncLifetime

`IAsyncLifetime` de xUnit fournit des hooks d'initialisation et de disposition asynchrones.

### Ordre d'exécution

```
Pour chaque test :
  1. Fixture instantiée
  2. InitializeAsync() appelé (attente terminée)
  3. Méthode de test [Fact] ou [Theory] exécutée
  4. DisposeAsync() appelé (attente terminée)
  5. Fixture disposée
```

### Exemple : Hooks du cycle de vie

```csharp
public class LifecycleTests : PeasyPilotTestBase
{
    private Database _db = null!;
    
    // Appelé avant chaque test
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _db = new Database();
        await _db.ConnectAsync();
    }
    
    // Appelé après chaque test
    public override async Task DisposeAsync()
    {
        await _db.CloseAsync();
        await base.DisposeAsync();
    }
    
    [Fact]
    public async Task TestUsesDatabase()
    {
        var users = await _db.GetUsersAsync();
        Assert.NotEmpty(users);
    }
}
```

### Points clés

- **Appelez toujours `base.InitializeAsync()`** pour assurer l'initialisation de `TestContext`
- **Utilisez `async/await`** – InitializeAsync/DisposeAsync sont asynchrones
- **Les exceptions dans InitializeAsync** échouent immédiatement le test
- **DisposeAsync s'exécute toujours**, même si le test échoue
- **Utilisez try-finally** pour le nettoyage critique :

```csharp
public override async Task InitializeAsync()
{
    await base.InitializeAsync();
    try
    {
        await _resource.AcquireAsync();
    }
    catch
    {
        await DisposeAsync();
        throw;
    }
}
```

---

## Classe et membres clés

### ITestContext

Gère l'état par test et la mise en cache.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestContext
{
    T GetOrAdd<T>(string key, Func<T> factory) where T : class;
    TValue? Get<TKey, TValue>(TKey key) where TKey : notnull where TValue : class;
    void Set<TKey, TValue>(TKey key, TValue value) where TKey : notnull where TValue : class;
    void Clear();
}
```

### ITestDataFactory

Interface optionnelle pour générer des données de test.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface ITestDataFactory
{
    T Create<T>() where T : class, new();
    T Create<T>(Action<T> configure) where T : class, new();
}
```

### IMockFactory

Interface optionnelle pour créer des mocks.

```csharp
namespace PeasyPilot.Core.Abstractions;

public interface IMockFactory
{
    Mock<T> CreateMock<T>() where T : class;
    Mock<T> CreateMock<T>(MockBehavior behavior) where T : class;
}
```

---

## Attributs et modèles

### Attributs xUnit standard

PeasyPilot.XUnit fonctionne de manière transparente avec les attributs intégrés de xUnit :

#### [Fact]

Marque une méthode de test sans paramètres.

```csharp
[Fact]
public void TestMethod()
{
    Assert.True(true);
}
```

#### [Theory] + [InlineData]

Marque un test paramétrisé avec des données en ligne.

```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(5, 5, 10)]
[InlineData(-1, 1, 0)]
public void Add_WithVariousInputs_ReturnsSum(int a, int b, int expected)
{
    var calc = new Calculator();
    Assert.Equal(expected, calc.Add(a, b));
}
```

#### [Theory] + [MemberData]

Utilisez les données de test des membres de classe.

```csharp
public static IEnumerable<object[]> AddTestData =>
    new List<object[]>
    {
        new object[] { 2, 2, 4 },
        new object[] { 1, 1, 2 },
    };

[Theory]
[MemberData(nameof(AddTestData))]
public void Add_WithMemberData_ReturnsSum(int a, int b, int expected)
{
    var calc = new Calculator();
    Assert.Equal(expected, calc.Add(a, b));
}
```

#### [Trait]

Catégorisez les tests pour le filtrage.

```csharp
[Fact]
[Trait("Category", "Unit")]
[Trait("Speed", "Fast")]
public void QuickTest()
{
    Assert.True(true);
}

// Exécutez : dotnet test --filter "Category=Unit&Speed=Fast"
```

### Fixtures de collection (instances partagées)

Partagez une seule instance de fixture entre plusieurs classes de test.

```csharp
// Définir la fixture
public class DatabaseFixture : IAsyncLifetime
{
    public Database Db { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        Db = new Database();
        await Db.ConnectAsync();
    }
    
    public async Task DisposeAsync()
    {
        await Db.CloseAsync();
    }
}

// Définir la collection
[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}

// Utiliser dans les tests
[Collection("Database Collection")]
public class UserRepositoryTests : PeasyPilotTestBase
{
    private readonly DatabaseFixture _fixture;
    
    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        var user = await _fixture.Db.GetUserAsync(1);
        Assert.NotNull(user);
    }
}
```

---

## Exemples fonctionnels

### Exemple 1 : Test basique avec contexte

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class CalculatorTests : PeasyPilotTestBase
{
    private Calculator _calculator = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _calculator = new Calculator();
    }
    
    [Fact]
    public void Add_WithPositiveNumbers_ReturnsSum()
    {
        int result = _calculator.Add(5, 3);
        Assert.Equal(8, result);
    }
    
    [Fact]
    public void Subtract_WithPositiveNumbers_ReturnsDifference()
    {
        int result = _calculator.Subtract(5, 3);
        Assert.Equal(2, result);
    }
}
```

### Exemple 2 : Theory avec plusieurs ensembles de données

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class MathOperationsTests : PeasyPilotTestBase
{
    private Calculator _calculator = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _calculator = new Calculator();
    }
    
    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 0)]
    [InlineData(100, 50, 150)]
    public void Add_WithVariousInputs_ReturnsCorrectSum(int a, int b, int expected)
    {
        int result = _calculator.Add(a, b);
        Assert.Equal(expected, result);
    }
}
```

### Exemple 3 : Méthode de test asynchrone

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class AsyncOperationTests : PeasyPilotTestBase
{
    private AsyncService _service = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new AsyncService();
        await _service.InitializeAsync();
    }
    
    [Fact]
    public async Task FetchData_WithValidId_ReturnsData()
    {
        var result = await _service.FetchDataAsync(1);
        
        Assert.NotNull(result);
        Assert.Equal("data", result.Name);
    }
}
```

### Exemple 4 : Utilisation du contexte de test pour l'état partagé

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class ContextSharingTests : PeasyPilotTestBase
{
    [Fact]
    public void FirstTest_StoresData()
    {
        var user = new User { Id = 1, Name = "Alice" };
        TestContext.Set("user", user);
        
        var retrieved = TestContext.Get<string, User>("user");
        Assert.NotNull(retrieved);
        Assert.Equal("Alice", retrieved.Name);
    }
    
    [Fact]
    public void GetOrCreate_CachesData()
    {
        var user = TestContext.GetOrAdd(
            "cached_user",
            () => new User { Id = 2, Name = "Bob" }
        );
        
        var second = TestContext.GetOrAdd(
            "cached_user",
            () => new User { Id = 3, Name = "Charlie" }
        );
        
        // Même instance retournée
        Assert.Same(user, second);
    }
}
```

### Exemple 5 : Modèle Factory de données de test

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class FactoryPatternTests : PeasyPilotTestBase
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        TestDataFactory = new UserFactory();
    }
    
    [Fact]
    public void CreateUser_WithFactory_GeneratesValidData()
    {
        var user = TestDataFactory?.Create<User>();
        
        Assert.NotNull(user);
        Assert.NotNull(user.Id);
        Assert.NotEmpty(user.Name);
    }
}

public class UserFactory : ITestDataFactory
{
    public T Create<T>() where T : class, new()
    {
        if (typeof(T) == typeof(User))
            return (new User { Id = Guid.NewGuid(), Name = "TestUser" } as T)!;
        return new T();
    }
    
    public T Create<T>(Action<T> configure) where T : class, new()
    {
        var instance = Create<T>();
        configure(instance);
        return instance;
    }
}
```

### Exemple 6 : Assertion d'exception

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class ExceptionTests : PeasyPilotTestBase
{
    private Calculator _calculator = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _calculator = new Calculator();
    }
    
    [Fact]
    public void Divide_ByZero_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => _calculator.Divide(10, 0));
        Assert.Equal("divisor", ex.ParamName);
    }
    
    [Fact]
    public async Task AsyncOperation_OnError_ThrowsException()
    {
        var service = new FailingService();
        
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.FailAsync()
        );
    }
}
```

### Exemple 7 : Fixture de collection avec base de données partagée

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class DatabaseFixture : IAsyncLifetime
{
    public Database Db { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        Db = new Database("Server=test;Database=testdb");
        await Db.ConnectAsync();
        await Db.InitializeSchemaAsync();
    }
    
    public async Task DisposeAsync()
    {
        await Db.CleanupAsync();
        await Db.DisconnectAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollectionDefinition : ICollectionFixture<DatabaseFixture>
{
}

[Collection("Database")]
public class UserRepositoryTests : PeasyPilotTestBase
{
    private readonly DatabaseFixture _dbFixture;
    
    public UserRepositoryTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }
    
    [Fact]
    public async Task InsertUser_WithValidData_SucceedsAsync()
    {
        var repo = new UserRepository(_dbFixture.Db);
        var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
        
        await repo.InsertAsync(user);
        
        var retrieved = await repo.GetAsync(user.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(user.Name, retrieved.Name);
    }
}
```

### Exemple 8 : Assertions multiples avec traits

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class UserServiceTests : PeasyPilotTestBase
{
    private UserService _service = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new UserService();
    }
    
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Speed", "Slow")]
    public async Task CreateUser_WithValidData_CreatesAndReturnsUser()
    {
        var command = new CreateUserCommand 
        { 
            Email = "test@example.com", 
            Name = "Test User" 
        };
        
        var result = await _service.CreateAsync(command);
        
        Assert.NotNull(result);
        Assert.True(result.Id != Guid.Empty);
        Assert.Equal(command.Email, result.Email);
        Assert.Equal(command.Name, result.Name);
    }
}
```

### Exemple 9 : Attribut Skip pour les tests en attente

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class PendingFeatureTests : PeasyPilotTestBase
{
    [Fact(Skip = "Feature not yet implemented")]
    public void UnfinishedFeature_ShouldBeSkipped()
    {
        // Ce test sera ignoré et signalé séparément
    }
    
    [Theory(Skip = "Awaiting API response")]
    [InlineData(1)]
    [InlineData(2)]
    public void ExternalApiTests_SkipAll(int id)
    {
        // Toutes les variations théoriques ignorées
    }
}
```

### Exemple 10 : Assertions personnalisées avec méthodes d'extension

```csharp
using Xunit;
using PeasyPilot.XUnit;

namespace MyApp.Tests;

public class ExtensionMethodTests : PeasyPilotTestBase
{
    [Fact]
    public void User_WithExtension_Assertions()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" };
        
        user.ShouldBeValid();
        user.Name.ShouldNotBeNullOrEmpty();
        user.Email.ShouldContain("@");
    }
}

public static class UserAssertions
{
    public static void ShouldBeValid(this User user)
    {
        Assert.NotNull(user);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.NotEmpty(user.Name);
    }
    
    public static void ShouldNotBeNullOrEmpty(this string? value)
    {
        Assert.NotNull(value);
        Assert.NotEmpty(value);
    }
}
```

---

## Bonnes pratiques

### 1. Appelez toujours les méthodes de base

```csharp
public override async Task InitializeAsync()
{
    await base.InitializeAsync();  // ✅ Obligatoire
    // Votre initialisation ici
}

public override async Task DisposeAsync()
{
    // Votre nettoyage ici
    await base.DisposeAsync();  // ✅ Obligatoire
}
```

### 2. Utilisez async/await pour les opérations I/O

```csharp
// ✅ BON : Initialisation asynchrone
public override async Task InitializeAsync()
{
    await base.InitializeAsync();
    _db = new Database();
    await _db.ConnectAsync();  // Opération asynchrone
}

// ❌ À ÉVITER : Appels de blocage
public override async Task InitializeAsync()
{
    await base.InitializeAsync();
    _db = new Database();
    _db.Connect();  // L'appel sync bloque le pipeline async
}
```

### 3. Nommez les tests clairement

```csharp
// ✅ BON : Intention claire
[Fact]
public void Add_WithPositiveNumbers_ReturnsSum() { }

// ❌ PAUVRE : Vague
[Fact]
public void TestAdd() { }
```

### 4. Un foyer d'assertion par test

```csharp
// ✅ BON : Ciblé
[Fact]
public void Add_WithTwoNumbers_ReturnsSum()
{
    var result = _calculator.Add(5, 3);
    Assert.Equal(8, result);
}

// ❌ À ÉVITER : Plusieurs comportements
[Fact]
public void Calculator_Works()
{
    Assert.Equal(8, _calculator.Add(5, 3));
    Assert.Equal(2, _calculator.Subtract(5, 3));
    Assert.Throws<Exception>(() => _calculator.Divide(1, 0));
}
```

### 5. Utilisez les fixtures pour les ressources coûteuses

```csharp
// ✅ BON : Fixture partagée pour ressource coûteuse
[Collection("Database")]
public class RepositoryTests : PeasyPilotTestBase
{
    private readonly DatabaseFixture _db;
    public RepositoryTests(DatabaseFixture db) => _db = db;
}

// ❌ À ÉVITER : Créer une nouvelle base de données par test
public class RepositoryTests : PeasyPilotTestBase
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var db = new Database();  // Opération coûteuse par test
        await db.ConnectAsync();
    }
}
```

### 6. Le nettoyage s'exécute toujours

Rappelez-vous que `DisposeAsync()` s'exécute même si le test échoue. Utilisez-le pour le nettoyage critique :

```csharp
public override async Task DisposeAsync()
{
    try
    {
        // Le nettoyage se produit quel que soit le résultat du test
        await _resource.ReleaseAsync();
    }
    finally
    {
        await base.DisposeAsync();
    }
}
```

---

## Voir aussi

- [Documentation xUnit.net](https://xunit.net/)
- [Référence API PeasyPilot.Core](api-core-FR.md)
- [Guide des adaptateurs de framework](../GUIDES/framework-adapters-guide-FR.md)
- [Guide des tests unitaires](../GUIDES/unit-testing-guide-FR.md)

---

**Dernière mise à jour :** 2026-09-11  
**Version :** 1.0  
[← Retour à RÉFÉRENCE](README.md)
