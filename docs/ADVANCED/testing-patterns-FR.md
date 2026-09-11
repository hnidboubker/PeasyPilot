# Patterns de Test Avancés

## Aperçu

Dans ce guide, vous apprendrez des stratégies de test avancées et des patterns pour écrire des tests plus efficaces, maintenables et complets :
- Maîtriser le pattern Arrange-Act-Assert (AAA) et ses variations avancées
- Appliquer les patterns Given-When-Then pour les tests BDD
- Construire des objets de test avec le pattern Builder fluide
- Gérer le cycle de vie des tests avec les patterns de Fixture
- Valider la qualité des tests avec les tests de mutation
- Générer des cas de test avec les tests basés sur les propriétés
- Utiliser efficacement les doublures de test (Mocks, Stubs, Fakes)

**Prérequis:** Terminer le [Guide des Tests Unitaires](../GUIDES/unit-testing-guide-FR.md)  
**Durée estimée:** 60 minutes  
**Frameworks:** xUnit, NUnit, TUnit  
**Exemples de code:** 15+ exemples fonctionnels

---

## 1. Pattern Arrange-Act-Assert (AAA) — Variations Avancées

### Pattern de Base (Révision)

Le pattern AAA structure les tests en trois phases distinctes. Bien que vous ayez vu les bases, les variations avancées aident avec les scénarios complexes.

#### Variation 1: Arrange-Act-Assert-Cleanup (AAA+C)

Pour les tests qui nécessitent un nettoyage de ressources :

```csharp
[Fact]
public void FileProcessor_CreatesLogFile_WhenProcessingDocument()
{
    // ARRANGE
    var logPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    var processor = new DocumentProcessor(logPath);
    var testDoc = new Document { Content = "Test content" };
    
    try
    {
        // ACT
        processor.Process(testDoc);
        
        // ASSERT
        Assert.True(File.Exists(logPath));
        var logContent = File.ReadAllText(logPath);
        Assert.Contains("Process completed", logContent);
    }
    finally
    {
        // CLEANUP
        if (File.Exists(logPath))
            File.Delete(logPath);
    }
}
```

#### Variation 2: Assertions Multiples avec Contexte

Lors du test d'objets complexes, groupez les assertions par contexte :

```csharp
[Fact]
public void UserRepository_CreateUser_PersistsAllProperties()
{
    // ARRANGE
    var repository = new UserRepository(_fixture.DbContext);
    var user = new User
    {
        FirstName = "Alice",
        LastName = "Smith",
        Email = "alice@example.com",
        CreatedDate = DateTime.UtcNow
    };
    
    // ACT
    var createdUser = repository.Create(user);
    
    // ASSERT - Propriétés de base
    Assert.NotNull(createdUser.Id);
    Assert.Equal(user.FirstName, createdUser.FirstName);
    Assert.Equal(user.LastName, createdUser.LastName);
    
    // ASSERT - Propriétés de l'e-mail
    Assert.Equal(user.Email, createdUser.Email);
    Assert.True(createdUser.EmailConfirmed == false);
    
    // ASSERT - Propriétés d'audit
    Assert.Equal(DateTime.UtcNow.Date, createdUser.CreatedDate.Date);
    Assert.Null(createdUser.ModifiedDate);
}
```

#### Variation 3: Pattern Assert-Verify (pour les tests basés sur l'état vs les interactions)

```csharp
[Fact]
public void OrderProcessor_ProcessesOrder_UpdatesInventoryAndSendsNotification()
{
    // ARRANGE
    var inventoryMock = new Mock<IInventoryService>();
    var notificationMock = new Mock<INotificationService>();
    var processor = new OrderProcessor(inventoryMock.Object, notificationMock.Object);
    
    var order = new Order { OrderId = 1, Quantity = 5 };
    
    // ACT
    processor.ProcessOrder(order);
    
    // ASSERT - État
    inventoryMock.Verify(x => x.DecrementStock(1, 5), Times.Once);
    
    // ASSERT - Interactions
    notificationMock.Verify(
        x => x.SendOrderConfirmation(It.Is<Order>(o => o.OrderId == 1)),
        Times.Once);
}
```

