# Guide de Performance et Optimisation

## Aperçu

Dans ce guide, vous apprendrez à :
- Concevoir des tests pour l'exécution parallèle et le débit maximal
- Optimiser les motifs d'isolation et de nettoyage des tests
- Implémenter des stratégies de cache et de mémoïsation
- Choisir entre les bases de données en mémoire et réelles
- Gérer la mémoire et détecter les fuites
- Mesurer et benchmarker la performance des tests
- Mettre à l'échelle votre suite de tests au fur et à mesure qu'elle grandit

**Prérequis :** Complétez le [Guide de Test d'Intégration](../GUIDES/integration-testing-guide-FR.md)  
**Durée estimée :** 45 minutes  
**Frameworks :** xUnit, NUnit, TUnit  
**Exemples de code :** 12 exemples fonctionnels

---

## Pourquoi la Performance Compte

Les suites de tests croissent au fil du temps. Ce qui s'exécute en 5 secondes aujourd'hui devient 50 secondes en un mois. À mesure que le temps d'exécution des tests augmente :

- Les boucles de rétroaction des développeurs ralentissent (attente plus longue pour le feu vert/rouge)
- Les pipelines CI/CD deviennent des goulots d'étranglement
- La couverture de test diminue (les développeurs ignorent les tests lents)
- Les développeurs laissent les tests non découverts, créant des lacunes

Une suite de tests bien optimisée est :
- **Rapide** – Complétée en secondes, pas en minutes
- **Scalable** – Ajoute 10 % de surcharge pour 10 % de nouveaux tests, pas exponentielle
- **Prévisible** – Même durée indépendamment de l'ordre des tests
- **Efficace** – Utilise la mémoire intelligemment, gaspillage de ressources minimal
- **Mesurable** – Lignes de base de performance claires et régressions suivies

---

## Concepts Fondamentaux

### 1. Stratégies d'Exécution Parallèle

L'exécution parallèle des tests est l'optimisation la plus impactante. L'orchestration PeasyPilot supporte le parallélisme configurable :

```csharp
// xUnit – Exécution parallèle intégrée
[CollectionDefinition("Séquentiel")]
public class SequentialCollection { }

[Collection("Séquentiel")]
public class DatabaseTests
{
    // Ces tests s'exécutent séquentiellement pour éviter les contentions de base de données
}

// Les tests restants s'exécutent en parallèle par défaut
[Fact]
public void UnitTest_RunsInParallel() { }
```

**Principes clés :**
- **Tests liés au CPU** (calculs, analyse, logique) se parallélisent bien
- **Tests liés à l'I/O** (base de données, appels API) se parallélisent modérément
- **Tests avec état** (état partagé, ordre-dépendant) doivent être sérialisés
- **DegreeOfParallelism** doit correspondre à (cœurs CPU / 2) pour la stabilité

### 2. Motifs d'Isolation et de Nettoyage des Tests

Le nettoyage approprié prévient la pollution des tests et les défaillances en cascade :

```csharp
// Mauvais : État partagé sans nettoyage
public class BadIsolationTests
{
    private static List<User> _users = new(); // ❌ État mutable partagé
    
    [Fact]
    public void Test1_AddsUser()
    {
        _users.Add(new User { Id = 1, Name = "Alice" });
        Assert.Single(_users);
    }
    
    [Fact]
    public void Test2_ChecksUsers()
    {
        // ❌ Test1 peut s'être exécuté en premier, _users contient Alice
        Assert.Empty(_users); // Flaky!
    }
}

// Bon : Isolation appropriée avec IResettable
public class GoodIsolationTests : XUnitIntegrationTestFixture
{
    private InMemoryUserRepository _repository = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = GetService<InMemoryUserRepository>();
    }
    
    [Fact]
    public async Task Test1_AddsUser()
    {
        await _repository.AddAsync(new User { Id = 1, Name = "Alice" });
        var users = await _repository.GetAllAsync();
        Assert.Single(users);
    }
    
    [Fact]
    public async Task Test2_StartsEmpty()
    {
        // ✅ Réinitialisation automatique du référentiel via IResettable
        var users = await _repository.GetAllAsync();
        Assert.Empty(users);
    }
}
```

### 3. Stratégies de Cache et de Mémoïsation

La découverte de liaison de pas et la correspondance de motifs sont coûteuses. Mettez-les en cache :

```csharp
// Sans cache : O(N) réflexion par scénario
public class SlowStepBindingResolver : IStepBindingResolver
{
    public StepBinding? ResolveBinding(string stepText)
    {
        // Analyse tout l'assembly pour les attributs [Given]/[When]/[Then]
        // Chaque exécution de scénario paie ce coût
        var bindings = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(BddStepDefinition).IsAssignableFrom(t))
            .SelectMany(t => GetMethodBindings(t));
        
        return bindings.FirstOrDefault(b => b.Matches(stepText));
    }
}

// Avec cache : O(1) après la première découverte
public class FastStepBindingResolver : IStepBindingResolver
{
    private readonly Dictionary<string, StepBinding> _bindingCache = new();
    private bool _initialized = false;
    
    public StepBinding? ResolveBinding(string stepText)
    {
        if (!_initialized)
        {
            _InitializeCache();
            _initialized = true;
        }
        
        return _bindingCache.TryGetValue(stepText, out var binding) 
            ? binding 
            : _FindPatternMatch(stepText);
    }
    
    private void _InitializeCache()
    {
        var bindings = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(BddStepDefinition).IsAssignableFrom(t))
            .SelectMany(t => GetMethodBindings(t));
        
        foreach (var binding in bindings)
        {
            _bindingCache[binding.Pattern] = binding;
        }
    }
    
    private StepBinding? _FindPatternMatch(string stepText)
    {
        // Cache miss : vérifier les motifs regex
        return _bindingCache.Values
            .FirstOrDefault(b => b.PatternRegex.IsMatch(stepText));
    }
}
```

**Impact de la performance :** 50ms → 1ms par découverte de scénario (50x plus rapide)

### 4. Compromis d'Optimisation de Base de Données

| Aspect | EnMémoire | SQLite | BD Réelle |
|--------|----------|--------|-----------|
| Vitesse | 1ms/op | 5ms/op | 50ms/op |
| Isolation | Automatique | Manuel | Manuel |
| Schéma Réel | ❌ | ✅ | ✅ |
| Transactions | ❌ | ✅ | ✅ |
| Temps de Configuration | 10ms | 100ms | 500ms |
| Nombre de Tests | 10,000+ | 1,000+ | 100+ |

**Stratégie :**

```csharp
public class OptimizedDatabaseChoice
{
    // Tests unitaires : En mémoire uniquement
    public class CalculatorTests : XUnitIntegrationTestFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITestDatabase, InMemoryTestDatabase>();
        }
        
        [Fact]
        public async Task Calculate_IsInstant()
        {
            var calculator = GetService<Calculator>();
            // S'exécute en <1ms
        }
    }
    
    // Tests d'intégration : Utiliser la base de données réelle pour les chemins critiques
    [CollectionDefinition("Base de Données")]
    public class DatabaseCollection
    {
        // Séquentiel pour éviter les contentions
    }
    
    [Collection("Base de Données")]
    public class PaymentProcessingTests : XUnitIntegrationTestFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            // Utiliser la base de données réelle pour le traitement des paiements
            services.AddSingleton<ITestDatabase>(provider =>
                new SqlTestDatabase("Server=.; Database=PaymentTest;")
            );
        }
        
        [Fact]
        public async Task ProcessPayment_WithValidCard_Succeeds()
        {
            var processor = GetService<PaymentProcessor>();
            // Test de transaction de base de données réelle
        }
    }
}
```

---

## Motifs d'Optimisation

### Motif 1 : Exécution Parallèle avec Contrôle du Degré

```csharp
public class ParallelExecutionOptimizer
{
    public static int CalculateDegreeOfParallelism()
    {
        // Règle : Utiliser (cœurs CPU / 2) pour la stabilité
        // Conservateur : Laisse de la place aux tâches système
        int cores = Environment.ProcessorCount;
        return Math.Max(1, cores / 2);
    }
}

// Dans la configuration des tests
public class TestConfiguration
{
    public static void ConfigureParallelism(TestOrchestrator orchestrator)
    {
        int degree = ParallelExecutionOptimizer.CalculateDegreeOfParallelism();
        orchestrator.SetParallelDegree(degree);
    }
}
```

### Motif 2 : Stratégie de Réinitialisation Singleton

```csharp
public interface IResettable
{
    Task ResetAsync();
}

// Référentiel qui a besoin d'une réinitialisation entre les tests
public class InMemoryUserRepository : IUserRepository, IResettable
{
    private List<User> _users = new();
    private Dictionary<int, User> _cache = new();
    
    public async Task ResetAsync()
    {
        _users.Clear();
        _cache.Clear();
        await Task.CompletedTask;
    }
}

// La fixture d'intégration réinitialise automatiquement
public class UserRepositoryTests : XUnitIntegrationTestFixture
{
    private IUserRepository _repository = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = GetService<IUserRepository>();
        // La fixture appelle automatiquement ResetAsync() sur les services IResettable
    }
    
    [Fact]
    public async Task EachTestStartsClean()
    {
        var users = await _repository.GetAllAsync();
        Assert.Empty(users); // ✅ État propre
    }
}
```

### Motif 3 : Découverte de Pas Mémoïsée

```csharp
public class MemoizedStepDiscovery
{
    private static readonly Lazy<Dictionary<string, MethodInfo>> StepCache = 
        new(() => DiscoverSteps());
    
    public static Dictionary<string, MethodInfo> GetSteps() => StepCache.Value;
    
    private static Dictionary<string, MethodInfo> DiscoverSteps()
    {
        var steps = new Dictionary<string, MethodInfo>();
        
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(BddStepDefinition).IsAssignableFrom(t)))
        {
            foreach (var method in type.GetMethods())
            {
                var attr = method.GetCustomAttribute<GivenAttribute>();
                if (attr != null)
                {
                    steps[attr.Pattern] = method;
                }
            }
        }
        
        return steps;
    }
}
```

### Motif 4 : Regroupement de Connexions pour les Bases de Données Réelles

```csharp
public class OptimizedDatabaseFactory : ITestDatabaseFactory
{
    private static readonly Lazy<SqlConnectionPool> _pool = 
        new(() => new SqlConnectionPool(connectionString: "..."));
    
    public async Task<ITestDatabase> CreateAsync()
    {
        // Réutilise les connexions regroupées au lieu d'en créer de nouvelles
        var connection = await _pool.Value.AcquireAsync();
        return new SqlTestDatabase(connection);
    }
}

public class SqlConnectionPool
{
    private readonly Queue<SqlConnection> _available = new();
    private readonly int _maxSize = 10;
    private readonly string _connectionString;
    
    public SqlConnectionPool(string connectionString)
    {
        _connectionString = connectionString;
        _InitializePool();
    }
    
    private void _InitializePool()
    {
        for (int i = 0; i < _maxSize / 2; i++) // Commencer avec 50% de capacité
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            _available.Enqueue(conn);
        }
    }
    
    public async Task<SqlConnection> AcquireAsync()
    {
        lock (_available)
        {
            if (_available.Count > 0)
            {
                return _available.Dequeue();
            }
        }
        
        var newConn = new SqlConnection(_connectionString);
        await newConn.OpenAsync();
        return newConn;
    }
    
    public void Release(SqlConnection connection)
    {
        if (connection.State == System.Data.ConnectionState.Open)
        {
            lock (_available)
            {
                if (_available.Count < _maxSize)
                {
                    _available.Enqueue(connection);
                }
                else
                {
                    connection.Dispose();
                }
            }
        }
    }
}
```

---

## Gestion de la Mémoire

### Profilage de la Mémoire

```csharp
public class MemoryOptimizedTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task TestDoesNotLeakMemory()
    {
        var beforeGC = GC.GetTotalMemory(false);
        
        // Exécuter l'opération de test
        for (int i = 0; i < 1000; i++)
        {
            var repo = GetService<IRepository>();
            var items = await repo.GetAllAsync();
            Assert.NotEmpty(items);
        }
        
        // Forcer la collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var afterGC = GC.GetTotalMemory(false);
        
        // La mémoire ne doit pas augmenter significativement
        var leakSuspicion = (afterGC - beforeGC) / (double)beforeGC;
        Assert.True(leakSuspicion < 0.1, $"Memory grew {leakSuspicion:P}"); // <10% de croissance
    }
}
```

### Motif de Regroupement d'Objets

```csharp
public class TestDataBuilder<T> where T : new()
{
    private static readonly ObjectPool<List<T>> _listPool = 
        new(() => new List<T>(), 
            list => { list.Clear(); return list; },
            poolSize: 10);
    
    public List<T> BuildTestList(int count)
    {
        var list = _listPool.Get();
        
        for (int i = 0; i < count; i++)
        {
            list.Add(new T());
        }
        
        return list;
    }
    
    public void ReturnTestList(List<T> list)
    {
        _listPool.Return(list);
    }
}

public class ObjectPool<T>
{
    private readonly Stack<T> _pool = new();
    private readonly Func<T> _factory;
    private readonly Func<T, T> _reset;
    private readonly int _maxSize;
    
    public ObjectPool(Func<T> factory, Func<T, T> reset, int poolSize = 10)
    {
        _factory = factory;
        _reset = reset;
        _maxSize = poolSize;
    }
    
    public T Get() => _pool.Count > 0 ? _pool.Pop() : _factory();
    
    public void Return(T item)
    {
        if (_pool.Count < _maxSize)
        {
            _reset(item);
            _pool.Push(item);
        }
    }
}
```

---

## Benchmarking

### Mesure de la Performance

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class StepBindingBenchmarks
{
    private IStepBindingResolver _slowResolver = null!;
    private IStepBindingResolver _fastResolver = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _slowResolver = new SlowStepBindingResolver();
        _fastResolver = new FastStepBindingResolver();
    }
    
    [Benchmark(Baseline = true)]
    public void SlowResolve()
    {
        for (int i = 0; i < 100; i++)
        {
            _slowResolver.ResolveBinding("Given a user with email test@example.com");
            _slowResolver.ResolveBinding("When the user logs in");
            _slowResolver.ResolveBinding("Then the dashboard is displayed");
        }
    }
    
    [Benchmark]
    public void FastResolve()
    {
        for (int i = 0; i < 100; i++)
        {
            _fastResolver.ResolveBinding("Given a user with email test@example.com");
            _fastResolver.ResolveBinding("When the user logs in");
            _fastResolver.ResolveBinding("Then the dashboard is displayed");
        }
    }
}

