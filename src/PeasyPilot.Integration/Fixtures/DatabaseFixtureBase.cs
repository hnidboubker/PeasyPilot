using PeasyPilot.Integration.Abstractions;

namespace PeasyPilot.Integration.Fixtures;

/// <summary>
/// Abstract base class for database test fixtures supporting initialization, seeding, and resetting.
/// </summary>
public abstract class DatabaseFixtureBase : IntegrationTestFixture, ITestDatabase
{
    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    public abstract string ConnectionString { get; }

    /// <inheritdoc />
    public abstract Task CleanupAsync();

    /// <inheritdoc />
    public abstract Task SeedAsync();

    /// <inheritdoc />
    public abstract Task ResetAsync();

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await CleanupAsync();
    }
}
