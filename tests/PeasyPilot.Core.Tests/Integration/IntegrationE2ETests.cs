using Microsoft.Extensions.DependencyInjection;

using PeasyPilot.Core.Tests.Abstractions;
using PeasyPilot.Core.Tests.Models;
using PeasyPilot.Integration.Fixtures;
using PeasyPilot.XUnit;

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

    private async Task ResetAllAsync()
    {
        await ResetDatabaseAsync();
        var repo = GetService<IUserRepository>();
        if (repo is IResettable resettable)
        {
            await resettable.ResetAsync();
        }
    }

    [Fact]
    public async Task E2E_User_Creation_And_Retrieval()
    {
        var service = GetService<IUserService>();

        // Act: Create user
        var userId = await service.CreateUserAsync("Alice", "alice@example.com");

        // Assert: User created with valid ID
        XAssert.True(userId > 0);

        // Act: Retrieve user
        var user = await service.GetUserAsync(userId);

        // Assert: User retrieved with correct data
        XAssert.NotNull(user);
        XAssert.Equal("Alice", user.Name);
        XAssert.Equal("alice@example.com", user.Email);
    }

    [Fact]
    public async Task E2E_Multiple_Users_Independent_State()
    {
        var service = GetService<IUserService>();

        // Test 1: Create first user
        var user1Id = await service.CreateUserAsync("Bob", "bob@example.com");
        var user1 = await service.GetUserAsync(user1Id);
        XAssert.Equal("Bob", user1.Name);

        // Test 2: Reset database
        await ResetAllAsync();

        // Assert: Database is empty after reset
        var usersAfterReset = await service.GetAllUsersAsync();
        XAssert.IsEmpty(usersAfterReset);

        // Test 3: Create second user (fresh state)
        var user2Id = await service.CreateUserAsync("Carol", "carol@example.com");
        var user2 = await service.GetUserAsync(user2Id);
        XAssert.Equal("Carol", user2.Name);

        // Assert: Only one user exists (no bleed from test 1)
        var allUsers = await service.GetAllUsersAsync();
        XAssert.Single(allUsers);
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
        XAssert.Equal(3, allUsers.Count);

        // Act: Get specific user
        var user2 = await service.GetUserAsync(2);

        // Assert: Correct user retrieved
        XAssert.Equal("User2", user2.Name);
    }

    [Fact]
    public async Task E2E_Service_Dependencies_Resolved_Correctly()
    {
        var repo = GetService<IUserRepository>();
        var service = GetService<IUserService>();

        XAssert.NotNull(repo);
        XAssert.NotNull(service);

        // Act: Add via repository directly
        await repo.AddAsync(new E2EUser { Name = "Direct", Email = "direct@example.com" });

        // Assert: Service sees the data
        var allUsers = await service.GetAllUsersAsync();
        XAssert.NotEmpty(allUsers);
        XAssert.True(allUsers.Any(u => u.Name == "Direct"));
    }

    [Fact]
    public async Task E2E_Lifecycle_Initialize_Use_Cleanup_Repeat()
    {
        var service = GetService<IUserService>();

        // Cycle 1
        await service.CreateUserAsync("First", "first@example.com");
        var countFirst = (await service.GetAllUsersAsync()).Count;
        XAssert.Equal(1, countFirst);

        // Reset
        await ResetAllAsync();
        var countAfterReset = (await service.GetAllUsersAsync()).Count;
        XAssert.IsEmpty(await service.GetAllUsersAsync());

        // Cycle 2
        await service.CreateUserAsync("Second", "second@example.com");
        var countSecond = (await service.GetAllUsersAsync()).Count;
        XAssert.Equal(1, countSecond);

        // Isolation verified: no bleed between cycles
    }
}
