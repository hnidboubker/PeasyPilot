# Guide de Dépannage et Débogage

## Aperçu

Quand les tests échouent, trouver la cause racine rapidement est essentiel. Ce guide vous enseigne des techniques de débogage systématiques pour diagnostiquer et résoudre les défaillances de tests efficacement.

**Prérequis :** Compléter [Unit Testing Guide](../GUIDES/unit-testing-guide-FR.md) et [Integration Testing Guide](../GUIDES/integration-testing-guide-FR.md)  
**Temps estimé :** 45 minutes  
**Frameworks :** xUnit, NUnit, TUnit  
**Exemples de code :** 10 exemples fonctionnels

Dans ce guide vous allez apprendre :
- Comment identifier la cause racine des défaillances de tests
- Utilisation efficace du débogueur et des points d'arrêt
- Journalisation stratégique et diagnostics
- Analyse des traces de pile et des messages d'erreur
- Diagnostic des défaillances intermittentes et instables
- Débogage des problèmes de tests d'intégration
- Reproduction des défaillances CI/CD

---

## Partie 1 : Comprendre les Défaillances de Tests

### Types de Défaillances de Tests

Les tests peuvent échouer de plusieurs manières distinctes, chacune nécessitant des approches de débogage différentes :

1. **Défaillances d'assertions** – Écart entre la valeur attendue et actuelle
2. **Défaillances d'exceptions** – Exception inattendue levée
3. **Défaillances de timeout** – Le test dépasse la durée limite
4. **Défaillances de configuration/nettoyage** – Erreur d'initialisation ou nettoyage du test
5. **Défaillances instables** – Réussi parfois, échoue d'autres fois
6. **Défaillances d'environnement** – Dépendant de la configuration locale

### Workflow de Débogage

```
├─ Lire attentivement le message d'erreur
├─ Examiner la trace de pile
├─ Reproduire l'erreur localement
├─ Ajouter une journalisation de diagnostic
├─ Utiliser le débogueur (points d'arrêt, observations)
├─ Isoler la zone du problème
└─ Implémenter la correction
```

---

## Partie 2 : Débogage des Défaillances de Tests

### 2.1 Lecture des Messages d'Erreur

Le message d'erreur est votre premier indice. Analysez-le systématiquement :

**Exemple : Défaillance d'assertion**
```
Expected: "John Smith"
Actual:   "John "
```

Cela vous dit :
- L'assertion compare des chaînes
- La valeur réelle manque "Smith"
- Problème probable : Erreur dans la logique de trim ou parsing

**Exemple : Exception de référence nulle**
```
System.NullReferenceException: Object reference not set to an instance of an object.
  at UserService.GetUser (id=5) in UserService.cs:line 42
```

Cela vous dit :
- Un objet null est en cours de déréférencement
- Le problème est dans la méthode GetUser
- Problème probable : Vérification null manquante ou ID invalide

### 2.2 Analyse de la Trace de Pile

Les traces de pile montrent la séquence d'appels du point de défaillance au point d'entrée :

```csharp
// Code de test
[Fact]
public async Task CreateUser_WithValidData_SavesSuccessfully()
{
    var service = new UserService(_repository); // Line 15
    var user = await service.CreateUserAsync("John", "john@example.com"); // Line 16
    Assert.NotNull(user.Id);
}

// La trace de pile indique :
// at UserService.CreateUserAsync (UserService.cs:42)
// at UserRepositoryTests.CreateUser_WithValidData_SavesSuccessfully (UserRepositoryTests.cs:16)
```

**Points clés :**
- Ligne 42 dans UserService est où la défaillance réelle se produit
- Le test a appelé CreateUserAsync à la ligne 16
- Trace montre : UserService.CreateUserAsync → Repository.SaveAsync → DbContext.SaveChangesAsync

**Lecture de la trace :**
1. Trouvez le cadre le plus profond dans votre code (ignorez les cadres du framework)
2. Regardez le fichier et le numéro de ligne
3. Vérifiez quelle opération s'y déroulait
4. Remontez la pile pour voir ce qui l'a déclenchée

### 2.3 Utilisation Efficace du Débogueur

**Configuration des Points d'Arrêt :**

