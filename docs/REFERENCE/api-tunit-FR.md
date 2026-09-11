# Référence API PeasyPilot.TUnit

Référence API complète pour le package adaptateur `PeasyPilot.TUnit`.

**Frameworks cibles :** .NET 8, 9, 10  
**Package :** [PeasyPilot.TUnit sur NuGet](https://www.nuget.org/packages/PeasyPilot.TUnit)  
**Dépôt :** [GitHub: PeasyPilot](https://github.com/houssinedev/PeasyPilot)

---

## Table des matières

1. [Vue d'ensemble](#vue-densemble)
2. [Abstractions principales](#abstractions-principales)
3. [Cycle de vie des hooks asynchrones](#cycle-de-vie-des-hooks-asynchrones)
4. [Classes et membres clés](#classes-et-membres-clés)
5. [Attributs et modèles](#attributs-et-modèles)
6. [Exemples fonctionnels](#exemples-fonctionnels)
7. [Bonnes pratiques](#bonnes-pratiques)

---

## Vue d'ensemble

L'adaptateur `PeasyPilot.TUnit` intègre TUnit 1.x avec l'infrastructure de test de PeasyPilot. Il fournit :

- **Support natif async/await** avec les hooks `BeforeEachAsync()` et `AfterEachAsync()`
- **Conception de test moderne** optimisée pour les flux de travail asynchrones
- **Gestion du contexte de test** pour partager l'état au sein d'un test
- **Intégration des usines de mock et de données** pour les modèles d'injection de dépendances
- **Intégration transparente** avec les fonctionnalités de PeasyPilot.Core
- **Exécution de test ultra-rapide** avec les capacités de parallélisation de TUnit
- **Aucun attribut nécessaire** – pur C# avec méthodes hook

### Quand choisir TUnit

Utilisez PeasyPilot.TUnit si :
- Votre projet cible .NET 9+ exclusivement
- Vous souhaitez le framework de test le plus rapide disponible
- Vous préférez les hooks asynchrones sans attributs
- Vos tests sont naturellement asynchrones (appels de base de données, tests d'API)
- Vous avez besoin d'une parallélisation maximale
- Vous voulez zéro passe-partout avec support asynchrone implicite

### Installation

```bash
dotnet add package PeasyPilot.TUnit
```

---

## Abstractions principales

### PeasyPilotTUnitTestBase

**Espace de noms :** `PeasyPilot.TUnit`

Classe de base pour les classes de test TUnit. Fournit les hooks asynchrones `BeforeEachAsync()` et `AfterEachAsync()`.

#### Déclaration

```csharp
public abstract class PeasyPilotTUnitTestBase
{
    protected ITestContext TestContext { get; private set; }
    protected ITestDataFactory? TestDataFactory { get; set; }
    protected IMockFactory? MockFactory { get; set; }
    
    public virtual ValueTask BeforeEachAsync();
    public virtual ValueTask AfterEachAsync();
    
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
| `BeforeEachAsync()` | ValueTask | Initialisation asynchrone avant chaque test |
| `AfterEachAsync()` | ValueTask | Nettoyage asynchrone après chaque test |
| `GetOrCreateTestData<T>()` | T | Obtenir ou créer des données de test mises en cache par clé |

### TUnitAdapter

**Espace de noms :** `PeasyPilot.TUnit`

Adaptateur implémentant `ITestFrameworkAdapter` pour la découverte et l'exécution de TUnit.

#### Déclaration

```csharp
public sealed class TUnitAdapter : ITestFrameworkAdapter
{
    public string Name { get; }
    public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default);
    public Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default);
}
```

#### Propriétés

| Propriété | Retour | Description |
|-----------|--------|-------------|
| `Name` | string | Retourne toujours `"TUnit"` |

#### Méthodes

| Méthode | Retour | Objectif |
|---------|--------|---------|
| `DiscoverAsync(CancellationToken)` | `Task<IReadOnlyCollection<TestCase>>` | Découvrir les cas de test dans les assemblies TUnit |
| `RunAsync(TestRunRequest, CancellationToken)` | `Task<TestRunResult>` | Exécuter des tests avec résultats détaillés |

---

## Cycle de vie des hooks asynchrones

TUnit utilise les méthodes `BeforeEachAsync()` et `AfterEachAsync()` `ValueTask` pour la gestion du cycle de vie des tests asynchrones.

### Ordre d'exécution

```
Pour chaque test :
  1. Instance de classe de test créée
  2. BeforeEachAsync() attendu
  3. Méthode de test exécutée (naturellement asynchrone)
  4. AfterEachAsync() attendu
  5. Instance disposée
```

### Exemple : Cycle de vie asynchrone basique

```csharp
public class LifecycleTests : PeasyPilotTUnitTestBase
{
    private Database _db = null!;
    
    // Appelé avant chaque test (asynchrone)
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _db = new Database();
        await _db.ConnectAsync();
    }
    
    // Appelé après chaque test (asynchrone)
    public override async ValueTask AfterEachAsync()
    {
        await _db.CloseAsync();
        await base.AfterEachAsync();
    }
    
    public async Task TestUsesDatabase()
    {
        var users = await _db.GetUsersAsync();
        Assert.NotEmpty(users);
    }
}
```

### Points clés

- **Appelez toujours `await base.BeforeEachAsync()`** pour assurer l'initialisation de `TestContext`
- **BeforeEachAsync retourne ValueTask** – ultra-efficace pour les scénarios d'allocation zéro
- **AfterEachAsync s'exécute toujours**, même si le test échoue
- **Les tests sont naturellement asynchrones** – signature de méthode `public async Task`
- **Aucun attribut nécessaire** – TUnit découvre les tests par convention
- **Utilisez try-finally pour le nettoyage critique :**

```csharp
public override async ValueTask BeforeEachAsync()
{
    await base.BeforeEachAsync();
    try
    {
        await _resource.AcquireAsync();
    }
    catch
    {
        await AfterEachAsync();
        throw;
    }
}
```

### ValueTask vs Task

TUnit utilise `ValueTask` pour de meilleures performances :

```csharp
// ✅ Style TUnit - ValueTask (pas d'allocation si complété de manière synchrone)
public override async ValueTask BeforeEachAsync()
{
    await base.BeforeEachAsync();
    _data = new Data();
}

// Fonctionne toujours, mais moins efficace - Task
public override async Task BeforeEachAsync()  // Non recommandé pour TUnit
{
    await base.BeforeEachAsync();
    _data = new Data();
}
```

---

## Classes et membres clés

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

### Méthodes de test

TUnit découvre les méthodes de test par convention – méthodes `public async Task` ou `public async ValueTask` :

```csharp
// ✅ Découvert automatiquement
public async Task Add_WithTwoNumbers_ReturnsSum()
{
    var result = await _calculator.AddAsync(5, 3);
    Assert.Equal(8, result);
}

// ✅ Également valide (variante ValueTask)
public async ValueTask Multiply_WithTwoNumbers_ReturnsProduct()
{
    var result = await _calculator.MultiplyAsync(4, 5);
    Assert.Equal(20, result);
}

// ✅ Méthodes asynchrones avec paramètres (tests paramétrés)
public async Task FetchUser_WithVariousIds(int id)
{
    var user = await _userService.GetUserAsync(id);
    Assert.NotNull(user);
}
```

### Tests paramétrés

TUnit découvre automatiquement les données de paramètre :

```csharp
public async Task Add_WithVariousInputs(int a, int b, int expected)
{
    var result = _calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

Pour fournir des données de test, remplacez :

```csharp
public static IEnumerable<object[]> AddTestData =>
    new List<object[]>
    {
        new object[] { 2, 3, 5 },
        new object[] { 0, 0, 0 },
        new object[] { -1, 1, 0 },
    };

[Parameters(nameof(AddTestData))]
public async Task Add_WithMemberData(int a, int b, int expected)
{
    var result = _calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

### Traits et catégories

Utilisez les attributs de méthode pour l'organisation des tests :

```csharp
[Trait("Category", "Unit")]
[Trait("Speed", "Fast")]
public async Task QuickTest()
{
    Assert.True(true);
}

// Exécutez : dotnet test --filter "Category=Unit&Speed=Fast"
```

### Sauter les tests

Ignorez les tests à l'aide de l'attribut `[Skip]` :

```csharp
[Skip("Not yet implemented")]
public async Task UnfinishedFeature()
{
    // Ignoré
}
```

---

## Exemples fonctionnels

### Exemple 1 : Test asynchrone basique

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class CalculatorTests : PeasyPilotTUnitTestBase
{
    private Calculator _calculator = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _calculator = new Calculator();
    }
    
    public async Task Add_WithPositiveNumbers_ReturnsSum()
    {
        int result = _calculator.Add(5, 3);
        await Assert.That(result).IsEqualTo(8);
    }
    
    public async Task Subtract_WithPositiveNumbers_ReturnsDifference()
    {
        int result = _calculator.Subtract(5, 3);
        await Assert.That(result).IsEqualTo(2);
    }
}
```

### Exemple 2 : Tests paramétrés

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class MathOperationsTests : PeasyPilotTUnitTestBase
{
    private Calculator _calculator = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _calculator = new Calculator();
    }
    
    public static IEnumerable<object[]> AddTestData =>
        new[]
        {
            new object[] { 2, 3, 5 },
            new object[] { 0, 0, 0 },
            new object[] { -1, 1, 0 },
            new object[] { 100, 50, 150 },
        };
    
    [Parameters(nameof(AddTestData))]
    public async Task Add_WithVariousInputs_ReturnsCorrectSum(int a, int b, int expected)
    {
        int result = _calculator.Add(a, b);
        await Assert.That(result).IsEqualTo(expected);
    }
}
```

### Exemple 3 : Opérations asynchrones avec await

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class AsyncOperationTests : PeasyPilotTUnitTestBase
{
    private AsyncService _service = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _service = new AsyncService();
        await _service.InitializeAsync();
    }
    
    public async Task FetchData_WithValidId_ReturnsData()
    {
        var result = await _service.FetchDataAsync(1);
        
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Name).IsEqualTo("data");
    }
    
    public async Task FetchMultipleUsers_WithIds_ReturnsAll()
    {
        var results = await _service.FetchUsersAsync(new[] { 1, 2, 3 });
        
        await Assert.That(results).HasCount(3);
        await Assert.That(results).AllSatisfy(r => Assert.That(r).IsNotNull());
    }
}
```

### Exemple 4 : Utilisation du contexte de test

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class ContextSharingTests : PeasyPilotTUnitTestBase
{
    public async Task StoreData_InContext()
    {
        var user = new User { Id = 1, Name = "Alice" };
        TestContext.Set("user", user);
        
        var retrieved = TestContext.Get<string, User>("user");
        await Assert.That(retrieved).IsNotNull();
        await Assert.That(retrieved.Name).IsEqualTo("Alice");
    }
    
    public async Task GetOrCreate_CachesData()
    {
        var user = TestContext.GetOrAdd(
            "cached_user",
            () => new User { Id = 2, Name = "Bob" }
        );
        
        var second = TestContext.GetOrAdd(
            "cached_user",
            () => new User { Id = 3, Name = "Charlie" }
        );
        
        await Assert.That(user).IsTheSameAs(second);
    }
}
```

### Exemple 5 : Test de base de données avec cycle de vie asynchrone

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class DatabaseTests : PeasyPilotTUnitTestBase
{
    private Database _db = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _db = new Database("Server=test;Database=testdb");
        await _db.ConnectAsync();
        await _db.InitializeSchemaAsync();
    }
    
    public override async ValueTask AfterEachAsync()
    {
        await _db.CleanupAsync();
        await _db.DisconnectAsync();
        await base.AfterEachAsync();
    }
    
    public async Task InsertUser_WithValidData_Succeeds()
    {
        var repo = new UserRepository(_db);
        var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
        
        await repo.InsertAsync(user);
        
        var retrieved = await repo.GetAsync(user.Id);
        await Assert.That(retrieved).IsNotNull();
        await Assert.That(retrieved.Name).IsEqualTo(user.Name);
    }
    
    public async Task QueryUsers_WithFilters_ReturnsMatching()
    {
        var repo = new UserRepository(_db);
        await repo.InsertAsync(new User { Id = Guid.NewGuid(), Name = "Alice" });
        await repo.InsertAsync(new User { Id = Guid.NewGuid(), Name = "Bob" });
        
        var results = await repo.QueryAsync(u => u.Name.StartsWith("A"));
        
        await Assert.That(results).HasCount(1);
        await Assert.That(results[0].Name).IsEqualTo("Alice");
    }
}
```

### Exemple 6 : Test d'exception

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class ExceptionTests : PeasyPilotTUnitTestBase
{
    private Calculator _calculator = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _calculator = new Calculator();
    }
    
    public async Task Divide_ByZero_ThrowsArgumentException()
    {
        var ex = await Assert.That(
            () => _calculator.Divide(10, 0)
        ).Throws<ArgumentException>();
        
        await Assert.That(ex.ParamName).IsEqualTo("divisor");
    }
    
    public async Task AsyncOperation_OnError_ThrowsException()
    {
        var service = new FailingService();
        
        await Assert.That(
            () => service.FailAsync()
        ).ThrowsAsync<InvalidOperationException>();
    }
}
```

### Exemple 7 : Factory de données de test avec async

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class FactoryPatternTests : PeasyPilotTUnitTestBase
{
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        TestDataFactory = new UserFactory();
    }
    
    public async Task CreateUser_WithFactory_GeneratesValidData()
    {
        var user = TestDataFactory?.Create<User>();
        
        await Assert.That(user).IsNotNull();
        await Assert.That(user.Id).IsNotNull();
        await Assert.That(user.Name).IsNotEmpty();
    }
    
    public async Task CreateMultipleUsers_WithFactory()
    {
        var users = Enumerable.Range(0, 5)
            .Select(_ => TestDataFactory?.Create<User>())
            .ToList();
        
        await Assert.That(users).HasCount(5);
        await Assert.That(users).AllSatisfy(u => Assert.That(u).IsNotNull());
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

### Exemple 8 : Exécution parallèle des tests

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class ParallelTests : PeasyPilotTUnitTestBase
{
    // TUnit exécute ces tests en parallèle par défaut
    
    public async Task Test_One()
    {
        await Task.Delay(100);
        await Assert.That(true).IsTrue();
    }
    
    public async Task Test_Two()
    {
        await Task.Delay(100);
        await Assert.That(true).IsTrue();
    }
    
    public async Task Test_Three()
    {
        await Task.Delay(100);
        await Assert.That(true).IsTrue();
    }
    
    // Les trois s'exécutent de manière concomitante, temps total ~100ms au lieu de ~300ms
}
```

### Exemple 9 : Chaîne d'assertions asynchrones

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class ChainedAssertionsTests : PeasyPilotTUnitTestBase
{
    private UserService _service = null!;
    
    public override async ValueTask BeforeEachAsync()
    {
        await base.BeforeEachAsync();
        _service = new UserService();
    }
    
    public async Task CreateUser_WithValidData_CreatesAndReturnsUser()
    {
        var command = new CreateUserCommand 
        { 
            Email = "test@example.com", 
            Name = "Test User" 
        };
        
        var result = await _service.CreateAsync(command);
        
        await Assert.That(result)
            .IsNotNull()
            .And(r => r.Id, id => id.IsNotEqualTo(Guid.Empty))
            .And(r => r.Email, email => email.IsEqualTo(command.Email))
            .And(r => r.Name, name => name.IsEqualTo(command.Name));
    }
}
```

### Exemple 10 : Sauter les tests de manière conditionnelle

```csharp
using TUnit.Assertions;
using PeasyPilot.TUnit;

namespace MyApp.Tests;

public class ConditionalSkipTests : PeasyPilotTUnitTestBase
{
    [Skip("Feature not yet implemented")]
    public async Task UnfinishedFeature_ShouldBeSkipped()
    {
        // Ignoré avec raison
    }
    
    public async Task SkipIf_Condition()
    {
        if (Environment.ProcessorCount < 4)
        {
            Assert.Skip("Test requires at least 4 cores");
        }
        
        // Logique de test
        await Assert.That(true).IsTrue();
    }
}
```

---

## Bonnes pratiques

### 1. Attendez toujours BeforeEachAsync/AfterEachAsync

```csharp
public override async ValueTask BeforeEachAsync()
{
    await base.BeforeEachAsync();  // ✅ Obligatoire
    // Votre initialisation ici
}

public override async ValueTask AfterEachAsync()
{
    // Votre nettoyage ici
    await base.AfterEachAsync();  // ✅ Obligatoire
}
```

### 2. Préférez ValueTask pour les opérations légères

```csharp
// ✅ BON : ValueTask pour les scénarios d'allocation zéro
public override async ValueTask BeforeEachAsync()
{
    await base.BeforeEachAsync();
    _data = new Data();
}

// Toujours valide mais moins optimal :
public override async Task BeforeEachAsync()
{
    await base.BeforeEachAsync();
    _data = new Data();
}
```

### 3. Nommez les tests clairement

```csharp
// ✅ BON : Intention claire
public async Task Add_WithPositiveNumbers_ReturnsSum() { }

// ❌ PAUVRE : Vague
public async Task TestAdd() { }
```

### 4. Adoptez la conception asynchrone d'abord

```csharp
// ✅ BON : Asynchrone d'abord
public async Task FetchData_WithValidId_ReturnsData()
{
    var result = await _service.FetchDataAsync(id);
    await Assert.That(result).IsNotNull();
}

// ❌ À ÉVITER : Mélanger les opérations synchrones
public async Task FetchData_WithValidId_ReturnsData()
{
    var result = _service.FetchData(id);  // L'appel sync bloque
    await Assert.That(result).IsNotNull();
}
```

### 5. Exploitez la parallélisation

TUnit exécute les tests en parallèle par défaut – écrivez des tests indépendants :

```csharp
// ✅ BON : Tests indépendants (s'exécutent en parallèle)
public async Task Test_One() { }
public async Task Test_Two() { }
public async Task Test_Three() { }

// ❌ À ÉVITER : État mutable partagé
private int _counter = 0;

public async Task Increment_Test()
{
    _counter++;  // Condition de course
}
```

### 6. Utilisez Try-Finally pour le nettoyage critique

```csharp
public override async ValueTask BeforeEachAsync()
{
    await base.BeforeEachAsync();
    try
    {
        await _resource.AcquireAsync();
    }
    catch
    {
        await AfterEachAsync();
        throw;
    }
}
```

### 7. Gardez les tests ciblés

```csharp
// ✅ BON : Un comportement par test
public async Task Add_WithPositiveNumbers_ReturnsSum()
{
    var result = _calculator.Add(5, 3);
    await Assert.That(result).IsEqualTo(8);
}

// ❌ À ÉVITER : Plusieurs comportements
public async Task Calculator_Works()
{
    var add = _calculator.Add(5, 3);
    var sub = _calculator.Subtract(5, 3);
    var mul = _calculator.Multiply(4, 5);
    // Teste plusieurs comportements
}
```

---

## Voir aussi

- [Documentation TUnit](https://thomhurst.github.io/tunit/)
- [Référence API PeasyPilot.Core](api-core-FR.md)
- [Guide des adaptateurs de framework](../GUIDES/framework-adapters-guide-FR.md)
- [Guide des tests unitaires](../GUIDES/unit-testing-guide-FR.md)

---

**Dernière mise à jour :** 2026-09-11  
**Version :** 1.0  
[← Retour à RÉFÉRENCE](README.md)
