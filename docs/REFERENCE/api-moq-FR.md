# Référence API PeasyPilot.Moq

## Aperçu

`PeasyPilot.Moq` fournit une abstraction d'usine de mock unifiée pour créer des objets mock à l'aide de la populaire bibliothèque Moq. Il simplifie la création de mocks dans les tests unitaires en fournissant une interface indépendante du framework qui abstrait la complexité de l'API Moq et permet une intégration transparente avec l'injection de dépendances et les fixtures de test.

**Responsabilités principales :**
- Abstraction de création d'objets mock via IMockFactory
- Intégration Moq sans couplage strict
- Support de mocks génériques type-safe
- Intégration transparente du conteneur DI
- Motifs de création de mocks indépendants du framework
- Support pour tous les scénarios Moq courants (setup, vérification, comportement)

**Cibles :** .NET 8.0, 9.0, 10.0

**Dépendances :** Moq 4.x (abstraite)

---

## Abstractions Principales

### IMockFactory

Interface centrale pour créer des objets mock de manière indépendante du framework.

```csharp
namespace PeasyPilot.Core.Abstractions;

/// <summary>
/// Usine pour créer des objets mock.
/// </summary>
public interface IMockFactory
{
    /// <summary>
    /// Crée une instance mock du type spécifié.
    /// </summary>
    /// <param name="type">Le type à mocker.</param>
    /// <returns>Un objet mock.</returns>
    object Create(Type type);
}
```

**Objectif :** Fournit un point d'abstraction unique pour toute création de mock, permettant des implémentations interchangeables (Moq, NSubstitute, etc.) sans changer le code de test.

---

### MockFactory

Implémentation concrète utilisant Moq 4.x.

```csharp
namespace PeasyPilot.Moq;

/// <summary>
/// Usine pour créer des objets mock en utilisant Moq.
/// </summary>
public class MockFactory : IMockFactory
{
    /// <summary>
    /// Crée une instance mock du type spécifié.
    /// </summary>
    /// <param name="type">Le type à mocker.</param>
    /// <returns>Un objet mock.</returns>
    public object Create(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var mockType = typeof(Mock<>).MakeGenericType(type);
        var mockInstance = Activator.CreateInstance(mockType);
        var objectProperty = mockType.GetProperty("Object");
        return objectProperty!.GetValue(mockInstance)!;
    }
}
```

**Objectif :** Utilise la réflexion pour créer dynamiquement des instances `Mock<T>` et extraire leur propriété `.Object`, permettant la création de mocks agnostiques du type.

---

## Motifs Principaux

### Stratégie de Création de Mock

MockFactory utilise une stratégie basée sur la réflexion pour supporter la création de type générique :

1. **Création Générique Type-Safe :** `Mock<T>` est créé via la réflexion
2. **Extraction d'Objet :** La propriété `.Object` retourne l'instance mockable
3. **Casting à l'Utilisation :** Les appelants castent au type interface/classe attendu
4. **Setup et Vérification :** Les motifs Moq standard s'appliquent après création

### Intégration avec les Conteneurs DI

```csharp
public static class MockFactoryServiceExtensions
{
    /// <summary>
    /// Enregistre l'implémentation IMockFactory.
    /// </summary>
    public static IServiceCollection AddMockFactory(
        this IServiceCollection services)
    {
        services.AddSingleton<IMockFactory, MockFactory>();
        return services;
    }
}
```

---

## Exemples Concrets

### Exemple 1 : Création Basique de Mock

```csharp
using PeasyPilot.Moq;
using Moq;

public class BasicMockCreationTest
{
    [Fact]
    public void TestCreateSimpleMock()
    {
        var factory = new MockFactory();

        // Create mock of an interface
        var userService = (IUserService)factory.Create(typeof(IUserService));

        Assert.NotNull(userService);
    }

    [Fact]
    public void TestCreateMockWithMultipleInterfaces()
    {
        var factory = new MockFactory();

        var repository = (IUserRepository)factory.Create(typeof(IUserRepository));
        var logger = (ILogger)factory.Create(typeof(ILogger));

        Assert.NotNull(repository);
        Assert.NotNull(logger);
    }
}

public interface IUserService
{
    Task<User> GetUserAsync(int id);
}

public interface IUserRepository
{
    Task<User?> FindByIdAsync(int id);
}

public interface ILogger
{
    void Log(string message);
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

### Exemple 2 : Setup et Vérification de Mock

```csharp
using PeasyPilot.Moq;
using Moq;

