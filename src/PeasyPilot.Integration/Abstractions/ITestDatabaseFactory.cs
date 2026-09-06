namespace PeasyPilot.Integration.Abstractions;

/// <summary>
/// Factory interface for creating test database instances.
/// Allows different database implementations (InMemory, SQLite, real DB) to be swapped.
/// </summary>
public interface ITestDatabaseFactory
{
    /// <summary>
    /// Creates a new test database instance.
    /// </summary>
    /// <returns>A configured ITestDatabase instance.</returns>
    ITestDatabase CreateDatabase();
}
