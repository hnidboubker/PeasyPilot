using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Integration.Fixtures;
using Xunit;

namespace PeasyPilot.Core.Tests.Integration;

/// <summary>
/// End-to-End integration tests demonstrating real-world usage patterns.
/// Tests complete workflows: setup, execution, isolation, and cleanup.
/// </summary>
public class IntegrationE2ETests : XUnitIntegrationTestFixture
{
    private readonly List<string> _executionLog = new();

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, E2EUserRepository>();
        services.AddSingleton<IUserService, E2EUserService>();
    }

    [Fact]
    public async Task E2E_User_Creation_And_Retrieval()
    {
        var service = GetService<IUserService>();

        // Act: Create user
        var userId = await service.CreateUserAsync("Alice", "alice@example.com");

        // Assert: User created with valid ID
        Assert.True(userId > 0);

        // Act: Retrieve user
        var user = await service.GetUserAsync(userId);

        // Assert: User retrieved with correct data
        Assert.NotNull(user);
        Assert.Equal("Alice", user.Name);
        Assert.Equal("alice@example.com", user.Email);
    }

    [Fact]
    public async Task E2E_Multiple_Users_Independent_State()
    {
        var service = GetService<IUserService>();

        // Test 1: Create first user
        var user1Id = await service.CreateUserAsync("Bob", "bob@example.com");
        var user1 = await service.GetUserAsync(user1Id);
        Assert.Equal("Bob", user1.Name);

        // Test 2: Reset database
        await ResetDatabaseAsync();

        // Assert: Database is empty after reset
        var usersAfterReset = await service.GetAllUsersAsync();
        Assert.Empty(usersAfterReset);

        // Test 3: Create second user (fresh state)
        var user2Id = await service.CreateUserAsync("Carol", "carol@example.com");
        var user2 = await service.GetUserAsync(user2Id);
        Assert.Equal("Carol", user2.Name);

        // Assert: Only one user exists (no bleed from test 1)
        var allUsers = await service.GetAllUsersAsync();
        Assert.Single(allUsers);
    }

    [Fact]
    public async Task E2E_Bulk_Operations_And_Filtering()
    {
        var service = GetService<IUserService>();

        // Arrange: Create multiple users
        await service.CreateUserAsync("User1", "user1@example.com");
        await service.CreateUserAsync("User2", "user2@example.com");
        await service.CreateUserAsync("User3", "user3@example.com");

        // Act: Get all users
        var allUsers = await service.GetAllUsersAsync();

        // Assert: All users retrieved
        Assert.Equal(3, allUsers.Count);

        // Act: Get specific user
        var user2 = await service.GetUserAsync(2);

        // Assert: Correct user retrieved
        Assert.Equal("User2", user2.Name);
    }

    [Fact]
    public async Task E2E_Service_Dependencies_Resolved_Correctly()
    {
        var repo = GetService<IUserRepository>();
        var service = GetService<IUserService>();

        Assert.NotNull(repo);
        Assert.NotNull(service);

        // Act: Add via repository directly
        await repo.AddAsync(new E2EUser { Name = "Direct", Email = "direct@example.com" });

        // Assert: Service sees the data
        var allUsers = await service.GetAllUsersAsync();
        Assert.NotEmpty(allUsers);
        Assert.Contains(allUsers, u => u.Name == "Direct");
    }

    [Fact]
    public async Task E2E_Lifecycle_Initialize_Use_Cleanup_Repeat()
    {
        var service = GetService<IUserService>();

        // Cycle 1
        await service.CreateUserAsync("First", "first@example.com");
        var countFirst = (await service.GetAllUsersAsync()).Count;
        Assert.Equal(1, countFirst);

        // Reset
        await ResetDatabaseAsync();
        var countAfterReset = (await service.GetAllUsersAsync()).Count;
        Assert.Empty(await service.GetAllUsersAsync());

        // Cycle 2
        await service.CreateUserAsync("Second", "second@example.com");
        var countSecond = (await service.GetAllUsersAsync()).Count;
        Assert.Equal(1, countSecond);

        // Isolation verified: no bleed between cycles
    }
}

// Test models and services
public class E2EUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public interface IUserRepository
{
    Task AddAsync(E2EUser user);
    Task<E2EUser?> GetByIdAsync(int id);
    Task<IReadOnlyList<E2EUser>> GetAllAsync();
}

public interface IUserService
{
    Task<int> CreateUserAsync(string name, string email);
    Task<E2EUser> GetUserAsync(int id);
    Task<IReadOnlyList<E2EUser>> GetAllUsersAsync();
}

public class E2EUserRepository : IUserRepository
{
    private readonly List<E2EUser> _users = new();
    private int _nextId = 1;

    public Task AddAsync(E2EUser user)
    {
        user.Id = _nextId++;
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<E2EUser?> GetByIdAsync(int id)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
    }

    public Task<IReadOnlyList<E2EUser>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<E2EUser>>(_users.AsReadOnly());
    }
}

public class E2EUserService : IUserService
{
    private readonly IUserRepository _repository;

    public E2EUserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CreateUserAsync(string name, string email)
    {
        var user = new E2EUser { Name = name, Email = email };
        await _repository.AddAsync(user);
        return user.Id;
    }

    public async Task<E2EUser> GetUserAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user ?? throw new InvalidOperationException($"User {id} not found");
    }

    public Task<IReadOnlyList<E2EUser>> GetAllUsersAsync()
    {
        return _repository.GetAllAsync();
    }
}