public class MockSetupAndVerificationTest
{
    [Fact]
    public async Task TestMockSetupAndVerification()
    {
        var factory = new MockFactory();
        
        // Create the mock
        var userServiceMock = factory.Create(typeof(IUserService));
        var userService = (IUserService)userServiceMock;

        // Setup behavior using Moq's extension methods
        // Note: You need to work with the Mock<T> instance for setup
        var moqInstance = new Mock<IUserService>();
        moqInstance
            .Setup(x => x.GetUserAsync(It.IsAny<int>()))
            .ReturnsAsync(new User { Id = 1, Name = "John Doe" });

        var result = await moqInstance.Object.GetUserAsync(1);

        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);

        // Verify the call
        moqInstance.Verify(
            x => x.GetUserAsync(1),
            Times.Once);
    }

    [Fact]
    public void TestMockThrowsException()
    {
        var userServiceMock = new Mock<IUserService>();
        
        userServiceMock
            .Setup(x => x.GetUserAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        var userService = userServiceMock.Object;

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await userService.GetUserAsync(999));
        
        Assert.NotNull(ex);
    }
}
```

### Exemple 3 : Factory avec Injection de Dépendances

```csharp
using PeasyPilot.Moq;
using PeasyPilot.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public class MockFactoryDiTest
{
    [Fact]
    public void TestMockFactoryWithDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMockFactory, MockFactory>();
        var serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetRequiredService<IMockFactory>();

        var userRepository = (IUserRepository)factory.Create(typeof(IUserRepository));

        Assert.NotNull(userRepository);
    }

    [Fact]
    public void TestInjectMockFactoryIntoService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMockFactory, MockFactory>();
        services.AddScoped<TestableService>();
        var serviceProvider = services.BuildServiceProvider();

        var testableService = serviceProvider.GetRequiredService<TestableService>();

        Assert.NotNull(testableService.UserService);
    }
}

public class TestableService
{
    private readonly IUserService _userService;

    public TestableService(IMockFactory mockFactory)
    {
        _userService = (IUserService)mockFactory.Create(typeof(IUserService));
    }

    public IUserService UserService => _userService;
}
```

### Exemple 4 : Motif Repository Mock

```csharp
using PeasyPilot.Moq;
using Moq;

public class RepositoryPatternTest
{
    [Fact]
    public async Task TestMockRepository()
    {
        var repositoryMock = new Mock<IUserRepository>();

        var testUsers = new List<User>
        {
            new User { Id = 1, Name = "Alice" },
            new User { Id = 2, Name = "Bob" }
        };

        repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(testUsers);

        repositoryMock
            .Setup(r => r.FindByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => testUsers.FirstOrDefault(u => u.Id == id));

        var repository = repositoryMock.Object;

        var all = await repository.GetAllAsync();
        Assert.Equal(2, all.Count);

        var user = await repository.FindByIdAsync(1);
        Assert.Equal("Alice", user?.Name);

        repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }
}

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> FindByIdAsync(int id);
    Task SaveAsync(User user);
}
```

### Exemple 5 : Multiples Mocks dans un Test de Service

```csharp
using PeasyPilot.Moq;
using Moq;