// Exécuter les benchmarks
public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<StepBindingBenchmarks>();
    }
}
```

**Exemple de sortie :**
```
| Méthode    | Moyenne  | EcartType | Gen0 | Gen1 | Gen2 | Alloué  |
|-----------|----------|-----------|------|------|------|---------|
| SlowResolve   | 45.32 ms | 2.10 ms   | 40.0 | 2.0  | 0.5  | 125 KB  |
| FastResolve   | 0.92 ms  | 0.08 ms   | 0.0  | 0.0  | 0.0  | 0 KB    |
```

### Test de Régression de Performance

```csharp
[TestFixture]
public class PerformanceRegressionTests
{
    private const int MaxAllowedMilliseconds = 100;
    
    [Test]
    public async Task ScenarioExecution_MustCompleteWithin_MaxAllowedTime()
    {
        var executor = new ScenarioExecutor();
        var scenario = LoadTestScenario("large_workflow.feature");
        
        var stopwatch = Stopwatch.StartNew();
        var result = await executor.ExecuteAsync(scenario);
        stopwatch.Stop();
        
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(MaxAllowedMilliseconds),
            $"Scenario took {stopwatch.ElapsedMilliseconds}ms (baseline: {MaxAllowedMilliseconds}ms). " +
            $"Possible regression detected.");
    }
    
    private Feature LoadTestScenario(string path)
    {
        // Charger le fichier de feature...
        return new Feature();
    }
}
```

---

## Scénarios Avancés

### Scénario 1 : Mise à l'Échelle des Suites de Tests BDD

À mesure que les fichiers de feature BDD se développent, la découverte de pas devient un goulot d'étranglement. Voici comment optimiser :

```csharp
// Avant : 500 features × 5 scénarios chacun = 2 500 résolutions de pas
// Temps : 50ms/pas = 125 secondes ! ❌

