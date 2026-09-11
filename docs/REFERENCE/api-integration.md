# PeasyPilot.Integration API Reference

## Overview

`PeasyPilot.Integration` provides comprehensive support for integration testing including database fixtures, HTTP client helpers, ASP.NET Core application factories, and authentication handling. It enables seamless setup and teardown of complex test environments with dependency injection and database lifecycle management.

**Key Responsibilities:**
- In-memory and external database fixture management
- Integration test base fixtures with DI
- ASP.NET Core application testing factories
- HTTP client testing helpers
- Authentication fixture handling
- Database factory abstraction
- Service lifecycle management (IResettable)

**Targets:** .NET 8.0, 9.0, 10.0

---

## Main Abstractions

### ITestDatabase

Interface for managing test database operations.

```csharp
namespace PeasyPilot.Integration.Abstractions;

/// <summary>
/// Interface for managing test database operations.
/// </summary>
public interface ITestDatabase
{
    /// <summary>
    /// Initializes the database asynchronously.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Cleans up the database asynchronously.
    /// </summary>
    Task CleanupAsync();

    /// <summary>
    /// Seeds the database with test data asynchronously.
    /// </summary>
    Task SeedAsync();

    /// <summary>
    /// Resets the database to its initial state asynchronously.
    /// </summary>
    Task ResetAsync();
}
```

**Purpose:** Provides lifecycle management (Initialize → Seed → Reset → Cleanup) for test databases.

**Example: Implementing ITestDatabase**
```csharp
using PeasyPilot.Integration.Abstractions;

public class TestDatabase : ITestDatabase
{
    private readonly DbContext _context;
    private readonly IEnumerable<TestDataSeed> _seeds;

    public TestDatabase(DbContext context, IEnumerable<TestDataSeed> seeds)
    {
        _context = context;
        _seeds = seeds;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task SeedAsync()
    {
        foreach (var seed in _seeds)
        {
            await seed.ExecuteAsync(_context);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ResetAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await InitializeAsync();
        await SeedAsync();
    }

    public async Task CleanupAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }
}
```

### ITestDatabaseFactory

Factory interface for creating test database instances.

```csharp
namespace PeasyPilot.Integration.Abstractions;

/// <summary>
/// Factory interface for creating test database instances.
/// </summary>
public interface ITestDatabaseFactory
{
    /// <summary>
    /// Creates a new test database instance.
    /// </summary>
    /// <returns>A configured ITestDatabase instance.</returns>
    ITestDatabase CreateDatabase();
}
```

**Purpose:** Enables dependency injection and swapping of different database implementations (in-memory, SQLite, real DB).

**Example: Implementing ITestDatabaseFactory**
```csharp
using PeasyPilot.Integration.Abstractions;
using Microsoft.EntityFrameworkCore;

public class CustomDatabaseFactory : ITestDatabaseFactory
{
    private readonly string _connectionString;
    private readonly IEnumerable<TestDataSeed> _seeds;

    public CustomDatabaseFactory(string connectionString, IEnumerable<TestDataSeed> seeds)
    {
        _connectionString = connectionString;
        _seeds = seeds;
    }

    public ITestDatabase CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connectionString)
            .Options;

        var context = new ApplicationDbContext(options);
        return new TestDatabase(context, _seeds);
    }
}
```

### IResettable

Interface for services that can be reset during test execution.

```csharp
namespace PeasyPilot.Integration.Abstractions;

/// <summary>
/// Interface for services that need to be reset between test runs.
/// </summary>
public interface IResettable
{
    /// <summary>
    /// Resets the service to its initial state asynchronously.
    /// </summary>
    Task ResetAsync();
}
```

**Purpose:** Allows singleton services to be reset between tests without full disposal.

**Example: Implementing IResettable**
```csharp
using PeasyPilot.Integration.Abstractions;

public class CacheService : IResettable
{
    private Dictionary<string, object> _cache = new();

    public void Set(string key, object value)
    {
        _cache[key] = value;
    }

    public object? Get(string key)
    {
        return _cache.TryGetValue(key, out var value) ? value : null;
    }

    public async Task ResetAsync()
    {
        _cache.Clear();
        await Task.CompletedTask;
    }
}
```

---

## Core Classes

### IntegrationTestFixture

Base class for integration tests with dependency injection and database lifecycle management.

```csharp
namespace PeasyPilot.Integration.Fixtures;

public abstract class IntegrationTestFixture : IAsyncDisposable
{
    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    protected IServiceProvider Services { get; }

    /// <summary>
    /// Gets the test database instance.
    /// </summary>
    protected ITestDatabase Database { get; }

    /// <summary>
    /// Configures the dependency injection container.
    /// Override this method to register your services.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Default empty implementation
    }

    /// <summary>
    /// Creates the database factory for test database instances.
    /// </summary>
    protected virtual ITestDatabaseFactory CreateDatabaseFactory()
    {
        return new InMemoryDatabaseFactory();
    }

    /// <summary>
    /// Initializes the fixture: sets up DI container and database.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        // Implementation...
    }

    /// <summary>
    /// Cleans up resources: resets database and disposes services.
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        // Implementation...
    }

    /// <summary>
    /// Resets the database to its initial state.
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        // Implementation...
    }
}
```