public class MultipleLocksServiceTest
{
    [Fact]
    public async Task TestServiceWithMultipleMocks()
    {
        // Setup mocks
        var repositoryMock = new Mock<IUserRepository>();
        var loggerMock = new Mock<ILogger>();
        var emailServiceMock = new Mock<IEmailService>();

        repositoryMock
            .Setup(r => r.FindByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new User { Id = 1, Name = "John Doe", Email = "john@example.com" });

        emailServiceMock
            .Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Create service with mocks
        var userService = new UserService(
            repositoryMock.Object,
            loggerMock.Object,
            emailServiceMock.Object);

        // Execute
        var result = await userService.NotifyUserAsync(1, "Welcome!");

        // Verify
        Assert.True(result);
        repositoryMock.Verify(r => r.FindByIdAsync(1), Times.Once);
        loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.AtLeast(1));
        emailServiceMock.Verify(e => e.SendAsync("john@example.com", "Welcome!"), Times.Once);
    }
}

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger _logger;
    private readonly IEmailService _emailService;

    public UserService(IUserRepository repository, ILogger logger, IEmailService emailService)
    {
        _repository = repository;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<bool> NotifyUserAsync(int userId, string message)
    {
        _logger.Log($"Notifying user {userId}");
        
        var user = await _repository.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _emailService.SendAsync(user.Email, message);
        _logger.Log($"Notification {'sent' if result else 'failed'}");
        
        return result;
    }
}

public interface IEmailService
{
    Task<bool> SendAsync(string to, string message);
}
```

### Exemple 6 : Mock avec Motifs It.IsAny

```csharp
using PeasyPilot.Moq;
using Moq;

public class ItIsAnyPatternsTest
{
    [Fact]
    public void TestMockWithWildcardMatching()
    {
        var processorMock = new Mock<IDataProcessor>();

        // Setup for any string input
        processorMock
            .Setup(p => p.Process(It.IsAny<string>()))
            .Returns("Processed");

        // Setup for specific range
        processorMock
            .Setup(p => p.Calculate(It.IsInRange<int>(0, 100, Moq.Range.Inclusive)))
            .Returns(true);

        var processor = processorMock.Object;

        Assert.Equal("Processed", processor.Process("anything"));
        Assert.Equal("Processed", processor.Process("foo"));
        Assert.True(processor.Calculate(50));

        processorMock.Verify(p => p.Process(It.IsAny<string>()), Times.Exactly(2));
    }
}

public interface IDataProcessor
{
    string Process(string input);
    bool Calculate(int value);
}
```

### Exemple 7 : Callbacks et Valeurs de Retour Mock

```csharp
using PeasyPilot.Moq;
using Moq;

public class CallbacksAndReturnTest
{
    [Fact]
    public void TestMockCallbacks()
    {
        var serviceMock = new Mock<IOrderService>();

        var callCount = 0;

        serviceMock
            .Setup(s => s.ProcessOrder(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                callCount++;
                order.Status = "Processing";
            })
            .Returns("OrderProcessed");

        var service = serviceMock.Object;
        var order = new Order { Id = 1 };

        var result = service.ProcessOrder(order);

        Assert.Equal("OrderProcessed", result);
        Assert.Equal(1, callCount);
        Assert.Equal("Processing", order.Status);
    }

    [Fact]
    public void TestMockReturnsSequence()
    {
        var counterMock = new Mock<ICounter>();

        counterMock
            .SetupSequence(c => c.GetNext())
            .Returns(1)
            .Returns(2)
            .Returns(3)
            .Throws(new InvalidOperationException("Exhausted"));

        var counter = counterMock.Object;

        Assert.Equal(1, counter.GetNext());
        Assert.Equal(2, counter.GetNext());
        Assert.Equal(3, counter.GetNext());
        Assert.Throws<InvalidOperationException>(() => counter.GetNext());
    }
}

public class Order
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
}

public interface IOrderService
{
    string ProcessOrder(Order order);
}

public interface ICounter
{
    int GetNext();
}
```

### Exemple 8 : Mocking de Propriétés

```csharp
using PeasyPilot.Moq;
using Moq;

public class PropertyMockingTest
{
    [Fact]
    public void TestMockProperties()
    {
        var configMock = new Mock<IConfiguration>();

        configMock
            .Setup(c => c.DatabaseConnectionString)
            .Returns("Server=localhost;Database=TestDb");

        configMock
            .Setup(c => c.LogLevel)
            .Returns("Debug");

        var config = configMock.Object;

        Assert.Equal("Server=localhost;Database=TestDb", config.DatabaseConnectionString);
        Assert.Equal("Debug", config.LogLevel);
    }