---

## 2. Pattern Given-When-Then (GWT) — Style BDD

Le pattern GWT aligne les tests avec les exigences métier en utilisant les principes BDD. Cela s'intègre naturellement avec le framework BDD de PeasyPilot.

#### Exemple : Scénario d'Inscription d'Utilisateur

```csharp
public class UserRegistrationBddTests : PeasyPilotBddTestBase
{
    private UserRepository _userRepository;
    private UserService _userService;
    private User _newUser;
    private Exception _registrationException;
    
    [Fact]
    public void UserRegistration_ValidEmail_SuccessfullyCreatesAccount()
    {
        // GIVEN: Un nouvel utilisateur avec des identifiants valides
        GivenNewUserWithValidCredentials();
        
        // WHEN: L'utilisateur est enregistré
        WhenUserIsRegistered();
        
        // THEN: Le compte est créé et l'utilisateur peut se connecter
        ThenUserAccountIsCreated();
        ThenUserCanLogin();
    }
    
    [Fact]
    public void UserRegistration_DuplicateEmail_FailsWithConflict()
    {
        // GIVEN: Un utilisateur existant avec l'email alice@example.com
        GivenExistingUserWithEmail("alice@example.com");
        
        // WHEN: Tentative d'enregistrement avec le même email
        WhenUserIsRegisteredWithEmail("alice@example.com");
        
        // THEN: L'enregistrement échoue avec une erreur d'email en doublon
        ThenRegistrationFailsWithError("Duplicate email");
    }
    
    // Méthodes d'aide Given-When-Then
    private void GivenNewUserWithValidCredentials()
    {
        _newUser = new User
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "SecurePassword123!"
        };
    }
    
    private void GivenExistingUserWithEmail(string email)
    {
        var user = new User
        {
            Email = email,
            FirstName = "Existing",
            LastName = "User"
        };
        _userRepository.Create(user);
    }
    
    private void WhenUserIsRegistered()
    {
        try
        {
            _userService.Register(_newUser);
            _registrationException = null;
        }
        catch (Exception ex)
        {
            _registrationException = ex;
        }
    }
    
    private void WhenUserIsRegisteredWithEmail(string email)
    {
        _newUser.Email = email;
        WhenUserIsRegistered();
    }
    
    private void ThenUserAccountIsCreated()
    {
        Assert.Null(_registrationException);
        var savedUser = _userRepository.GetByEmail(_newUser.Email);
        Assert.NotNull(savedUser);
    }
    
    private void ThenUserCanLogin()
    {
        var loginResult = _userService.Login(_newUser.Email, _newUser.Password);
        Assert.NotNull(loginResult.Token);
    }
    
    private void ThenRegistrationFailsWithError(string expectedError)
    {
        Assert.NotNull(_registrationException);
        Assert.Contains(expectedError, _registrationException.Message);
    }
}
```

---

## 3. Pattern Builder pour les Tests

Le pattern Builder réduit la complexité de la configuration des tests et améliore la lisibilité par le biais d'API fluides.

#### Exemple 1: TestDataBuilder

```csharp
public class OrderBuilder
{
    private string _customerId = "CUST001";
    private decimal _totalAmount = 100m;
    private List<OrderItem> _items = new();
    private OrderStatus _status = OrderStatus.Pending;
    private DateTime _createdDate = DateTime.UtcNow;
    
    public OrderBuilder WithCustomerId(string customerId)
    {
        _customerId = customerId;
        return this;
    }
    
    public OrderBuilder WithAmount(decimal amount)
    {
        _totalAmount = amount;
        return this;
    }
    
    public OrderBuilder WithItem(string productId, int quantity, decimal price)
    {
        _items.Add(new OrderItem
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = price
        });
        return this;
    }
    
    public OrderBuilder WithStatus(OrderStatus status)
    {
        _status = status;
        return this;
    }
    
    public OrderBuilder CreatedOn(DateTime date)
    {
        _createdDate = date;
        return this;
    }
    
    public Order Build()
    {
        return new Order
        {
            CustomerId = _customerId,
            TotalAmount = _totalAmount,
            Items = _items,
            Status = _status,
            CreatedDate = _createdDate
        };
    }
}

// Utilisation dans les tests
[Fact]
public void OrderProcessor_CalculateDiscount_AppliesToOrders()
{
    // ARRANGE: Construire une commande complexe avec le builder
    var order = new OrderBuilder()
        .WithCustomerId("CUST001")
        .WithItem("PROD001", 2, 50m)
        .WithItem("PROD002", 1, 100m)
        .WithStatus(OrderStatus.Confirmed)
        .Build();
    
    var processor = new OrderProcessor();
    
    // ACT
    var discountedTotal = processor.CalculateDiscount(order);
    
    // ASSERT
    Assert.True(discountedTotal < order.TotalAmount);
}
```

