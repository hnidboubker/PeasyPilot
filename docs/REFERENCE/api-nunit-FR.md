# Référence API PeasyPilot.NUnit

Référence API complète pour le package adaptateur `PeasyPilot.NUnit`.

**Frameworks cibles :** .NET 8, 9, 10  
**Package :** [PeasyPilot.NUnit sur NuGet](https://www.nuget.org/packages/PeasyPilot.NUnit)  
**Dépôt :** [GitHub: PeasyPilot](https://github.com/houssinedev/PeasyPilot)

---

## Table des matières

1. [Vue d'ensemble](#vue-densemble)
2. [Abstractions principales](#abstractions-principales)
3. [Cycle de vie Setup/Teardown](#cycle-de-vie-setupteardown)
4. [Classes et membres clés](#classes-et-membres-clés)
5. [Attributs et modèles](#attributs-et-modèles)
6. [Exemples fonctionnels](#exemples-fonctionnels)
7. [Bonnes pratiques](#bonnes-pratiques)

---

## Vue d'ensemble

L'adaptateur `PeasyPilot.NUnit` intègre NUnit 3.x avec l'infrastructure de test de PeasyPilot. Il fournit :

- **Support synchrone et asynchrone** avec les attributs `[SetUp]` et `[TearDown]`
- **Gestion du contexte de test** pour partager l'état au sein d'un test
- **Modèle TestFixture** pour organiser les tests associés
- **Intégration des usines de mock et de données** pour les modèles d'injection de dépendances
- **Intégration transparente** avec les fonctionnalités de PeasyPilot.Core
- **Approche conviviale pour les entreprises** familière aux utilisateurs de NUnit

### Quand choisir NUnit

Utilisez PeasyPilot.NUnit si :
- Votre projet utilise NUnit 3.x
- Vous avez une équipe familière avec les conventions de NUnit
- Vous travaillez avec des projets hérités ou d'entreprise
- Vous avez besoin de support de test synchrone et asynchrone
- Vous préférez les hooks de cycle de vie explicites [SetUp] et [TearDown]

### Installation

```bash
dotnet add package PeasyPilot.NUnit
```

---

## Abstractions principales

### PeasyPilotNUnitTestBase

**Espace de noms :** `PeasyPilot.NUnit`

Classe de base pour les classes de test NUnit. Fournit les hooks de cycle de vie `[SetUp]` et `[TearDown]`.

#### Déclaration

```csharp
public abstract class PeasyPilotNUnitTestBase
{
    protected ITestContext TestContext { get; private set; }
    protected ITestDataFactory? TestDataFactory { get; set; }
    protected IMockFactory? MockFactory { get; set; }
    
    [SetUp]
    public virtual void Setup();
    
    [TearDown]
    public virtual void TearDown();
    
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
| `Setup()` | void | Configuration synchrone avant chaque test |
| `TearDown()` | void | Nettoyage synchrone après chaque test |
| `GetOrCreateTestData<T>()` | T | Obtenir ou créer des données de test mises en cache par clé |

### NUnitAdapter

**Espace de noms :** `PeasyPilot.NUnit`

Adaptateur implémentant `ITestFrameworkAdapter` pour la découverte et l'exécution de NUnit.

#### Déclaration

```csharp
public sealed class NUnitAdapter : ITestFrameworkAdapter
{
    public string Name { get; }
    public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default);
    public Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default);
}
```

#### Propriétés

| Propriété | Retour | Description |
|-----------|--------|-------------|
| `Name` | string | Retourne toujours `"NUnit"` |

#### Méthodes

| Méthode | Retour | Objectif |
|---------|--------|---------|
| `DiscoverAsync(CancellationToken)` | `Task<IReadOnlyCollection<TestCase>>` | Découvrir les cas de test dans les assemblies NUnit |
| `RunAsync(TestRunRequest, CancellationToken)` | `Task<TestRunResult>` | Exécuter des tests avec résultats détaillés |

---

## Cycle de vie Setup/Teardown

NUnit utilise les attributs `[SetUp]` et `[TearDown]` pour la gestion du cycle de vie des tests.

### Ordre d'exécution

```
Pour chaque test :
  1. Classe de test instantiée
  2. Tous les méthodes [OneTimeSetUp] s'exécutent (le cas échéant)
  3. Méthode [SetUp] appelée
  4. Méthode [Test] ou [TestCase] exécutée
  5. Méthode [TearDown] appelée
  6. (Après tous les tests : [OneTimeTearDown])
```

### Exemple : Cycle de vie basique

```csharp
[TestFixture]
public class LifecycleTests : PeasyPilotNUnitTestBase
{
    private Calculator _calculator = null!;
    
    // Appelé avant chaque test
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _calculator = new Calculator();
    }
    
    // Appelé après chaque test
    [TearDown]
    public override void TearDown()
    {
        // Nettoyage si nécessaire
        base.TearDown();
    }
    
    [Test]
    public void Add_WithTwoNumbers_ReturnsSum()
    {
        var result = _calculator.Add(5, 3);
        Assert.That(result, Is.EqualTo(8));
    }
}
```

### Points clés

- **Appelez toujours `base.Setup()`** pour assurer l'initialisation de `TestContext`
- **[SetUp] s'exécute avant chaque test** – parfait pour l'initialisation par test
- **[TearDown] s'exécute après chaque test** – même si le test échoue
- **[OneTimeSetUp]** s'exécute une fois par classe de fixture de test (pas par test)
- **[OneTimeTearDown]** s'exécute une fois après tous les tests de la fixture
- **Utilisez try-finally** pour le nettoyage critique :

```csharp
[TearDown]
public override void TearDown()
{
    try
    {
        // Nettoyage critique
        _resource?.Dispose();
    }
    finally
    {
        base.TearDown();
    }
}
```

### Exemple OneTimeSetUp

```csharp
[TestFixture]
public class OneTimeSetupExample : PeasyPilotNUnitTestBase
{
    private static Database _sharedDb = null!;
    
    // S'exécute une fois avant tous les tests de cette fixture
    [OneTimeSetUp]
    public static void OneTimeSetup()
    {
        _sharedDb = new Database();
        _sharedDb.Connect();
        _sharedDb.InitializeSchema();
    }
    
    // S'exécute avant chaque test
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        // Effacer les données avant chaque test
        _sharedDb.ClearAllTables();
    }
    
    // S'exécute une fois après tous les tests de cette fixture
    [OneTimeTearDown]
    public static void OneTimeTearDown()
    {
        _sharedDb?.Disconnect();
    }
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

### Attributs NUnit standard

PeasyPilot.NUnit fonctionne de manière transparente avec les attributs intégrés de NUnit :

#### [TestFixture]

Marque une classe comme contenant des tests.

```csharp
[TestFixture]
public class CalculatorTests : PeasyPilotNUnitTestBase
{
    // Tests ici
}
```

#### [Test]

Marque une méthode de test sans paramètres.

```csharp
[Test]
public void Add_WithTwoNumbers_ReturnsSum()
{
    Assert.That(_calculator.Add(5, 3), Is.EqualTo(8));
}
```

#### [TestCase] avec plusieurs ensembles de données

Marque un test paramétrisé.

```csharp
[TestCase(1, 2, 3)]
[TestCase(5, 5, 10)]
[TestCase(-1, 1, 0)]
public void Add_WithVariousInputs_ReturnsSum(int a, int b, int expected)
{
    var calc = new Calculator();
    Assert.That(calc.Add(a, b), Is.EqualTo(expected));
}
```

#### [Category]

Catégorisez les tests pour le filtrage.

```csharp
[Test]
[Category("Unit")]
[Category("Fast")]
public void QuickTest()
{
    Assert.That(true, Is.True);
}

// Exécutez : dotnet test --filter "Category=Unit&Category=Fast"
```

### Modèles d'assertion

Syntaxe d'assertion fluide de NUnit :

```csharp
// Égalité
Assert.That(result, Is.EqualTo(8));

// Vérifications nulles
Assert.That(obj, Is.Null);
Assert.That(obj, Is.Not.Null);

// Collections
Assert.That(list, Is.Empty);
Assert.That(list, Is.Not.Empty);
Assert.That(list, Contains.Item(5));

// Chaînes
Assert.That(text, Does.Contain("hello"));
Assert.That(text, Does.StartWith("the"));
Assert.That(text, Does.EndWith("end"));

// Nombres
Assert.That(value, Is.GreaterThan(10));
Assert.That(value, Is.LessThan(100));
Assert.That(value, Is.InRange(1, 10));

// Vérifications de type
Assert.That(obj, Is.TypeOf<User>());
Assert.That(obj, Is.InstanceOf<IRepository>());
```

### Assertions multiples

```csharp
[Test]
public void User_WithValidData_HasRequiredProperties()
{
    var user = new User { Id = 1, Name = "Alice" };
    
    Assert.Multiple(() =>
    {
        Assert.That(user.Id, Is.Not.EqualTo(0));
        Assert.That(user.Name, Is.Not.Null);
        Assert.That(user.Name, Does.StartWith("A"));
    });
}
```

---

## Exemples fonctionnels

### Exemple 1 : TestFixture basique

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class CalculatorTests : PeasyPilotNUnitTestBase
{
    private Calculator _calculator = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _calculator = new Calculator();
    }
    
    [Test]
    public void Add_WithPositiveNumbers_ReturnsSum()
    {
        int result = _calculator.Add(5, 3);
        Assert.That(result, Is.EqualTo(8));
    }
    
    [Test]
    public void Subtract_WithPositiveNumbers_ReturnsDifference()
    {
        int result = _calculator.Subtract(5, 3);
        Assert.That(result, Is.EqualTo(2));
    }
}
```

### Exemple 2 : TestCase avec plusieurs ensembles de données

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class MathOperationsTests : PeasyPilotNUnitTestBase
{
    private Calculator _calculator = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _calculator = new Calculator();
    }
    
    [TestCase(2, 3, 5)]
    [TestCase(0, 0, 0)]
    [TestCase(-1, 1, 0)]
    [TestCase(100, 50, 150)]
    public void Add_WithVariousInputs_ReturnsCorrectSum(int a, int b, int expected)
    {
        int result = _calculator.Add(a, b);
        Assert.That(result, Is.EqualTo(expected));
    }
}
```

### Exemple 3 : Utilisation du contexte de test pour l'état partagé

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class ContextSharingTests : PeasyPilotNUnitTestBase
{
    [Test]
    public void FirstTest_StoresData()
    {
        var user = new User { Id = 1, Name = "Alice" };
        TestContext.Set("user", user);
        
        var retrieved = TestContext.Get<string, User>("user");
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved.Name, Is.EqualTo("Alice"));
    }
    
    [Test]
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
        Assert.That(user, Is.SameAs(second));
    }
}
```

### Exemple 4 : Modèle Factory de données de test

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class FactoryPatternTests : PeasyPilotNUnitTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        TestDataFactory = new UserFactory();
    }
    
    [Test]
    public void CreateUser_WithFactory_GeneratesValidData()
    {
        var user = TestDataFactory?.Create<User>();
        
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Id, Is.Not.Null);
        Assert.That(user.Name, Is.Not.Empty);
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

### Exemple 5 : Tests d'exception

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class ExceptionTests : PeasyPilotNUnitTestBase
{
    private Calculator _calculator = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _calculator = new Calculator();
    }
    
    [Test]
    public void Divide_ByZero_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => _calculator.Divide(10, 0));
        Assert.That(ex.ParamName, Is.EqualTo("divisor"));
    }
    
    [Test]
    public void NullArgument_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _calculator.Multiply(null));
    }
}
```

### Exemple 6 : OneTimeSetUp et OneTimeTearDown

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class DatabaseTests : PeasyPilotNUnitTestBase
{
    private static Database _db = null!;
    
    [OneTimeSetUp]
    public static void SetupDatabase()
    {
        _db = new Database("Server=test;Database=testdb");
        _db.Connect();
        _db.InitializeSchema();
    }
    
    [OneTimeTearDown]
    public static void TeardownDatabase()
    {
        _db?.Cleanup();
        _db?.Disconnect();
    }
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _db.ClearAllTables();
    }
    
    [Test]
    public void InsertUser_WithValidData_Succeeds()
    {
        var repo = new UserRepository(_db);
        var user = new User { Id = Guid.NewGuid(), Name = "TestUser" };
        
        repo.Insert(user);
        
        var retrieved = repo.Get(user.Id);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved.Name, Is.EqualTo(user.Name));
    }
}
```

### Exemple 7 : Modèle assertions multiples

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class UserServiceTests : PeasyPilotNUnitTestBase
{
    private UserService _service = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _service = new UserService();
    }
    
    [Test]
    [Category("Integration")]
    public void CreateUser_WithValidData_CreatesAndReturnsUser()
    {
        var command = new CreateUserCommand 
        { 
            Email = "test@example.com", 
            Name = "Test User" 
        };
        
        var result = _service.Create(command);
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Email, Is.EqualTo(command.Email));
            Assert.That(result.Name, Is.EqualTo(command.Name));
        });
    }
}
```

### Exemple 8 : Attribut Ignore pour les tests en attente

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class PendingFeatureTests : PeasyPilotNUnitTestBase
{
    [Test]
    [Ignore("Feature not yet implemented")]
    public void UnfinishedFeature_ShouldBeIgnored()
    {
        // Ce test sera ignoré et signalé séparément
    }
    
    [TestCase(1)]
    [TestCase(2)]
    [Ignore("Awaiting API response")]
    public void ExternalApiTests_IgnoreAll(int id)
    {
        // Tous les cas de test ignorés
    }
}
```

### Exemple 9 : Filtrage par catégorie

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
[Category("Integration")]
public class IntegrationTests : PeasyPilotNUnitTestBase
{
    [Test]
    [Category("Database")]
    public void DbConnection_Succeeds()
    {
        Assert.That(true, Is.True);
    }
    
    [Test]
    [Category("Api")]
    public void ApiCall_Succeeds()
    {
        Assert.That(true, Is.True);
    }
}

// Exécutez uniquement les tests de base de données : dotnet test --filter "Category=Database"
// Exécutez uniquement l'intégration : dotnet test --filter "Category=Integration"
```

### Exemple 10 : Tests asynchrones avec NUnit

```csharp
using NUnit.Framework;
using PeasyPilot.NUnit;

namespace MyApp.Tests;

[TestFixture]
public class AsyncTests : PeasyPilotNUnitTestBase
{
    private AsyncService _service = null!;
    
    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _service = new AsyncService();
    }
    
    [Test]
    public async Task FetchData_WithValidId_ReturnsData()
    {
        var result = await _service.FetchDataAsync(1);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("data"));
    }
    
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task FetchUsers_WithVariousIds_ReturnsResults(int id)
    {
        var result = await _service.FetchUserAsync(id);
        Assert.That(result, Is.Not.Null);
    }
}
```

---

## Bonnes pratiques

### 1. Appelez toujours les méthodes de base

```csharp
[SetUp]
public override void Setup()
{
    base.Setup();  // ✅ Obligatoire
    // Votre initialisation ici
}

[TearDown]
public override void TearDown()
{
    // Votre nettoyage ici
    base.TearDown();  // ✅ Obligatoire
}
```

### 2. Utilisez l'attribut [TestFixture]

```csharp
// ✅ BON : Marque explicitement la classe de test
[TestFixture]
public class CalculatorTests : PeasyPilotNUnitTestBase { }

// ❌ À ÉVITER : Déclaration de fixture manquante (peut causer des problèmes)
public class CalculatorTests : PeasyPilotNUnitTestBase { }
```

### 3. Nommez les tests clairement

```csharp
// ✅ BON : Intention claire
[Test]
public void Add_WithPositiveNumbers_ReturnsSum() { }

// ❌ PAUVRE : Vague
[Test]
public void TestAdd() { }
```

### 4. Utilisez OneTimeSetUp pour les ressources coûteuses

```csharp
// ✅ BON : Ressource partagée coûteuse
[OneTimeSetUp]
public static void SetupDatabase()
{
    _db = new Database();
    _db.Connect();  // Opération coûteuse, faite une fois
}

// ❌ À ÉVITER : Ressource coûteuse par test
[SetUp]
public override void Setup()
{
    base.Setup();
    _db = new Database();  // Appelé avant chaque test
    _db.Connect();
}
```

### 5. Nettoyez l'état entre les tests

```csharp
[SetUp]
public override void Setup()
{
    base.Setup();
    // Effacez les données avant chaque test
    _db.ClearAllTables();
}
```

### 6. Utilisez les assertions multiples avec prudence

```csharp
// ✅ BON : Utilisez Assert.Multiple pour les assertions associées
[Test]
public void User_HasRequiredProperties()
{
    var user = new User { Id = 1, Name = "Alice" };
    
    Assert.Multiple(() =>
    {
        Assert.That(user.Id, Is.Not.EqualTo(0));
        Assert.That(user.Name, Is.Not.Null);
    });
}

// ❌ À ÉVITER : Tester plusieurs comportements non liés
[Test]
public void AllFeatures_Work()
{
    Assert.That(_calc.Add(2, 2), Is.EqualTo(4));
    Assert.That(_calc.Multiply(3, 3), Is.EqualTo(9));
    Assert.That(_calc.Divide(10, 2), Is.EqualTo(5));
}
```

### 7. Le nettoyage s'exécute toujours

Rappelez-vous que `[TearDown]` s'exécute même si le test échoue :

```csharp
[TearDown]
public override void TearDown()
{
    try
    {
        // Le nettoyage se produit quel que soit le résultat du test
        _resource?.Dispose();
    }
    finally
    {
        base.TearDown();
    }
}
```

---

## Voir aussi

- [Documentation NUnit](https://docs.nunit.org/)
- [Référence API PeasyPilot.Core](api-core-FR.md)
- [Guide des adaptateurs de framework](../GUIDES/framework-adapters-guide-FR.md)
- [Guide des tests unitaires](../GUIDES/unit-testing-guide-FR.md)

---

**Dernière mise à jour :** 2026-09-11  
**Version :** 1.0  
[← Retour à RÉFÉRENCE](README.md)