    [Fact]
    public void TestPropertySetterTracking()
    {
        var cacheMock = new Mock<ICache>();

        cacheMock.SetupProperty(c => c.Ttl, TimeSpan.FromMinutes(5));

        var cache = cacheMock.Object;

        // Can set property
        cache.Ttl = TimeSpan.FromMinutes(10);

        // Can read property
        Assert.Equal(TimeSpan.FromMinutes(10), cache.Ttl);

        cacheMock.VerifySet(c => c.Ttl = TimeSpan.FromMinutes(10), Times.Once);
    }
}

public interface IConfiguration
{
    string DatabaseConnectionString { get; }
    string LogLevel { get; }
}

public interface ICache
{
    TimeSpan Ttl { get; set; }
}
```

### Exemple 9 : Mocking Loose vs Strict

```csharp
using PeasyPilot.Moq;
using Moq;

public class LooseVsStrictMockingTest
{
    [Fact]
    public void TestLooseMock()
    {
        // Default behavior - returns default values for unmocked calls
        var serviceMock = new Mock<IService>(MockBehavior.Loose);
        
        var service = serviceMock.Object;

        // These calls don't throw, just return defaults
        var result = service.GetValue("unmocked");
        Assert.Null(result);
    }

    [Fact]
    public void TestStrictMock()
    {
        // Strict behavior - throws for unmocked calls
        var serviceMock = new Mock<IService>(MockBehavior.Strict);
        
        serviceMock
            .Setup(s => s.GetValue("valid"))
            .Returns("Value");

        var service = serviceMock.Object;

        Assert.Equal("Value", service.GetValue("valid"));

        // This throws because GetValue("unmocked") was not setup
        Assert.Throws<MockException>(() => service.GetValue("unmocked"));
    }
}

public interface IService
{
    string? GetValue(string key);
}
```

### Exemple 10 : Intégration avec les Fixtures de Test

```csharp
using PeasyPilot.Moq;
using PeasyPilot.Unit.Fixtures;

public class FixtureIntegrationTest : UnitTestFixture
{
    private readonly MockFactory _mockFactory = new();

    [Fact]
    public void TestMockInFixture()
    {
        var userService = (IUserService)_mockFactory.Create(typeof(IUserService));

        Assert.NotNull(userService);
    }

    [Fact]
    public void TestMultipleMocksInFixture()
    {
        var repository = (IUserRepository)_mockFactory.Create(typeof(IUserRepository));
        var logger = (ILogger)_mockFactory.Create(typeof(ILogger));

        Assert.NotNull(repository);
        Assert.NotNull(logger);
    }
}
```

---

## Bonnes Pratiques

1. **Utiliser l'Interface IMockFactory :** Injecter toujours `IMockFactory` pour permettre la flexibilité
2. **Setup Avant Utilisation :** Configurer le comportement du mock avant de le passer au code testé
3. **Vérifier les Attentes :** Utiliser Verify pour s'assurer que les interactions attendues se sont produites
4. **Garder les Mocks Simples :** Mocker seulement ce qui est nécessaire; le sur-mocking crée des tests fragiles
5. **Utiliser le Mode Strict avec Prudence :** Le mode Loose est par défaut; le mode strict pour les contrats critiques
6. **Éviter de Mocker les Types Concrets :** Mocker les interfaces/abstractions, pas les implémentations concrètes

---

## Considérations de Performances

- **Création de Mock :** La création basée sur la réflexion est rapide (microsecondes par mock)
- **Surcharge de Setup :** Surcharge minimale pour setup et vérification
- **Mémoire :** Chaque mock utilise une mémoire minimale; adapter à des milliers sans problème
- **Isolation des Tests :** Les mocks fournissent une excellente isolation des tests sans état partagé

---

## Voir Aussi

- [API PeasyPilot.Core](api-core-FR.md)
- [API PeasyPilot.Unit](api-unit-FR.md)
- [Guide de Test Unitaire](../GUIDES/unit-testing-guide-FR.md)
- [Documentation Moq](https://github.com/moq/moq4)