#### Exemple 2: Pattern AnonymousBuilder

Pour quand vous ne vous souciez que de propriétés spécifiques :

```csharp
public class AnonymousUserBuilder
{
    private readonly User _user = new();
    
    public AnonymousUserBuilder()
    {
        _user.Id = Guid.NewGuid();
        _user.FirstName = "John";
        _user.LastName = "Doe";
        _user.Email = $"user_{Guid.NewGuid()}@example.com";
        _user.IsActive = true;
    }
    
    public AnonymousUserBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }
    
    public AnonymousUserBuilder WithoutEmail()
    {
        _user.Email = null;
        return this;
    }
    
    public AnonymousUserBuilder AsInactive()
    {
        _user.IsActive = false;
        return this;
    }
    
    public User Build() => _user;
}

[Fact]
public void UserValidator_ValidatesEmail_WhenPresent()
{
    // Arrange: Personnaliser uniquement ce qui importe pour ce test
    var user = new AnonymousUserBuilder()
        .WithEmail("invalid-email")
        .Build();
    
    // Act & Assert
    Assert.False(UserValidator.IsValid(user));
}
```

---

## 4. Patterns de Fixture

Les fixtures gèrent la configuration, le démontage et le cycle de vie des ressources sur plusieurs tests.

#### Exemple 1: Fixture Partagée (Une instance par classe de test)

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly IHost _host;
    public DbContext DbContext { get; private set; }
    
    public DatabaseFixture()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<DbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            })
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _host.StartAsync();
        DbContext = _host.Services.GetRequiredService<DbContext>();
        await DbContext.Database.EnsureCreatedAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
    }
}

public class UserRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture = new();
    
    public Task InitializeAsync() => _fixture.InitializeAsync();
    public Task DisposeAsync() => _fixture.DisposeAsync();
    
    [Fact]
    public void Repository_GetUser_ReturnsPersistedUser()
    {
        // ARRANGE
        var user = new User { Email = "test@example.com" };
        _fixture.DbContext.Users.Add(user);
        _fixture.DbContext.SaveChanges();
        
        var repository = new UserRepository(_fixture.DbContext);
        
        // ACT
        var result = repository.GetByEmail("test@example.com");
        
        // ASSERT
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }
}
```

#### Exemple 2: Fixture Isolée (Instance fraîche par test)

```csharp
public class IsolatedDatabaseFixture : IAsyncLifetime
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    public DbContext DbContext { get; private set; }
    
    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;
        
        DbContext = new DbContext(options);
        await DbContext.Database.EnsureCreatedAsync();
    }
    
    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        DbContext.Dispose();
    }
}