public class BddScalabilityOptimization
{
    // Solution : Initialisation Lazy + mise en cache catégorisée
    public class CategoryizedStepRegistry
    {
        private readonly Dictionary<string, StepBinding[]> _categoryCache = new();
        private readonly Lazy<StepBinding[]> _allBindings;
        
        public CategoryizedStepRegistry()
        {
            _allBindings = new(() => DiscoverAllBindings());
        }
        
        public StepBinding? FindBinding(string stepText)
        {
            // Extraire l'indice de catégorie (ex. "Given a USER exists" → "USER")
            var category = ExtractCategory(stepText);
            
            if (_categoryCache.TryGetValue(category, out var cached))
            {
                return FindInCategory(cached, stepText);
            }
            
            // Premier accès : mettre en cache cette catégorie
            var categoryBindings = _allBindings.Value
                .Where(b => b.BelongsToCategory(category))
                .ToArray();
            
            _categoryCache[category] = categoryBindings;
            return FindInCategory(categoryBindings, stepText);
        }
        
        private StepBinding? FindInCategory(StepBinding[] bindings, string stepText)
        {
            return bindings.FirstOrDefault(b => b.Matches(stepText));
        }
        
        private string ExtractCategory(string stepText)
        {
            // Extraire le premier nom comme catégorie : "Given a USER exists" → "USER"
            var words = stepText.Split(' ');
            return words.FirstOrDefault(w => w.Length > 3)?.ToUpper() ?? "DEFAULT";
        }
        
