# Performance & Optimization Guide

## Overview

In this guide you'll learn how to:
- Design tests for parallel execution and maximum throughput
- Optimize test isolation and cleanup patterns
- Implement caching and memoization strategies
- Choose between in-memory and real databases
- Manage memory and detect leaks
- Measure and benchmark test performance
- Scale your test suite as it grows

**Prerequisites:** Complete [Integration Testing Guide](../GUIDES/integration-testing-guide.md)  
**Time estimate:** 45 minutes  
**Frameworks:** xUnit, NUnit, TUnit  
**Code examples:** 12 working examples

---

## Why Performance Matters

Test suites grow over time. What runs in 5 seconds today becomes 50 seconds in a month. As test execution time increases:

- Developer feedback loops slow down (longer wait for red/green)
- CI/CD pipelines become bottlenecks
- Test coverage decreases (developers skip slow tests)
- Developers leave tests undiscovered, creating gaps

A well-optimized test suite is:
- **Fast** – Complete in seconds, not minutes
- **Scalable** – Adds 10% overhead per 10% new tests, not exponential
- **Predictable** – Same duration regardless of test order
- **Efficient** – Uses memory wisely, minimal resource waste
- **Measurable** – Clear performance baselines and regressions tracked

---

## Core Concepts

### 1. Parallel Execution Strategies

Parallel test execution is the most impactful optimization. PeasyPilot orchestration supports configurable parallelism:

```csharp
// xUnit – Built-in parallel execution
[CollectionDefinition("Sequential")]
public class SequentialCollection { }

[Collection("Sequential")]
public class DatabaseTests
{
    // These tests run sequentially to avoid database contention
}

// Remaining tests run in parallel by default
[Fact]
public void UnitTest_RunsInParallel() { }
```

**Key principles:**
- **CPU-bound tests** (calculations, parsing, logic) parallelize well
- **I/O-bound tests** (database, API calls) parallelize moderately
- **Stateful tests** (shared state, order-dependent) must serialize
- **DegreeOfParallelism** should match (CPU cores / 2) for stability

### 2. Test Isolation & Cleanup Patterns

Proper cleanup prevents test pollution and cascading failures:

```csharp
// Bad: Shared state without cleanup
public class BadIsolationTests
{
    private static List<User> _users = new(); // ❌ Shared mutable state
    
    [Fact]
    public void Test1_AddsUser()
    {
        _users.Add(new User { Id = 1, Name = "Alice" });
        Assert.Single(_users);
    }
    
    [Fact]
    public void Test2_ChecksUsers()
    {
        // ❌ Test1 may have run first, _users contains Alice
        Assert.Empty(_users); // Flaky!
    }
}

// Good: Proper isolation with IResettable
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
        // ✅ Repository auto-reset via IResettable
        var users = await _repository.GetAllAsync();
        Assert.Empty(users);
    }
}
```

### 3. Caching & Memoization

Step binding discovery and pattern matching are expensive. Cache them:

```csharp
// Without caching: O(N) reflection per scenario
public class SlowStepBindingResolver : IStepBindingResolver
{
    public StepBinding? ResolveBinding(string stepText)
    {
        // Scans entire assembly for [Given]/[When]/[Then] attributes
        // Every scenario execution pays this cost
        var bindings = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(BddStepDefinition).IsAssignableFrom(t))
            .SelectMany(t => GetMethodBindings(t));
        
        return bindings.FirstOrDefault(b => b.Matches(stepText));
    }
}

// With caching: O(1) after first discovery
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
        // Cache miss: check regex patterns
        return _bindingCache.Values
            .FirstOrDefault(b => b.PatternRegex.IsMatch(stepText));
    }
}
```

**Performance impact:** 50ms → 1ms per scenario discovery (50x faster)

### 4. Database Optimization Trade-offs

| Aspect | InMemory | SQLite | Real DB |
|--------|----------|--------|---------|
| Speed | 1ms/op | 5ms/op | 50ms/op |
| Isolation | Automatic | Manual | Manual |
| Real Schema | ❌ | ✅ | ✅ |
| Transactions | ❌ | ✅ | ✅ |
| Setup Time | 10ms | 100ms | 500ms |
| Test Count | 10,000+ | 1,000+ | 100+ |

**Strategy:**

```csharp
public class OptimizedDatabaseChoice
{
    // Unit tests: In-memory only
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
            // Runs in <1ms
        }
    }
    
    // Integration tests: Use real DB for critical paths
    [CollectionDefinition("Database")]
    public class DatabaseCollection
    {
        // Sequential to avoid contention
    }
    
    [Collection("Database")]
    public class PaymentProcessingTests : XUnitIntegrationTestFixture
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            // Use real database for payment processing
            services.AddSingleton<ITestDatabase>(provider =>
                new SqlTestDatabase("Server=.; Database=PaymentTest;")
            );
        }
        
        [Fact]
        public async Task ProcessPayment_WithValidCard_Succeeds()
        {
            var processor = GetService<PaymentProcessor>();
            // Real database transaction testing
        }
    }
}
```

