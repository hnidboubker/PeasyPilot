using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Integration.Fixtures;
using Xunit;

namespace PeasyPilot.XUnit.Samples.Tests;

/// <summary>
/// Example integration tests using InMemory database with PeasyPilot Integration fixture.
/// Demonstrates:
/// - Dependency injection configuration
/// - Database lifecycle management (Initialize, Seed, Reset)
/// - Service resolution from DI container
/// </summary>
public class UserRepositoryIntegrationTests : XUnitIntegrationTestFixture
{
    /// <summary>
    /// Configures services for the test fixture.
    /// This runs before each test to set up a fresh environment.
    /// </summary>
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Register a fake/in-memory user repository for testing
        services.AddSingleton<IUserRepository>(new InMemoryUserRepository());
    }

    [Fact]
    public async Task GetAllUsers_WhenDatabaseIsEmpty_ReturnsEmptyList()
    {
        var repository = GetService<IUserRepository>();
        var users = await repository.GetAllAsync();
        Assert.Empty(users);
    }

    [Fact]
    public async Task AddUser_WithValidUser_Succeeds()
    {
        var repository = GetService<IUserRepository>();
        var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };

        await repository.AddAsync(user);

        var users = await repository.GetAllAsync();
        Assert.Single(users);
        Assert.Equal("John Doe", users.First().Name);
    }

    [Fact]
    public async Task GetUserById_WithExistingId_ReturnsUser()
    {
        var repository = GetService<IUserRepository>();
        var user = new User { Id = 1, Name = "Jane Smith", Email = "jane@example.com" };
        await repository.AddAsync(user);

        var retrieved = await repository.GetByIdAsync(1);

        Assert.NotNull(retrieved);
        Assert.Equal("Jane Smith", retrieved.Name);
    }

    [Fact]
    public async Task ResetDatabase_ClearsAllData()
    {
        var repository = GetService<IUserRepository>();

        // Add data
        await repository.AddAsync(new User { Id = 1, Name = "User1", Email = "user1@example.com" });
        await repository.AddAsync(new User { Id = 2, Name = "User2", Email = "user2@example.com" });

        var countBefore = (await repository.GetAllAsync()).Count;
        Assert.Equal(2, countBefore);

        // Reset
        await ResetDatabaseAsync();

        var countAfter = (await repository.GetAllAsync()).Count;
        Assert.Empty(await repository.GetAllAsync());
    }
}

/// <summary>
/// Example in-memory repository for testing without a real database.
/// </summary>
public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByIdAsync(int id);
    Task<IReadOnlyList<User>> GetAllAsync();
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public Task AddAsync(User user)
    {
        user.Id = _nextId++;
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public Task<IReadOnlyList<User>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<User>>(_users.AsReadOnly());
    }
}
