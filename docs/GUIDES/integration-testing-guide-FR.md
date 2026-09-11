# Guide des Tests d'Intégration

## Aperçu

Les tests d'intégration vérifient que plusieurs composants fonctionnent ensemble correctement. Contrairement aux tests unitaires qui isolent le code, les tests d'intégration utilisent de vraies bases de données, APIs et services externes.

**Prérequis:** [Guide des Tests Unitaires](./unit-testing-guide-FR.md)  
**Durée:** 30 minutes  

---

## Pourquoi Ça Importe

Les tests unitaires capturent les erreurs de logique. Les tests d'intégration capturent les erreurs de configuration, les problèmes de BD et les interactions entre composants.

**Quand les utiliser:**
- Tester les couches d'accès aux données
- Tester les endpoints API
- Tester les workflows multi-composants
- Tester le code dépendant de la configuration

---

## Concepts Clés

### 1. Fixtures de Test

Les fixtures gèrent l'infrastructure de test (BDD, services, conteneurs DI) :

```csharp
using PeasyPilot.Integration.Fixtures;
using Xunit;

public class UserRepositoryIntegrationTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    }
    
    [Fact]
    public async Task AddUser_WithValidUser_Succeeds()
    {
        var repository = GetService<IUserRepository>();
        var user = new User { Name = "John", Email = "john@example.com" };
        
        await repository.AddAsync(user);
        
        var users = await repository.GetAllAsync();
        Assert.Single(users);
    }
}
```

### 2. Bases de Données In-Memory vs Réelles

**In-Memory** (rapide, pour les tests) :
```csharp
services.AddSingleton<ITestDatabase, InMemoryTestDatabase>();
```

**Réelle BD** (production-like) :
```csharp
services.AddSingleton<ITestDatabase>(provider =>
    new SqliteTestDatabase("Data Source=:memory:")
);
```

### 3. Réinitialisation Automatique

L'interface `IResettable` réinitialise les singletons entre les tests :

```csharp
public class InMemoryUserRepository : IUserRepository, IResettable
{
    private List<User> _users = new();
    
    public async Task ResetAsync()
    {
        _users.Clear();
        await Task.CompletedTask;
    }
}
```

---

## Exemples

### Exemple 1: Test d'Intégration Basique

```csharp
public class UserServiceIntegrationTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task CreateAndRetrieveUser_Succeeds()
    {
        var service = GetService<IUserService>();
        var user = new User { Name = "Alice", Email = "alice@example.com" };
        
        await service.CreateUserAsync(user);
        var retrieved = await service.GetUserAsync(user.Id);
        
        Assert.NotNull(retrieved);
        Assert.Equal("Alice", retrieved.Name);
    }
}
```

### Exemple 2: Multiple Tests avec Setup Partagé

```csharp
public class OrderServiceIntegrationTests : XUnitIntegrationTestFixture
{
    private IOrderService _orderService = null!;
    private IInventoryService _inventoryService = null!;
    
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IOrderService, OrderService>();
        services.AddSingleton<IInventoryService, InventoryService>();
    }
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _orderService = GetService<IOrderService>();
        _inventoryService = GetService<IInventoryService>();
    }
    
    [Fact]
    public async Task CreateOrder_ChecksInventory()
    {
        await _inventoryService.AddStockAsync(productId: 1, quantity: 100);
        
        var order = await _orderService.CreateOrderAsync(
            productId: 1, 
            quantity: 10
        );
        
        Assert.NotNull(order);
        var remaining = await _inventoryService.GetStockAsync(productId: 1);
        Assert.Equal(90, remaining);
    }
}
```

### Exemple 3: Tester les Transactions de BD

```csharp
[Fact]
public async Task TransferMoney_RollsBackOnFailure()
{
    var service = GetService<IAccountService>();
    var from = new Account { Balance = 100 };
    var to = new Account { Balance = 50 };
    
    var exception = await Assert.ThrowsAsync<InsufficientFundsException>(
        () => service.TransferAsync(from, to, amount: 150)
    );
    
    // Vérifier le rollback : les soldes inchangés
    Assert.Equal(100, from.Balance);
    Assert.Equal(50, to.Balance);
}
```

### Exemple 4: Test d'Intégration Web API

```csharp
public class UserApiIntegrationTests : XUnitIntegrationTestFixture
{
    private HttpClient _httpClient = null!;
    
    protected override void ConfigureServices(IServiceCollection services)
    {
        var factory = new WebApplicationFactory<Program>();
        _httpClient = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetUser_WithValidId_Returns200()
    {
        var response = await _httpClient.GetAsync("/api/users/1");
        
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("John", content);
    }
}
```

### Exemple 5: Réinitialisation Entre Tests

```csharp
public class UserRepositoryResetTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task FirstTest_AddsUser()
    {
        var repository = GetService<IUserRepository>();
        await repository.AddAsync(new User { Name = "Alice" });
        
        var users = await repository.GetAllAsync();
        Assert.Single(users);
    }
    
    [Fact]
    public async Task SecondTest_StartsClean()
    {
        var repository = GetService<IUserRepository>();
        
        // Réinitialisation automatique entre les tests
        var users = await repository.GetAllAsync();
        Assert.Empty(users);
    }
}
```

---

## Meilleures Pratiques

✅ **FAIRE**
- Utiliser les BDD in-memory pour la vitesse
- Réinitialiser l'état automatiquement
- Tester les configurations réelles
- Garder les tests d'intégration focalisés
- Mock les APIs externes

❌ **NE PAS FAIRE**
- Se connecter aux BDD de production
- Sauter le nettoyage entre tests
- Tester plusieurs workflows dans un seul test
- Ignorer les erreurs de setup
- Utiliser des délais figés

---

## Prochaines Étapes

📖 **[Guide BDD](./bdd-testing-guide-FR.md)** – Tester le comportement métier  
📖 **[Guide des Tests Unitaires](./unit-testing-guide-FR.md)** – Revoir les patterns unitaires  

Tests intégrés + tests unitaires = couverture complète! 🎯