---

## Optimization Patterns

### Pattern 1: Parallel Execution with Degree Control

```csharp
public class ParallelExecutionOptimizer
{
    public static int CalculateDegreeOfParallelism()
    {
        // Rule: Use (CPU cores / 2) for stability
        // Conservative: Leaves room for system tasks
        int cores = Environment.ProcessorCount;
        return Math.Max(1, cores / 2);
    }
}

// In test configuration
public class TestConfiguration
{
    public static void ConfigureParallelism(TestOrchestrator orchestrator)
    {
        int degree = ParallelExecutionOptimizer.CalculateDegreeOfParallelism();
        orchestrator.SetParallelDegree(degree);
    }
}
```

### Pattern 2: Singleton Reset Strategy

```csharp
public interface IResettable
{
    Task ResetAsync();
}

// Repository that needs reset between tests
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

// Integration fixture auto-resets
public class UserRepositoryTests : XUnitIntegrationTestFixture
{
    private IUserRepository _repository = null!;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = GetService<IUserRepository>();
        // Fixture auto-calls ResetAsync() on IResettable services
    }
    
    [Fact]
    public async Task EachTestStartsClean()
    {
        var users = await _repository.GetAllAsync();
        Assert.Empty(users); // ✅ Clean state
    }
}
```

### Pattern 3: Memoized Step Discovery

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

### Pattern 4: Connection Pooling for Real Databases

```csharp
public class OptimizedDatabaseFactory : ITestDatabaseFactory
{
    private static readonly Lazy<SqlConnectionPool> _pool = 
        new(() => new SqlConnectionPool(connectionString: "..."));
    
    public async Task<ITestDatabase> CreateAsync()
    {
        // Reuses pooled connections instead of creating new ones
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
        for (int i = 0; i < _maxSize / 2; i++) // Start with 50% capacity
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

## Memory Management

### Memory Profiling

```csharp
public class MemoryOptimizedTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task TestDoesNotLeakMemory()
    {
        var beforeGC = GC.GetTotalMemory(false);
        
        // Run test operation
        for (int i = 0; i < 1000; i++)
        {
            var repo = GetService<IRepository>();
            var items = await repo.GetAllAsync();
            Assert.NotEmpty(items);
        }
        
        // Force collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var afterGC = GC.GetTotalMemory(false);
        
        // Memory should not grow significantly
        var leakSuspicion = (afterGC - beforeGC) / (double)beforeGC;
        Assert.True(leakSuspicion < 0.1, $"Memory grew {leakSuspicion:P}"); // <10% growth
    }
}
```

### Object Pooling Pattern

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

### Performance Measurement

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

// Run benchmarks
public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<StepBindingBenchmarks>();
    }
}
```

**Example output:**
```
| Method    | Mean     | StdDev   | Gen0 | Gen1 | Gen2 | Allocated |
|-----------|----------|----------|------|------|------|-----------|
| SlowResolve  | 45.32 ms | 2.10 ms  | 40.0 | 2.0  | 0.5  | 125 KB    |
| FastResolve  | 0.92 ms  | 0.08 ms  | 0.0  | 0.0  | 0.0  | 0 KB      |
```

### Performance Regression Testing

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
        // Load feature file...
        return new Feature();
    }
}
```

---

## Advanced Scenarios

### Scenario 1: Scaling BDD Test Suites

As BDD feature files grow, step discovery becomes a bottleneck. Here's how to optimize:

```csharp
// Before: 500 features × 5 scenarios each = 2,500 step resolutions
// Time: 50ms/step = 125 seconds! ❌

public class BddScalabilityOptimization
{
    // Solution: Lazy initialization + categorized caching
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
            // Extract category hint (e.g., "Given a USER exists" → "USER")
            var category = ExtractCategory(stepText);
            
            if (_categoryCache.TryGetValue(category, out var cached))
            {
                return FindInCategory(cached, stepText);
            }
            
            // First access: cache this category
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
            // Extract first noun as category: "Given a USER exists" → "USER"
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

// Result: 50ms → 5ms per 500 scenarios (10x improvement) ✅
```

### Scenario 2: Stress Testing with Memory Limits

```csharp
public class MemoryStressTests : XUnitIntegrationTestFixture
{
    [Fact]
    public async Task CreateThousandUsers_WithinMemoryBudget()
    {
        var repo = GetService<IRepository>();
        long initialMemory = GC.GetTotalMemory(true);
        const long maxMemoryIncrease = 10_000_000; // 10 MB budget
        
        // Create users in batches to measure incremental memory
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
            
            // Each batch should use predictable memory
            Assert.True(
                memoryUsed < maxMemoryIncrease,
                $"Batch {batch} memory usage: {memoryUsed / 1024.0 / 1024.0:F2} MB"
            );
        }
    }
}
```

### Scenario 3: Async/Await Parallelization Pitfalls

```csharp
// ❌ BAD: Looks parallel, but serializes at await
public class DeceptiveParallelism
{
    [Fact]
    public async Task Slow_FakeParallel()
    {
        var service = GetService<IService>();
        
        // All tasks created but awaited sequentially
        var task1 = service.FetchDataAsync(1);
        var task2 = service.FetchDataAsync(2);
        var task3 = service.FetchDataAsync(3);
        
        var result1 = await task1; // Wait for 1
        var result2 = await task2; // Wait for 2
        var result3 = await task3; // Wait for 3
        
        // Total time: sum of all tasks (serialized)
    }
}