public class OrderProcessingTests
{
    [Fact]
    public async Task ProcessOrder_WithValidData_CreatesOrderRecord()
    {
        // Utiliser une fixture - base de données fraîche pour ce test
        await using var fixture = new IsolatedDatabaseFixture();
        await fixture.InitializeAsync();
        
        var repository = new OrderRepository(fixture.DbContext);
        var order = new Order { /* ... */ };
        
        repository.Create(order);
        
        var retrieved = repository.GetById(order.Id);
        Assert.NotNull(retrieved);
        
        await fixture.DisposeAsync();
    }
}
```

---

## 5. Tests de Mutation

Les tests de mutation valident que vos tests détectent réellement les bugs en injectant de petites modifications de code (mutations).

### Comment Fonctionnent les Tests de Mutation

Un outil de mutation modifie votre code (par exemple, `>` à `>=`, `true` à `false`) et réexécute les tests. Si les tests réussissent malgré la mutation, vos tests ont une lacune.

#### Exemple : Écrire des Tests pour les Tests de Mutation

```csharp
public class PriceCalculator
{
    public decimal CalculateDiscount(decimal price, int quantityPurchased)
    {
        // Règle métier : 10% de réduction si quantité >= 10
        if (quantityPurchased >= 10)
            return price * 0.9m;
        
        return price;
    }
}

// ❌ TEST FAIBLE : Ne détecte pas la mutation de >= à >
[Fact]
public void CalculateDiscount_Quantity10_AppliesDiscount()
{
    var calc = new PriceCalculator();
    Assert.Equal(90m, calc.CalculateDiscount(100m, 10));
}

// ✅ TESTS FORTS : Détectent les mutations
[Fact]
public void CalculateDiscount_Quantity10_AppliesDiscount()
{
    var calc = new PriceCalculator();
    Assert.Equal(90m, calc.CalculateDiscount(100m, 10));
}

[Fact]
public void CalculateDiscount_Quantity9_NoDiscount()
{
    var calc = new PriceCalculator();
    Assert.Equal(100m, calc.CalculateDiscount(100m, 9));
}

[Fact]
public void CalculateDiscount_Quantity100_AppliesDiscount()
{
    var calc = new PriceCalculator();
    Assert.Equal(90m, calc.CalculateDiscount(100m, 100));
}
```

**Pourquoi les tests forts importent:** Le test faible réussit même si quelqu'un change `>=` en `>`. Les tests forts détectent cette mutation.

### Meilleures Pratiques pour les Tests de Mutation

1. **Tester les conditions aux limites** – `=, <, >, <=, >=`
2. **Tester les deux branches** – `if/else`, `true/false`
3. **Tester les changements d'opérateurs** – `+` à `-`, `*` à `/`
4. **Utiliser des bibliothèques d'assertion** – Les assertions plus spécifiques détectent plus de mutations

```csharp
// Meilleure détection de mutation avec FluentAssertions
[Fact]
public void CalculateDiscount_EdgeCases_WorkCorrectly()
{
    var calc = new PriceCalculator();
    
    calc.CalculateDiscount(100m, 9).Should().Be(100m);
    calc.CalculateDiscount(100m, 10).Should().Be(90m);
    calc.CalculateDiscount(100m, 0).Should().Be(100m);
}
```

---

## 6. Tests Basés sur les Propriétés

Les tests basés sur les propriétés génèrent automatiquement de nombreux cas de test, détectant les cas limites que vous pourriez manquer.

### Utiliser Bogus pour la Génération de Données

PeasyPilot inclut le support de Bogus pour générer des données de test réalistes :

```csharp
using Bogus;
using Xunit;

public class PasswordValidationTests
{
    private readonly Faker<User> _userFaker = new Faker<User>()
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.Password, f => f.Internet.Password(8));
    
    [Fact]
    public void PasswordValidator_100GeneratedPasswords_AllValidateCorrectly()
    {
        var validator = new PasswordValidator();
        
        // Générer 100 utilisateurs aléatoires
        var users = _userFaker.Generate(100);
        
        foreach (var user in users)
        {
            var result = validator.IsPasswordValid(user.Password);
            Assert.True(result, $"Password '{user.Password}' should be valid");
        }
    }
    
    [Fact]
    public void PasswordValidator_WeakPasswords_Rejected()
    {
        var validator = new PasswordValidator();
        var weakPasswords = new[] { "123", "abc", "pass", "12345", "" };
        
        foreach (var password in weakPasswords)
        {
            var result = validator.IsPasswordValid(password);
            Assert.False(result, $"Password '{password}' should be invalid");
        }
    }
}
```

### Patterns de Tests Basés sur les Propriétés

```csharp
public class MathOperationsPropertyTests
{
    private readonly Faker _faker = new();
    