**Purpose:** Provides a complete integration testing environment with DI, database management, and async lifecycle.

**Example: Using IntegrationTestFixture**
```csharp
using PeasyPilot.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class UserRepositoryIntegrationTests : IntegrationTestFixture, IAsyncLifetime
{
    private IUserRepository _repository;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await InitializeAsync();
        _repository = Services.GetRequiredService<IUserRepository>();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsId()
    {
        var user = new User { Name = "John", Email = "john@example.com" };
        var id = await _repository.CreateAsync(user);

        Assert.True(id > 0);
    }

    [Fact]
    public async Task GetUser_WithExistingId_ReturnsUser()
    {
        var user = new User { Name = "Jane", Email = "jane@example.com" };
        var id = await _repository.CreateAsync(user);

        var retrieved = await _repository.GetAsync(id);

        Assert.NotNull(retrieved);
        Assert.Equal("Jane", retrieved.Name);
    }

    [Fact]
    public async Task ResetBetweenTests_ClearsDatabase()
    {
        await ResetDatabaseAsync();
        
        var users = await _repository.GetAllAsync();
        
        Assert.Empty(users);
    }
}
```

### InMemoryTestDatabase

In-memory database implementation backed by ITestStore.

```csharp
namespace PeasyPilot.Integration.Fixtures;

public class InMemoryTestDatabase : ITestDatabase
{
    /// <summary>
    /// Gets the underlying in-memory test store.
    /// </summary>
    public ITestStore Store { get; }

    /// <summary>
    /// Initializes a new instance with optional custom store.
    /// </summary>
    public InMemoryTestDatabase(ITestStore? store = null)
    {
        Store = store ?? new InMemoryTestStore();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task CleanupAsync() => Store.ResetAsync();

    public Task SeedAsync() => Task.CompletedTask;

    public Task ResetAsync() => Store.ResetAsync();
}
```

**Purpose:** Fast in-memory test database for unit and lightweight integration tests.

**Example: Using InMemoryTestDatabase**
```csharp
using PeasyPilot.Integration.Fixtures;

[Fact]
public async Task InMemoryDatabase_FastForTesting()
{
    var database = new InMemoryTestDatabase();
    await database.InitializeAsync();

    // Use database for testing...

    await database.CleanupAsync();
}
```

### WebApplicationTestFactory<TStartup>

Test factory for ASP.NET Core applications with configurable services and middleware.

```csharp
namespace PeasyPilot.Integration.Fixtures;

public class WebApplicationTestFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    /// <summary>
    /// Configures services for the test application.
    /// </summary>
    public WebApplicationTestFactory<TStartup> WithServices(
        Action<IServiceCollection> configure)
    {
        // Implementation...
        return this;
    }

    /// <summary>
    /// Configures the application middleware pipeline.
    /// </summary>
    public WebApplicationTestFactory<TStartup> WithApp(
        Action<IApplicationBuilder> configure)
    {
        // Implementation...
        return this;
    }

    /// <summary>
    /// Configures the web host builder directly.
    /// </summary>
    public WebApplicationTestFactory<TStartup> WithWebHostBuilder(
        Action<IWebHostBuilder> configure)
    {
        // Implementation...
        return this;
    }

    /// <summary>
    /// Creates an HTTP client for test requests.
    /// </summary>
    public HttpClient CreateTestClient()
    {
        return CreateClient();
    }

    /// <summary>
    /// Creates an HTTP client with assertion helpers.
    /// </summary>
    public HttpTestClient CreateHttpTestClient()
    {
        return new HttpTestClient(CreateTestClient());
    }
}
```

**Purpose:** Fluent API for setting up ASP.NET Core application tests with service overrides and middleware injection.

**Example: Using WebApplicationTestFactory**
```csharp
using PeasyPilot.Integration.Fixtures;
using Xunit;

public class ApiIntegrationTests : IAsyncLifetime
{
    private WebApplicationTestFactory<Startup> _factory;
    private HttpClient _client;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationTestFactory<Startup>()
            .WithServices(services =>
            {
                // Override services for testing
                services.AddScoped<IUserService, MockUserService>();
            })
            .WithApp(app =>
            {
                // Add test middleware
                app.UseMiddleware<TestAuthenticationMiddleware>();
            });

        _client = _factory.CreateTestClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetApi_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/users");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostApi_CreatesResource()
    {
        var content = new StringContent("{\"name\":\"John\"}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/users", content);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }
}
```

### HttpTestClient

HTTP client with assertion helpers for integration testing.