// ✅ GOOD: Truly parallel with WhenAll
public class ProperAsyncParallelism
{
    [Fact]
    public async Task Fast_TrulyParallel()
    {
        var service = GetService<IService>();
        
        // Create all tasks simultaneously
        var tasks = new[]
        {
            service.FetchDataAsync(1),
            service.FetchDataAsync(2),
            service.FetchDataAsync(3)
        };
        
        // Wait for all to complete
        var results = await Task.WhenAll(tasks);
        
        // Total time: max of all tasks (truly parallel)
    }
}
```

---

## Practical Optimization Checklist

| Optimization | Impact | Effort | Priority |
|--------------|--------|--------|----------|
| Parallel execution | 4-8x | Low | 🔴 HIGH |
| In-memory databases | 50x | Medium | 🔴 HIGH |
| Step binding caching | 50x | Low | 🟠 MEDIUM |
| Connection pooling | 5x | Medium | 🟠 MEDIUM |
| Object pooling | 2-3x | Medium | 🟡 LOW |
| Singleton reset | N/A | Low | 🔴 HIGH |
| Benchmark tracking | N/A | Low | 🟡 LOW |

---

## Performance Examples: Before & After

### Example: Slow BDD Test Suite

**Before optimization:**
```
Total tests: 150 BDD scenarios
Execution time: 45 seconds
DegreeOfParallelism: 1 (sequential)
Database: Real SQL Server
Step binding: Reflected per scenario
```

**After optimization:**
```
Total tests: 150 BDD scenarios
Execution time: 8 seconds (5.6x faster) ✅
DegreeOfParallelism: 4 (CPU cores / 2)
Database: InMemory (unit), SQLite (integration)
Step binding: Cached at startup
```

### Example: Memory Leak Detection

**Before optimization:**
```csharp
[Fact]
public async Task CreateManyUsers_LeaksMemory()
{
    var repo = GetService<IRepository>();
    
    for (int i = 0; i < 1000; i++)
    {
        var user = new User { Id = i, Name = $"User{i}" };
        await repo.AddAsync(user);
        // ❌ Objects never released
    }
}
```

**After optimization:**
```csharp
[Fact]
public async Task CreateManyUsers_IsMemoryClean()
{
    var repo = GetService<IRepository>();
    
    // Use disposable scope
    using (var batch = repo.CreateBatch())
    {
        for (int i = 0; i < 1000; i++)
        {
            var user = new User { Id = i, Name = $"User{i}" };
            batch.Add(user);
        }
        await batch.FlushAsync();
    } // ✅ Objects properly disposed
}
```

---

## Common Pitfalls

### Pitfall 1: Over-parallelization

```csharp
// ❌ Wrong: Parallelizing database tests
[Collection("Parallel")] // ❌ Contention!
public class UserDatabaseTests
{
    [Fact]
    public async Task Test1_CreatesUser() { }
    
    [Fact]
    public async Task Test2_UpdatesUser() { } // May interfere with Test1
}

// ✅ Right: Sequential for stateful tests
[Collection("Sequential")]
public class UserDatabaseTests
{
    [Fact]
    public async Task Test1_CreatesUser() { }
    
    [Fact]
    public async Task Test2_UpdatesUser() { } // Guaranteed order
}
```

### Pitfall 2: Ignoring Cleanup

```csharp
// ❌ Wrong: Forgot to reset
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
        Assert.Equal("data", content); // Flaky: depends on Test1 running first
    }
}

// ✅ Right: Explicit cleanup
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

## Summary

Performance optimization is an ongoing discipline. Focus on:

1. **Parallel execution first** – Biggest impact, lowest effort
2. **Right database choice** – InMemory for speed, Real for fidelity
3. **Proper cleanup** – Prevent cascading failures and flakiness
4. **Strategic caching** – Discovery and reflection are expensive
5. **Measure continuously** – Baselines catch regressions early

Your test suite's performance directly impacts developer productivity. A 10-second suite gets run before every commit. A 2-minute suite gets skipped. Choose wisely.

---

**Next:** [Troubleshooting & Debugging Guide](./troubleshooting-debugging.md)  
**Previous:** [Test Generation Guide](../GUIDES/test-generation-guide.md)
