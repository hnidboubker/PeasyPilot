using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Integration.Abstractions;

namespace PeasyPilot.Integration.Fixtures;

/// <summary>
/// Base class for integration tests with dependency injection and database lifecycle management.
/// </summary>
public abstract class IntegrationTestFixture : IAsyncDisposable
{
    private IServiceProvider? _serviceProvider;
    private ITestDatabase? _database;

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    protected IServiceProvider Services
    {
        get => _serviceProvider ?? throw new InvalidOperationException("Services not initialized. Call InitializeAsync first.");
    }

    /// <summary>
    /// Gets the test database instance.
    /// </summary>
    protected ITestDatabase Database
    {
        get => _database ?? throw new InvalidOperationException("Database not initialized. Call InitializeAsync first.");
    }

    /// <summary>
    /// Configures the dependency injection container.
    /// Override this method to register your services.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Default empty implementation
    }

    /// <summary>
    /// Creates and configures the test database.
    /// Override this method to use a different database implementation.
    /// </summary>
    protected virtual ITestDatabase CreateDatabase()
    {
        return new InMemoryTestDatabase();
    }

    /// <summary>
    /// Initializes the fixture: sets up DI container and database.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        _database = CreateDatabase();
        await _database.InitializeAsync();
        await _database.SeedAsync();
    }

    /// <summary>
    /// Cleans up resources: resets database and disposes services.
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        if (_database != null)
        {
            await _database.CleanupAsync();
        }

        if (_serviceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }

        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Resets the database to its initial state.
    /// Useful for running multiple tests with a clean slate.
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        await Database.ResetAsync();
    }

    /// <summary>
    /// Gets a scoped service from the dependency injection container.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }
}
