using PeasyPilot.Integration.Fixtures;

namespace PeasyPilot.Integration.Abstractions;

/// <summary>
/// Factory for creating InMemory test database instances.
/// </summary>
public class InMemoryDatabaseFactory : ITestDatabaseFactory
{
    /// <inheritdoc />
    public ITestDatabase CreateDatabase()
    {
        return new InMemoryTestDatabase();
    }
}