        private StepBinding[] DiscoverAllBindings()
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(BddStepDefinition).IsAssignableFrom(t))
                .SelectMany(t => t.GetMethods()
                    .Where(m => m.GetCustomAttribute<StepAttribute>() != null)
                    .Select(m => new StepBinding(m)))
                .ToArray();
        }
    }
}

// Résultat : 50ms → 5ms par 500 scénarios (amélioration 10x) ✅
```

### Scénario 2 : Test de Stress avec Limites de Mémoire

```csharp
public class MemoryStressTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task CreateThousandUsers_WithinMemoryBudget()
    {
        var repo = GetService<IRepository>();
        long initialMemory = GC.GetTotalMemory(true);
        const long maxMemoryIncrease = 10_000_000; // Budget de 10 MB
        
        // Créer les utilisateurs par lots pour mesurer la mémoire incrémentale
        for (int batch = 0; batch < 10; batch++)
        {
            for (int i = 0; i < 100; i++)
            {
                var user = new User { Id = batch * 100 + i, Name = $"User{i}" };
                await repo.AddAsync(user);
            }
            
            GC.Collect();
            long currentMemory = GC.GetTotalMemory(false);
            long memoryUsed = currentMemory - initialMemory;
            
            // Chaque lot doit utiliser une mémoire prévisible
            Assert.True(
                memoryUsed < maxMemoryIncrease,
                $"Batch {batch} memory usage: {memoryUsed / 1024.0 / 1024.0:F2} MB"
            );
        }
    }
}
```

### Scénario 3 : Pièges de Parallélisation Async/Await

```csharp
// ❌ MAUVAIS : Semble parallèle, mais sérialise à l'attente
public class DeceptiveParallelism
{
    [Fact]
    public async Task Slow_FakeParallel()
    {
        var service = GetService<IService>();
        
        // Toutes les tâches créées mais attendues séquentiellement
        var task1 = service.FetchDataAsync(1);
        var task2 = service.FetchDataAsync(2);
        var task3 = service.FetchDataAsync(3);
        
        var result1 = await task1; // Attendre 1
        var result2 = await task2; // Attendre 2
        var result3 = await task3; // Attendre 3
        
        // Temps total : somme de toutes les tâches (sérialisé)
    }
}

