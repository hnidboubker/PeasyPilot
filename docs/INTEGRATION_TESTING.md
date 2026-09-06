# Integration Testing with PeasyPilot

## Overview

PeasyPilot.Integration provides a unified infrastructure for writing integration tests with ASP.NET Core applications and in-memory databases.

## Core Components

### 1. IntegrationTestFixture — Foundation

Base class for all integration tests with DI support and database lifecycle.

```csharp
public class MyIntegrationTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Register your test dependencies here
        services.AddScoped<IMyService, MockMyService>();
    }

    [Fact]
    public async Task MyTest()
    {
        var service = GetService<IMyService>();
        var result = await service.DoSomethingAsync();
        Assert.NotNull(result);
    }
}
```

**Lifecycle:**
- `InitializeAsync()` — Runs before each test
  - Creates ServiceProvider
  - Initializes database
  - Seeds initial data (if needed)
- `DisposeAsync()` — Runs after each test
  - Cleans up database
  - Disposes ServiceProvider

**Available Methods:**
- `GetService<T>()` — Resolve dependency from DI container
- `ResetDatabaseAsync()` — Reset database to initial state between tests
- `Database` — Access the ITestDatabase directly
- `Services` — Access the ServiceProvider

### 2. InMemoryTestDatabase — Fast Testing

Default in-memory implementation for quick integration tests.

```csharp
protected override ITestDatabaseFactory CreateDatabaseFactory()
{
    return new InMemoryDatabaseFactory(); // Default
}
```

**Lifecycle Methods:**
- `InitializeAsync()` — Set up database
- `SeedAsync()` — Populate with test data
- `ResetAsync()` — Clear data between tests
- `CleanupAsync()` — Final cleanup

### 3. WebApplicationTestFactory — ASP.NET Core

Test factory for ASP.NET Core applications with fluent configuration.

```csharp
var factory = new WebApplicationTestFactory<Startup>()
    .WithServices(services =>
    {
        // Override services for testing
        services.AddScoped<IUserRepository, MockUserRepository>();
    })
    .WithApp(app =>
    {
        // Configure middleware pipeline
        app.UseRouting();
    });

var client = factory.CreateHttpTestClient();
var response = await client.GetJsonAsync<User>("/api/users/1");
```

**Configuration Methods:**
- `WithServices(Action<IServiceCollection>)` — Override dependencies
- `WithApp(Action<IApplicationBuilder>)` — Configure middleware
- `WithWebHostBuilder(Action<IWebHostBuilder>)` — Advanced configuration
- `CreateTestClient()` — Get raw HttpClient
- `CreateHttpTestClient()` — Get HttpTestClient with helpers

### 4. HttpTestClient — API Testing Helpers

Wrapper around HttpClient with JSON request/response support.

```csharp
var client = factory.CreateHttpTestClient();

// GET with automatic deserialization
var user = await client.GetJsonAsync<User>("/api/users/1");

// POST with automatic serialization and deserialization
var newUser = new { name = "John", email = "john@example.com" };
var created = await client.PostJsonAsync<object, User>("/api/users", newUser);

// Assert status codes
var response = await client.Client.GetAsync("/api/health");
HttpTestClient.AssertStatusCode(response, HttpStatusCode.OK);
```

### 5. WebApplicationAuthenticationFixture — Auth Testing

Base class for testing authenticated scenarios.

```csharp
public class AuthenticatedApiTests : WebApplicationAuthenticationFixture<Startup>
{
    [Fact]
    public async Task ProtectedEndpoint_WithAuthentication_Succeeds()
    {
        var client = CreateAuthenticatedClient("user-123");
        var response = await client.Client.GetAsync("/api/protected");
        
        HttpTestClient.AssertStatusCode(response, HttpStatusCode.OK);
    }
}
```

## Example: In-Memory Database Testing

```csharp
public class UserRepositoryTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
    }

    [Fact]
    public async Task AddUser_Succeeds()
    {
        var repo = GetService<IUserRepository>();
        var user = new User { Name = "Alice", Email = "alice@example.com" };
        
        await repo.AddAsync(user);
        
        var retrieved = await repo.GetByIdAsync(user.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Alice", retrieved.Name);
    }

    [Fact]
    public async Task GetAll_AfterReset_IsEmpty()
    {
        var repo = GetService<IUserRepository>();
        
        // Add data
        await repo.AddAsync(new User { Name = "Bob", Email = "bob@example.com" });
        
        // Reset
        await ResetDatabaseAsync();
        
        // Verify empty
        var users = await repo.GetAllAsync();
        Assert.Empty(users);
    }
}
```

## Example: ASP.NET Core API Testing

```csharp
public class UserApiTests : IAsyncLifetime
{
    private WebApplicationTestFactory<Startup>? _factory;
    private HttpTestClient? _client;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationTestFactory<Startup>()
            .WithServices(services =>
            {
                services.AddScoped<IUserService, MockUserService>();
            });
        
        _client = _factory.CreateHttpTestClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_factory is IAsyncDisposable ad)
            await ad.DisposeAsync();
        else
            _factory?.Dispose();
    }

    [Fact]
    public async Task PostUser_WithValidData_ReturnsCreated()
    {
        var payload = new { name = "Charlie", email = "charlie@example.com" };
        var user = await _client!.PostJsonAsync<object, User>("/api/users", payload);
        
        Assert.NotNull(user);
        Assert.Equal("Charlie", user.Name);
    }

    [Fact]
    public async Task GetUsers_ReturnsAll()
    {
        var users = await _client!.GetJsonAsync<List<User>>("/api/users");
        
        Assert.NotNull(users);
        Assert.NotEmpty(users);
    }
}
```

## Best Practices

### 1. Use Fixture Base Classes
```csharp
// ✅ Good
public class MyTests : XUnitIntegrationTestFixture
```

### 2. Override ConfigureServices for Test Setup
```csharp
// ✅ Good - Clear test dependencies
protected override void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IRepository, MockRepository>();
}
```

### 3. Reset Between Tests When Needed
```csharp
// ✅ Good - Each test starts fresh
[Fact]
public async Task Test1() { ... }

[Fact]
public async Task Test2() 
{
    await ResetDatabaseAsync(); // Fresh state
    ...
}
```

### 4. Use HttpTestClient for Assertions
```csharp
// ✅ Good - Clear intent
var user = await client.GetJsonAsync<User>("/api/users/1");
Assert.NotNull(user);

// ❌ Avoid - Manual deserialization
var response = await client.Client.GetAsync("/api/users/1");
var json = await response.Content.ReadAsStringAsync();
// ... manual JSON parsing
```

## Framework Support

PeasyPilot Integration supports all major .NET test frameworks:

- **xUnit:** Use `XUnitIntegrationTestFixture`
- **NUnit:** Use `NUnitIntegrationTestFixture` (in PeasyPilot.NUnit package)
- **TUnit:** Use `TUnitIntegrationTestFixture` (in PeasyPilot.TUnit package)

## Architecture

```
IntegrationTestFixture (base)
    ├── ServiceProvider (DI)
    ├── ITestDatabase (lifecycle)
    └── Helpers (GetService, ResetDatabase)

WebApplicationTestFactory<TStartup>
    ├── WithServices()
    ├── WithApp()
    └── CreateHttpTestClient()

HttpTestClient
    ├── GetJsonAsync<T>()
    ├── PostJsonAsync<TReq, TRes>()
    └── AssertStatusCode()
```

## See Also

- [Unit Testing Guide](./UNIT_TESTING.md)
- [BDD Testing Guide](./BDD_TESTING.md)
- [Sample Projects](../samples/)