    [Fact]
    public void Addition_Commutative_Property()
    {
        // Propriété : a + b == b + a (pour tout a, b)
        for (int i = 0; i < 100; i++)
        {
            decimal a = (decimal)_faker.Random.Double();
            decimal b = (decimal)_faker.Random.Double();
            
            var calc = new Calculator();
            Assert.Equal(calc.Add(a, b), calc.Add(b, a));
        }
    }
    
    [Fact]
    public void Multiplication_IdentityProperty()
    {
        // Propriété : a * 1 == a (pour tout a)
        for (int i = 0; i < 100; i++)
        {
            decimal a = (decimal)_faker.Random.Double();
            
            var calc = new Calculator();
            Assert.Equal(a, calc.Multiply(a, 1));
        }
    }
}
```

---

## 7. Doublures de Test — Mocks, Stubs, and Fakes

### Comprendre les Doublures de Test

| Doublure de Test | Objectif | Quand l'Utiliser |
|------------------|----------|-----------------|
| **Mock** | Vérifier les interactions | Tester que les méthodes sont appelées |
| **Stub** | Fournir des réponses en boîte | Simuler le comportement du service externe |
| **Fake** | Implémentation fonctionnelle | Tester la logique métier |
| **Spy** | Enregistrer les interactions | Mocking partiel avec comportement réel |

### Exemple 1: Mock (Vérifier les Interactions)

```csharp
[Fact]
public void OrderProcessor_SendsConfirmationEmail_WhenOrderConfirmed()
{
    // ARRANGE
    var emailServiceMock = new Mock<IEmailService>();
    var processor = new OrderProcessor(emailServiceMock.Object);
    var order = new Order { OrderId = 123, CustomerEmail = "test@example.com" };
    
    // ACT
    processor.ConfirmOrder(order);
    
    // ASSERT: Vérifier que le service d'email a été appelé correctement
    emailServiceMock.Verify(
        x => x.SendConfirmation(
            It.Is<Order>(o => o.OrderId == 123),
            It.Is<string>(s => s == "test@example.com")),
        Times.Once,
        "Email should be sent exactly once");
}
```

### Exemple 2: Stub (Fournir des Réponses)

```csharp
[Fact]
public void UserService_GetUser_ReturnsStubData()
{
    // ARRANGE: Créer un stub
    var userRepositoryStub = new Mock<IUserRepository>();
    userRepositoryStub
        .Setup(x => x.GetById(1))
        .Returns(new User { Id = 1, FirstName = "Alice" });
    
    var service = new UserService(userRepositoryStub.Object);
    
    // ACT
    var user = service.GetUser(1);
    
    // ASSERT
    Assert.Equal("Alice", user.FirstName);
}
```

### Exemple 3: Fake (Implémentation Fonctionnelle)

```csharp
public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<int, User> _users = new();
    
    public User GetById(int id)
    {
        return _users.TryGetValue(id, out var user) ? user : null;
    }
    
    public void Save(User user)
    {
        _users[user.Id] = user;
    }
}