// ✅ BON : Véritablement parallèle avec WhenAll
public class ProperAsyncParallelism
{
    [Fact]
    public async Task Fast_TrulyParallel()
    {
        var service = GetService<IService>();
        
        // Créer toutes les tâches simultanément
        var tasks = new[]
        {
            service.FetchDataAsync(1),
            service.FetchDataAsync(2),
            service.FetchDataAsync(3)
        };
        
        // Attendre que tout soit terminé
        var results = await Task.WhenAll(tasks);
        
        // Temps total : max de toutes les tâches (véritablement parallèle)
    }
}
```

---

## Liste de Contrôle Pratique d'Optimisation

| Optimisation | Impact | Effort | Priorité |
|--------------|--------|--------|----------|
| Exécution parallèle | 4-8x | Faible | 🔴 HAUTE |
| Bases de données en mémoire | 50x | Moyen | 🔴 HAUTE |
| Cache de liaison de pas | 50x | Faible | 🟠 MOYEN |
| Regroupement de connexions | 5x | Moyen | 🟠 MOYEN |
| Regroupement d'objets | 2-3x | Moyen | 🟡 FAIBLE |
| Réinitialisation singleton | N/A | Faible | 🔴 HAUTE |
| Suivi des benchmarks | N/A | Faible | 🟡 FAIBLE |

---

## Exemples de Performance : Avant et Après

### Exemple : Suite de Tests BDD Lente

**Avant optimisation :**
```
Nombre total de tests : 150 scénarios BDD
Temps d'exécution : 45 secondes
DegreeOfParallelism : 1 (séquentiel)
Base de données : SQL Server réelle
Liaison de pas : Réfléchie par scénario
```

**Après optimisation :**
```
Nombre total de tests : 150 scénarios BDD
Temps d'exécution : 8 secondes (5.6x plus rapide) ✅
DegreeOfParallelism : 4 (cœurs CPU / 2)
Base de données : EnMémoire (unité), SQLite (intégration)
Liaison de pas : Mise en cache au démarrage
```

### Exemple : Détection de Fuite Mémoire

**Avant optimisation :**
```csharp
[Fact]
public async Task CreateManyUsers_LeaksMemory()
{
    var repo = GetService<IRepository>();
    
    for (int i = 0; i < 1000; i++)
    {
        var user = new User { Id = i, Name = $"User{i}" };
        await repo.AddAsync(user);
        // ❌ Les objets ne sont jamais libérés
    }
}
```

**Après optimisation :**
```csharp
[Fact]
public async Task CreateManyUsers_IsMemoryClean()
{
    var repo = GetService<IRepository>();
    
    // Utiliser une portée jetable
    using (var batch = repo.CreateBatch())
    {
        for (int i = 0; i < 1000; i++)
        {
            var user = new User { Id = i, Name = $"User{i}" };
            batch.Add(user);
        }
        await batch.FlushAsync();
    } // ✅ Les objets sont correctement supprimés
}
```

---

## Pièges Courants

### Piège 1 : Sur-parallélisation

```csharp
// ❌ Faux : Paralléliser les tests de base de données
[Collection("Parallèle")] // ❌ Contention!
public class UserDatabaseTests
{
    [Fact]
    public async Task Test1_CreatesUser() { }
    