```csharp
[Fact]
public async Task UpdateUser_WithValidId_UpdatesName()
{
    // Arrange
    var service = new UserService(_repository);
    var user = await service.CreateUserAsync("John", "john@example.com");
    
    // Act – Placer un point d'arrêt sur la ligne suivante
    var updated = await service.UpdateUserAsync(user.Id, "Jane");
    
    // Assert
    Assert.Equal("Jane", updated.Name);
}
```

**Types de points d'arrêt :**
- **Point d'arrêt simple** – Arrêter l'exécution à la ligne
- **Point d'arrêt conditionnel** – Arrêter seulement si la condition est vraie : `id == 5`
- **Point d'arrêt de journal** – Imprimer le message sans arrêt

**Utilisation des observations :**
```
Watch: user.Id = 0 (devrait être > 0)
Watch: repository.GetUser(id) = null (devrait retourner User)
Watch: service._logger = null (dépendance non injectée)
```

---

## Partie 3 : Stratégies de Journalisation

### 3.1 Journalisation Structurée

Utilisez ILogger pour les diagnostics qui persistent quand les tests s'exécutent en CI :

```csharp
using Microsoft.Extensions.Logging;

public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _repository;
    
    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<User> CreateUserAsync(string name, string email)
    {
        _logger.LogInformation("Creating user: {Name} ({Email})", name, email);
        
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Email is empty for user: {Name}", name);
            throw new ArgumentException("Email is required");
        }
        
        try
        {
            var user = new User { Name = name, Email = email };
            var saved = await _repository.AddAsync(user);
            
            _logger.LogInformation("User created successfully: {UserId}", saved.Id);
            return saved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user: {Name} ({Email})", name, email);
            throw;
        }
    }
}
```

### 3.2 Niveaux de Journal et Filtrage

**Niveaux de Journal (ordonnés par gravité) :**
- **Trace** – Les plus détaillés, détails internes du framework
- **Debug** – Diagnostics de développement
- **Information** – Événements significatifs (démarrage, création, complétion)
- **Warning** – Conditions inhabituelles qui ne préviennent pas l'exécution
- **Error** – Erreurs récupérables
- **Critical** – Défaillances au niveau du système

**Configurer la journalisation dans les tests :**

```csharp
public class UserServiceTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureLogging(ILoggingBuilder logging)
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
        
        // Filtrer des namespaces spécifiques
        logging.AddFilter("UserService", LogLevel.Debug);
        logging.AddFilter("Repository", LogLevel.Information);
    }
    
    [Fact]
    public async Task CreateUser_WithLogging_TracesExecution()
    {
        var service = GetService<UserService>();
        
        // La sortie de débogage affichera :
        // [Information] Creating user: John (john@example.com)
        // [Debug] Validating email format
        // [Information] User created successfully: 123
        
        var user = await service.CreateUserAsync("John", "john@example.com");
        Assert.NotNull(user.Id);
    }
}
```

### 3.3 Capture de Contexte

Journalisez le contexte pertinent pour accélérer le débogage :

```csharp
public async Task<Order> CreateOrderAsync(int customerId, List<LineItem> items)
{
    var context = new Dictionary<string, object>
    {
        { "CustomerId", customerId },
        { "ItemCount", items.Count },
        { "TotalAmount", items.Sum(i => i.Price * i.Quantity) },
        { "Timestamp", DateTime.UtcNow }
    };
    
    using (_logger.BeginScope(context))
    {
        _logger.LogInformation("Processing order for customer");
        
        if (items.Count == 0)
        {
            _logger.LogWarning("Order has no items");
            throw new InvalidOperationException("Order must contain at least one item");
        }
        
        try
        {
            var order = new Order { CustomerId = customerId };
            return await _repository.SaveOrderAsync(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order");
            throw;
        }
    }
}
```

---

## Partie 4 : Défaillances de Tests Courantes

### 4.1 Tests Instables (Problèmes de Timing)

**Problème :** Le test réussit parfois, échoue d'autres fois.

**Causes courantes :**
- Opérations asynchrones pas correctement attendues
- Conditions de concurrence en code multithéadé
- Timeouts de service externe
- Dépendances de l'heure système