```csharp
namespace PeasyPilot.Integration.Helpers;

public class HttpTestClient
{
    private readonly HttpClient _httpClient;

    public HttpTestClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the underlying HTTP client.
    /// </summary>
    public HttpClient Client => _httpClient;

    /// <summary>
    /// Makes a GET request and asserts success status.
    /// </summary>
    public async Task<T> GetAsJsonAsync<T>(string requestUri)
    {
        var response = await _httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content)!;
    }

    /// <summary>
    /// Makes a POST request and asserts success status.
    /// </summary>
    public async Task<T> PostAsJsonAsync<T>(string requestUri, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseContent)!;
    }
}
```

**Purpose:** Simplifies common HTTP testing operations with automatic JSON serialization and status code assertions.

**Example: Using HttpTestClient**
```csharp
using PeasyPilot.Integration.Helpers;

public class ApiClientTests
{
    [Fact]
    public async Task GetUser_ReturnsParsedObject()
    {
        var client = new HttpTestClient(new HttpClient { BaseAddress = new Uri("http://api.example.com") });

        var user = await client.GetAsJsonAsync<User>("/api/users/1");

        Assert.NotNull(user);
        Assert.Equal("John", user.Name);
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedUser()
    {
        var client = new HttpTestClient(new HttpClient { BaseAddress = new Uri("http://api.example.com") });
        var newUser = new User { Name = "Jane", Email = "jane@example.com" };

        var created = await client.PostAsJsonAsync<User>("/api/users", newUser);

        Assert.NotNull(created.Id);
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

---

## Common Patterns

### Pattern 1: End-to-End Integration Test

```csharp
using PeasyPilot.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;

public class OrderProcessingIntegrationTests : IntegrationTestFixture, IAsyncLifetime
{
    private IOrderService _orderService;
    private IPaymentService _paymentService;

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("OrderDb"));
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await InitializeAsync();
        _orderService = Services.GetRequiredService<IOrderService>();
        _paymentService = Services.GetRequiredService<IPaymentService>();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
    }

    [Fact]
    public async Task ProcessOrder_ValidatesAndPays()
    {
        var order = new Order { ItemCount = 3, Total = 99.99m };
        
        await _orderService.CreateAsync(order);
        var paid = await _paymentService.ProcessAsync(order.Id);

        Assert.True(paid);
    }
}
```

### Pattern 2: Database Factory Pattern

```csharp
using PeasyPilot.Integration.Abstractions;

public class SqliteDatabaseFactory : ITestDatabaseFactory
{
    private readonly string _dbFile;

    public SqliteDatabaseFactory(string dbFile = "test.db")
    {
        _dbFile = dbFile;
    }

    public ITestDatabase CreateDatabase()
    {
        var connectionString = $"Data Source={_dbFile};";
        var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connectionString)
                .Options);

        return new SqliteTestDatabase(context);
    }
}

public class SqliteTestDatabase : ITestDatabase
{
    private readonly DbContext _context;

    public SqliteTestDatabase(DbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task SeedAsync()
    {
        // Add test data
        await _context.SaveChangesAsync();
    }

    public async Task ResetAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await InitializeAsync();
        await SeedAsync();
    }

    public async Task CleanupAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }
}
```

### Pattern 3: ASP.NET Core with Service Override

```csharp
using PeasyPilot.Integration.Fixtures;

public class AuthenticationIntegrationTests : IAsyncLifetime
{
    private WebApplicationTestFactory<Startup> _factory;
    private HttpClient _client;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationTestFactory<Startup>()
            .WithServices(services =>
            {
                services.AddScoped<IAuthenticationService, MockAuthenticationService>();
                services.AddScoped<IUserRepository, MockUserRepository>();
            })
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            });

        _client = _factory.CreateTestClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var loginRequest = new { email = "test@example.com", password = "password" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

## Configuration

### Dependency Injection Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Integration.Abstractions;
using PeasyPilot.Integration.Fixtures;

public class IntegrationTestConfiguration
{
    public static IServiceCollection AddIntegrationTestSupport(
        this IServiceCollection services)
    {
        // Register database factory
        services.AddScoped<ITestDatabaseFactory, InMemoryDatabaseFactory>();

        return services;
    }
}
```

---

## Reference Summary

| Component | Purpose | Usage |
|-----------|---------|-------|
| ITestDatabase | Lifecycle management (init, seed, reset, cleanup) | Interface for implementations |
| ITestDatabaseFactory | Create database instances | Factory pattern, DI |
| IResettable | Reset singleton services between tests | Interface for services |
| IntegrationTestFixture | Base class for integration tests | Inheritance in test classes |
| InMemoryTestDatabase | Fast in-memory test database | Direct instantiation |
| WebApplicationTestFactory<T> | ASP.NET Core app testing | Inheritance or composition |
| HttpTestClient | HTTP testing helpers | Direct instantiation |

---

## See Also

- **GETTING-STARTED.md** — Quick start guide
- **integration-testing-guide.md** — Comprehensive integration testing guide
- **api-core.md** — PeasyPilot.Core API
- **api-unit.md** — PeasyPilot.Unit API