[Fact]
public void UserService_SaveAndRetrieve_WorksWithFakeRepository()
{
    // ARRANGE: Utiliser une implémentation fake
    var fakeRepository = new InMemoryUserRepository();
    var service = new UserService(fakeRepository);
    var user = new User { Id = 1, FirstName = "Bob" };
    
    // ACT
    service.SaveUser(user);
    var retrieved = service.GetUser(1);
    
    // ASSERT
    Assert.Equal("Bob", retrieved.FirstName);
}
```

### Exemple 4: Spy (Enregistrer + Vérifier)

```csharp
[Fact]
public void Logger_LogsAllMessages_WhenEnabled()
{
    // ARRANGE: Espionner le logger réel
    var realLogger = new ConsoleLogger();
    var loggerSpy = new Mock<ILogger>(MockBehavior.Default) { CallBase = true };
    loggerSpy.Setup(x => x.Log(It.IsAny<string>())).CallBase();
    
    var service = new ReportGenerator(loggerSpy.Object);
    
    // ACT
    service.GenerateReport();
    
    // ASSERT: Vérifier ce qui a été enregistré
    loggerSpy.Verify(x => x.Log(It.IsAny<string>()), Times.AtLeastOnce);
}
```

---

## 8. Tester du Code Asynchrone

Le code asynchrone introduit des défis de test uniques. Comprendre les patterns async garantit que vos tests sont fiables et ne cachent pas les conditions de course.

### Pattern 1 : Tester les Méthodes Async

```csharp
[Fact]
public async Task UserService_GetUserAsync_ReturnsUser()
{
    // ARRANGE
    var repository = new Mock<IUserRepository>();
    repository
        .Setup(x => x.GetUserAsync(1))
        .ReturnsAsync(new User { Id = 1, FirstName = "Alice" });
    
    var service = new UserService(repository.Object);
    
    // ACT
    var result = await service.GetUserAsync(1);
    
    // ASSERT
    Assert.NotNull(result);
    Assert.Equal("Alice", result.FirstName);
}
```

### Pattern 2 : Tester l'Annulation de Tâche

```csharp
[Fact]
public async Task LongRunningOperation_WithCancellation_StopsGracefully()
{
    // ARRANGE
    var cts = new CancellationTokenSource();
    var operation = new LongRunningOperation();
    
    // ACT: Démarrer l'opération et annuler après 100ms
    var task = operation.ExecuteAsync(cts.Token);
    await Task.Delay(100);
    cts.Cancel();
    
    // ASSERT: L'opération doit être annulée
    await Assert.ThrowsAsync<OperationCanceledException>(() => task);
}
```

### Pattern 3 : Tester le Comportement du Délai d'Expiration

```csharp
[Fact]
public async Task ApiClient_WithTimeout_ThrowsTimeoutException()
{
    // ARRANGE
    var client = new ApiClient(timeoutMs: 100);
    
    // ACT & ASSERT: Doit expirer
    await Assert.ThrowsAsync<TimeoutException>(
        () => client.FetchDataAsync("http://httpbin.org/delay/5"));
}
```

### Pattern 4 : Composition de Tâches et Appels Async Multiples

```csharp
[Fact]
public async Task OrderProcessor_ProcessMultipleOrders_ExecutesInParallel()
{
    // ARRANGE
    var processor = new OrderProcessor();
    var orders = new[] 
    { 
        new Order { Id = 1 }, 
        new Order { Id = 2 }, 
        new Order { Id = 3 } 
    };
    
    var stopwatch = Stopwatch.StartNew();
    
    // ACT: Traiter les commandes en parallèle
    var results = await Task.WhenAll(
        orders.Select(o => processor.ProcessAsync(o))
    );
    
    stopwatch.Stop();
    
    // ASSERT: Toutes complétées avec succès et plus rapidement qu'en séquentiel
    Assert.Equal(3, results.Length);
    Assert.True(stopwatch.ElapsedMilliseconds < 1000); // Rapide car parallèle
}
```

---

## 9. Stratégies de Test Avancées

### Stratégie 1 : Pyramide de Test

Structurez les tests par type et nombre :

```
        △ Tests E2E (10%)
       △△ Tests d'Intégration (30%)
      △△△△△ Tests Unitaires (60%)
```

```csharp
// Test Unitaire (Rapide, Isolé)
[Fact]
public void Validator_Email_WithValidEmail_ReturnsTrue()
{
    var validator = new EmailValidator();
    Assert.True(validator.IsValid("test@example.com"));
}

// Test d'Intégration (Vitesse modérée, avec dépendances)
[Fact]
public async Task UserRepository_SaveAndLoad_PersistsToDatabase()
{
    var dbContext = GetTestDbContext();
    var repository = new UserRepository(dbContext);
    var user = new User { Email = "test@example.com" };
    
    repository.Save(user);
    var loaded = repository.GetByEmail("test@example.com");
    
    Assert.NotNull(loaded);
}