**Exemple de test instable :**

```csharp
// ❌ MAUVAIS : Instable – n'attend pas l'opération asynchrone
[Fact]
public async Task ProcessOrder_CompletesQuickly()
{
    var service = new OrderProcessingService();
    
    service.ProcessOrderAsync(orderId: 1); // Tirer et oublier
    
    var status = await service.GetStatusAsync(1);
    Assert.Equal("Completed", status); // Peut échouer si pas prêt
}

// ✅ BON : Attendre la complétion
[Fact]
public async Task ProcessOrder_CompletesQuickly()
{
    var service = new OrderProcessingService();
    
    await service.ProcessOrderAsync(orderId: 1); // Attendre la complétion
    
    var status = await service.GetStatusAsync(1);
    Assert.Equal("Completed", status);
}
```

**Une autre source courante – attentes de timing :**

```csharp
// ❌ MAUVAIS : Délais codés en dur
[Fact]
public async Task BackgroundJob_ExecutesWithinTimeout()
{
    var job = new BackgroundJob();
    job.Start();
    
    await Task.Delay(100); // Peut ne pas être assez sur machines lentes
    
    Assert.True(job.IsComplete);
}

// ✅ BON : Attendre avec assertion de timeout
[Fact]
public async Task BackgroundJob_ExecutesWithinTimeout()
{
    var job = new BackgroundJob();
    job.Start();
    
    var task = job.WaitForCompletionAsync();
    var completed = await task.ConfigureAwait(false);
    var timeout = TimeSpan.FromSeconds(5);
    
    Assert.True(completed.IsCompletedSuccessfully, 
        "Job should complete within timeout");
}
```

### 4.2 Problèmes d'Isolation des Données

**Problème :** Les tests interfèrent les uns avec les autres, partageant l'état de la base de données.

**Exemple d'échec d'isolation des données :**

```csharp
// ❌ MAUVAIS : Les tests partagent les données
[Collection("Database collection")]
public class UserRepositoryTests
{
    private static readonly List<User> _users = new(); // État partagé !
    
    [Fact]
    public void Test1_CreateUser_Succeeds()
    {
        _users.Add(new User { Id = 1, Name = "John" });
        Assert.Single(_users); // Réussit
    }
    
    [Fact]
    public void Test2_CreateAnotherUser_Succeeds()
    {
        _users.Add(new User { Id = 2, Name = "Jane" });
        Assert.Single(_users); // Échoue si Test1 s'exécute d'abord !
    }
}

// ✅ BON : Tests isolés
public class UserRepositoryTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    }
    
    [Fact]
    public async Task Test1_CreateUser_Succeeds()
    {
        var repo = GetService<IUserRepository>();
        await repo.AddAsync(new User { Name = "John" });
        
        var users = await repo.GetAllAsync();
        Assert.Single(users);
    }
    
    [Fact]
    public async Task Test2_CreateAnotherUser_Succeeds()
    {
        var repo = GetService<IUserRepository>();
        await repo.AddAsync(new User { Name = "Jane" });
        
        var users = await repo.GetAllAsync();
        Assert.Single(users); // État frais pour chaque test
    }
}
```

### 4.3 Problèmes Async/Await

**Problème :** Code synchrone bloquant les opérations asynchrones.

```csharp
// ❌ MAUVAIS : Blocage d'appel asynchrone avec .Result
[Fact]
public void GetUser_WithValidId_ReturnsUser()
{
    var service = new UserService();
    var user = service.GetUserAsync(1).Result; // Cause un interblocage
    Assert.NotNull(user);
}

// ✅ BON : Attendre correctement
[Fact]
public async Task GetUser_WithValidId_ReturnsUser()
{
    var service = new UserService();
    var user = await service.GetUserAsync(1);
    Assert.NotNull(user);
}
```

**Exemple : ConfigureAwait manquant**

```csharp
// ❌ Problème potentiel dans le code de bibliothèque
public async Task<User> GetUserAsync(int id)
{
    var user = await _repository.GetAsync(id); // Ne pas capturer le contexte
    return user;
}

// ✅ BON : Utiliser ConfigureAwait(false) dans le code de bibliothèque
public async Task<User> GetUserAsync(int id)
{
    var user = await _repository.GetAsync(id).ConfigureAwait(false);
    return user;
}
```

