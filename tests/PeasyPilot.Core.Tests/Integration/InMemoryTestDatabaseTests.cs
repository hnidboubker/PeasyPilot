using PeasyPilot.Integration.Abstractions;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Integration.Fixtures;
using Xunit;

namespace PeasyPilot.Core.Tests.Integration;

/// <summary>
/// Tests for InMemoryTestDatabase lifecycle and operations.
/// Validates that the database fixture works correctly with the test infrastructure.
/// </summary>
public class InMemoryTestDatabaseTests
{
    [Fact]
    public void CreateDatabase_Succeeds()
    {
        var factory = new InMemoryDatabaseFactory();
        var db = factory.CreateDatabase();

        PeasyAssert.NotNull(db);
        PeasyAssert.IsAssignableFrom<ITestDatabase>(db);
    }

    [Fact]
    public async Task InitializeAsync_Completes()
    {
        var db = new InMemoryTestDatabase();

        await db.InitializeAsync();

        // No exception thrown
        PeasyAssert.True(true);
    }

    [Fact]
    public async Task SeedAsync_Completes()
    {
        var db = new InMemoryTestDatabase();
        await db.InitializeAsync();

        await db.SeedAsync();

        // No exception thrown
        PeasyAssert.True(true);
    }

    [Fact]
    public async Task ResetAsync_Completes()
    {
        var db = new InMemoryTestDatabase();
        await db.InitializeAsync();

        await db.ResetAsync();

        // No exception thrown
        PeasyAssert.True(true);
    }

    [Fact]
    public async Task CleanupAsync_Completes()
    {
        var db = new InMemoryTestDatabase();
        await db.InitializeAsync();

        await db.CleanupAsync();

        // No exception thrown
        PeasyAssert.True(true);
    }

    [Fact]
    public async Task FullLifecycle_Initialize_Seed_Reset_Cleanup()
    {
        var db = new InMemoryTestDatabase();

        await db.InitializeAsync();
        await db.SeedAsync();
        await db.ResetAsync();
        await db.CleanupAsync();

        // All operations completed without error
        PeasyAssert.True(true);
    }
}