    [Fact]
    public async Task Test2_UpdatesUser() { } // Peut interférer avec Test1
}

// ✅ Correct : Séquentiel pour les tests avec état
[Collection("Séquentiel")]
public class UserDatabaseTests
{
    [Fact]
    public async Task Test1_CreatesUser() { }
    
    [Fact]
    public async Task Test2_UpdatesUser() { } // Ordre garanti
}
```

### Piège 2 : Oublier le Nettoyage

```csharp
// ❌ Faux : Oublié de réinitialiser
public class RepeatingFileTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task Test1_WritesFile()
    {
        await File.WriteAllTextAsync("test.txt", "data");
    }
    
    [Fact]
    public async Task Test2_ChecksFile()
    {
        var content = await File.ReadAllTextAsync("test.txt");
        Assert.Equal("data", content); // Flaky : dépend de Test1 s'exécutant en premier
    }
}

// ✅ Correct : Nettoyage explicite
public class ProperFileTests : XUnitIntegrationTestFixture
{
    private const string TestFile = "test.txt";
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (File.Exists(TestFile)) File.Delete(TestFile);
    }
    
    public override async Task DisposeAsync()
    {
        if (File.Exists(TestFile)) File.Delete(TestFile);
        await base.DisposeAsync();
    }
}
```

---

## Résumé

L'optimisation des performances est une discipline en cours. Concentrez-vous sur :

1. **Exécution parallèle en premier** – Plus grand impact, moins d'effort
2. **Bonne sélection de base de données** – EnMémoire pour la vitesse, Réelle pour la fidélité
3. **Nettoyage approprié** – Prévenir les défaillances en cascade et la flakiness
4. **Cache stratégique** – La découverte et la réflexion sont coûteuses
5. **Mesurer continuellement** – Les lignes de base détectent les régressions tôt

La performance de votre suite de tests affecte directement la productivité des développeurs. Une suite de 10 secondes est exécutée avant chaque commit. Une suite de 2 minutes est ignorée. Choisissez sagement.

---

**Suivant :** [Guide de Dépannage et Débogage](./troubleshooting-debugging-FR.md)  
**Précédent :** [Guide de Génération de Tests](../GUIDES/test-generation-guide-FR.md)