---

## Partie 5 : Outils et Techniques de Diagnostic

### 5.1 Assistants d'Assertion

Créer des assistants pour fournir de meilleurs messages d'erreur :

```csharp
public static class AssertionHelpers
{
    public static void AssertUserEqual(User expected, User actual, string? message = null)
    {
        var failures = new List<string>();
        
        if (expected.Id != actual.Id)
            failures.Add($"Id: expected {expected.Id}, got {actual.Id}");
        if (expected.Name != actual.Name)
            failures.Add($"Name: expected '{expected.Name}', got '{actual.Name}'");
        if (expected.Email != actual.Email)
            failures.Add($"Email: expected '{expected.Email}', got '{actual.Email}'");
        
        if (failures.Any())
        {
            var details = string.Join(Environment.NewLine, failures);
            var fullMessage = message == null
                ? details
                : $"{message}:{Environment.NewLine}{details}";
            throw new Xunit.Sdk.XunitException(fullMessage);
        }
    }
}

// Utilisation
[Fact]
public async Task CreateUser_StoresAllFields()
{
    var service = new UserService(_repository);
    var expected = new User { Id = 1, Name = "John", Email = "john@example.com" };
    
    var actual = await service.CreateUserAsync("John", "john@example.com");
    
    AssertionHelpers.AssertUserEqual(expected, actual, "User data mismatch");
}
```

### 5.2 Profilage Mémoire pour les Tests

Détecter les fuites mémoire dans les tests :

```csharp
public class MemoryLeakDetectionTests
{
    [Fact]
    public void Service_DoesNotLeakMemory()
    {
        var initialMemory = GC.GetTotalMemory(true);
        
        // Allouer et disposer de nombreux objets
        for (int i = 0; i < 1000; i++)
        {
            var service = new UserService(_repository);
            service.ProcessUser(i);
            // Le service devrait être nettoyé
        }
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(false);
        var increase = finalMemory - initialMemory;
        
        // La mémoire ne devrait pas augmenter significativement (< 10MB pour 1000 itérations)
        Assert.True(increase < 10_000_000, 
            $"Memory leak detected: {increase / 1024.0 / 1024.0:F2} MB increase");
    }
}
```

### 5.3 Analyse de la Sécurité des Threads

Vérifier les problèmes de sécurité des threads :

```csharp
public class ThreadSafetyTests
{
    [Fact]
    public void Service_IsThreadSafe()
    {
        var service = new UserService(_repository);
        var errors = new List<Exception>();
        var tasks = new List<Task>();
        
        // Simuler l'accès concurrent de 10 threads
        for (int i = 0; i < 10; i++)
        {
            int userId = i;
            var task = Task.Run(async () =>
            {
                try
                {
                    await service.CreateUserAsync($"User{userId}", $"user{userId}@example.com");
                    await service.UpdateUserAsync(userId, $"Updated{userId}");
                    await service.DeleteUserAsync(userId);
                }
                catch (Exception ex)
                {
                    lock (errors)
                    {
                        errors.Add(ex);
                    }
                }
            });
            
            tasks.Add(task);
        }
        
        Task.WaitAll(tasks.ToArray());
        
        Assert.Empty(errors); // Pas d'exceptions de sécurité des threads
    }
}
```

---

## Partie 6 : Débogage des Tests d'Intégration

### 6.1 Problèmes d'État de la Base de Données

**Problème :** L'état de la base de données persiste entre les tests.