// Test E2E (Lent, système complet)
[Fact]
public async Task API_RegisterUser_CreateAccountViaHttpRequest()
{
    var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    var response = await client.PostAsync("/api/users/register", 
        new StringContent("{\"email\":\"test@example.com\"}"));
    
    Assert.True(response.IsSuccessStatusCode);
}
```

### Stratégie 2 : Isolation des Tests avec Setup/Teardown

```csharp
public class UserServiceTests : IAsyncLifetime
{
    private DbContext _dbContext;
    private UserService _userService;
    
    public async Task InitializeAsync()
    {
        // Setup: Créer une base de données propre
        _dbContext = new TestDbContextFactory().CreateDbContext();
        await _dbContext.Database.EnsureCreatedAsync();
        _userService = new UserService(_dbContext);
    }
    
    public async Task DisposeAsync()
    {
        // Teardown: Nettoyer les ressources
        await _dbContext.Database.EnsureDeletedAsync();
        _dbContext.Dispose();
    }
    
    [Fact]
    public void CreateUser_WithValidData_SuccessfullyCreates()
    {
        var user = new User { Email = "test@example.com" };
        var result = _userService.Create(user);
        Assert.NotNull(result.Id);
    }
}
```

---

## Résumé des Meilleures Pratiques

| Pratique | Avantage |
|----------|----------|
| **Responsabilité Unique** | Les tests sont plus faciles à maintenir |
| **Nommage Clair** | Les tests échoués sont auto-documentés |
| **Structure AAA/GWT** | Les tests sont prévisibles et lisibles |
| **Builders** | Réduit la configuration redondante |
| **Fixtures** | Gère le cycle de vie des ressources en toute sécurité |
| **Doublures de Test** | Isole le code testé |
| **Couverture Complète** | Détecte les mutations et les cas limites |
| **Tests Basés sur les Propriétés** | Trouve automatiquement les cas limites |

---

## Tout Mettre Ensemble

Voici un exemple complet combinant plusieurs patterns :

```csharp
public class CompleteOrderProcessingTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly OrderProcessor _processor;
    
    // Utiliser le pattern fixture
    public CompleteOrderProcessingTests()
    {
        var fixture = new DatabaseFixture();
        _orderRepository = new OrderRepository(fixture.DbContext);
        _processor = new OrderProcessor(_orderRepository, new Mock<IEmailService>().Object);
    }
    
    [Fact]
    public void ProcessOrder_ValidOrder_SuccessfullyProcesses()
    {
        // Utiliser le pattern builder
        var order = new OrderBuilder()
            .WithCustomerId("CUST001")
            .WithAmount(100m)
            .WithItem("PROD001", 10, 10m)
            .Build();
        
        // Pattern AAA avec étapes de style GWT
        // GIVEN
        _orderRepository.Save(order);
        
        // WHEN
        var result = _processor.Process(order);
        
        // THEN
        Assert.True(result.IsSuccessful);
        Assert.Equal(OrderStatus.Processed, result.Status);
    }
}
```

---

## Parcours d'Apprentissage

1. **Fondation** – Maîtriser les patterns AAA et GWT
2. **Efficacité** – Apprendre les builders et les fixtures
3. **Qualité** – Implémenter les tests de mutation et basés sur les propriétés
4. **Isolation** – Comprendre les doublures de test en profondeur
5. **Intégration** – Combiner les patterns pour les scénarios complexes

---

## Ressources Supplémentaires

- [Guide des Tests Unitaires](../GUIDES/unit-testing-guide-FR.md)
- [Guide des Tests d'Intégration](../GUIDES/integration-testing-guide-FR.md)
- [Guide des Tests BDD](../GUIDES/bdd-testing-guide-FR.md)
- [Documentation Moq](../PeasyPilot-Moq-FR.md)
- [Guide de Génération de Tests](../GUIDES/test-generation-guide-FR.md)

---

**[← Retour aux Sujets Avancés](./README.md)** | **[← Retour au Hub Documentation](../README.md)**

**Version:** Français | **[English](./testing-patterns.md)**