```csharp
public class OrderIntegrationTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITestDatabase, InMemoryTestDatabase>();
        services.AddSingleton<IOrderRepository, OrderRepository>();
    }
    
    [Fact]
    public async Task CreateOrder_WithExistingData_Succeeds()
    {
        var db = GetService<ITestDatabase>();
        var repo = GetService<IOrderRepository>();
        
        // Setup : Créer les données initiales
        await db.InitializeAsync();
        
        // Vérifier l'état propre
        var initialOrders = await repo.GetAllAsync();
        Assert.Empty(initialOrders);
        
        // Act
        var order = new Order { CustomerId = 1, Total = 100 };
        await repo.AddAsync(order);
        
        // Assert
        var allOrders = await repo.GetAllAsync();
        Assert.Single(allOrders);
    }
    
    // Chaque test obtient un état de base de données frais
    [Fact]
    public async Task CreateMultipleOrders_Succeeds()
    {
        var repo = GetService<IOrderRepository>();
        
        // État frais – les données du test précédent sont parties
        var initialOrders = await repo.GetAllAsync();
        Assert.Empty(initialOrders);
        
        // Act
        for (int i = 1; i <= 3; i++)
        {
            await repo.AddAsync(new Order { CustomerId = i, Total = i * 100 });
        }
        
        // Assert
        var allOrders = await repo.GetAllAsync();
        Assert.Equal(3, allOrders.Count);
    }
}
```

### 6.2 Problèmes de Nettoyage des Ressources

**Problème :** Les ressources ne sont pas correctement nettoyées, bloquant les tests suivants.

```csharp
// ❌ MAUVAIS : Ressource non nettoyée
[Fact]
public async Task Service_AccessesFile()
{
    var filePath = "test.txt";
    var fileWriter = new FileWriter(filePath);
    
    await fileWriter.WriteAsync("data");
    // Le fichier est verrouillé, le test suivant peut échouer
}

// ✅ BON : Nettoyage explicite
[Fact]
public async Task Service_AccessesFile()
{
    var filePath = "test.txt";
    var fileWriter = new FileWriter(filePath);
    
    try
    {
        await fileWriter.WriteAsync("data");
    }
    finally
    {
        fileWriter.Dispose(); // Assurer le nettoyage
    }
}

// ✅ MEILLEUR : Utiliser IAsyncDisposable
[Fact]
public async Task Service_AccessesFile()
{
    var filePath = "test.txt";
    await using var fileWriter = new FileWriter(filePath);
    
    await fileWriter.WriteAsync("data");
    // Automatiquement disposé
}
```

---

## Partie 7 : Reproduction des Défaillances CI/CD

### 7.1 Reproduction des Défaillances CI Localement

Quand un test échoue en CI mais réussit localement :

**Étape 1 : Vérifier les variables d'environnement**
```csharp
[Fact]
public void Test_ReadsConfiguration()
{
    var apiUrl = Environment.GetEnvironmentVariable("API_URL");
    var apiKey = Environment.GetEnvironmentVariable("API_KEY");
    
    Assert.NotNull(apiUrl);
    Assert.NotNull(apiKey);
}
```

**Étape 2 : Utiliser le même filtre de test que CI**
```bash
# Si CI exécute : dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Integration"

# Correspondre aux versions exactes du framework
dotnet test --framework net8.0
```

**Étape 3 : Vérifier la sortie de journalisation**
```csharp
protected override void ConfigureLogging(ILoggingBuilder logging)
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
}
```

### 7.2 Problèmes Spécifiques à CI Courants

**Problème : Différences de fuseau horaire**
```csharp
// ❌ MAUVAIS : Suppose un fuseau horaire spécifique
var now = DateTime.Now;
var tomorrow = now.AddDays(1);

// ✅ BON : Utiliser UTC
var now = DateTime.UtcNow;
var tomorrow = now.AddDays(1);
```

**Problème : Conflits d'exécution parallèle des tests**
```csharp
// Utiliser les noms de collection pour sérialiser les tests
[Collection("Database collection")]
public class Test1 { }

[Collection("Database collection")]
public class Test2 { }

// Les tests dans la même collection s'exécutent séquentiellement
```

**Problème : Dépendances manquantes**
```csharp
[Fact]
public async Task Service_WorksWithExternalApi()
{
    // Vérifier que les services requis sont disponibles
    var apiClient = GetService<IExternalApiClient>();
    Assert.NotNull(apiClient);
    
    // Peut échouer en CI si le service externe est hors ligne
    try
    {
        var result = await apiClient.CallAsync();
        Assert.NotNull(result);
    }
    catch (HttpRequestException)
    {
        // Documenter la dépendance externe
        Assert.Skip("External API unavailable");
    }
}
```

---

## Partie 8 : Workflow de Débogage Systématique

### Exemple Complet : Débogage d'un Test Échoué

**Scénario :** Le test échoue intermittently en CI.

**Étape 1 : Collecter les Informations**
```csharp
[Fact]
public async Task ProcessOrder_WithLogging()
{
    var logger = GetService<ILogger<OrderService>>();
    var service = GetService<OrderService>();
    
    // Ajouter une journalisation de contexte
    logger.LogInformation("Test start: ProcessOrder_WithLogging");
    logger.LogInformation("Environment: {Env}", 
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
    
    var order = new Order { CustomerId = 1, Items = new[] { "Item1", "Item2" } };
    logger.LogInformation("Order created with {ItemCount} items", order.Items.Length);
    
    var result = await service.ProcessAsync(order);
    
    logger.LogInformation("Result: {Status}", result.Status);
    Assert.Equal("Completed", result.Status);
}
```

**Étape 2 : Ajouter un Point d'Arrêt Conditionnel**
```
Point d'arrêt sur : Appel ProcessAsync
Condition : order.Items.Length == 0
```

**Étape 3 : Reproduire dans Différents Scénarios**
- Exécuter un seul test plusieurs fois
- Exécuter avec d'autres tests (vérifier l'isolation)
- Exécuter sur une machine différente (vérifier l'environnement)
- Exécuter avec une version spécifique du framework

**Étape 4 : Identifier le Motif**
- Échoue-t-il en CI seulement ?
- Échoue-t-il dans un ordre spécifique ?
- Échoue-t-il sous charge ?

**Étape 5 : Implémenter la Correction**
```csharp
public async Task<OrderResult> ProcessAsync(Order order)
{
    if (order.Items.Length == 0)
    {
        _logger.LogWarning("Order has no items");
        throw new InvalidOperationException("Order must have items");
    }
    
    // Reste de l'implémentation...
}
```

**Étape 6 : Valider la Correction**
- Exécuter le test 10x localement
- Exécuter la suite de tests complète
- Exécuter en CI
- Surveiller une régression

---

## Bonnes Pratiques

### À Faire ✅

- **À faire** que les noms de test décrivent ce qui est testé
- **À faire** utiliser la journalisation structurée avec contexte
- **À faire** isoler les tests pour prévenir l'interférence
- **À faire** attendre correctement les opérations asynchrones
- **À faire** nettoyer les ressources dans les blocs finally
- **À faire** tester les cas limites et conditions d'erreur
- **À faire** utiliser des assertions spécifiques avec messages clairs
- **À faire** documenter les dépendances externes et timeouts

### À Ne Pas Faire ❌

- **À ne pas faire** ignorer les défaillances de tests – investiguer immédiatement
- **À ne pas faire** utiliser des délais codés au lieu d'une attente appropriée
- **À ne pas faire** partager l'état entre les tests
- **À ne pas faire** bloquer le code asynchrone avec .Result ou .Wait()
- **À ne pas faire** attraper et avaler les exceptions
- **À ne pas faire** supposer l'ordre d'exécution des tests
- **À ne pas faire** ignorer les tests instables – corriger la cause racine
- **À ne pas faire** committer du code avec des tests ignorés

---

## Résumé

Le débogage efficace nécessite une réflexion systématique :

1. **Lire attentivement** – Les messages d'erreur contiennent des indices
2. **Isoler** – Reproduire dans le cas le plus petit possible
3. **Journaliser stratégiquement** – Capturer le contexte et le flux
4. **Utiliser les outils** – Débogueur, observations, profileur mémoire
5. **Penser systématiquement** – Qu'est-ce qui a changé ? Qu'est-ce qui est différent en CI ?
6. **Corriger la cause racine** – Pas seulement le symptôme
7. **Valider** – S'assurer que la correction fonctionne de manière cohérente

---

## Prochaines Étapes

- Lire [Performance & Optimization Guide](./performance-optimization-FR.md)
- Apprendre [Advanced Testing Patterns](./testing-patterns-FR.md)
- Explorer [Migration & Upgrade Guide](./migration-upgrade-FR.md)

[← Retour à la Documentation](../README.md)
