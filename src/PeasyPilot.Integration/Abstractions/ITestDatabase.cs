namespace PeasyPilot.Integration.Abstractions;

/// <summary>
/// Interface for managing test database operations.
/// </summary>
public interface ITestDatabase
{
    /// <summary>
    /// Initializes the database asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitializeAsync();

    /// <summary>
    /// Cleans up the database asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CleanupAsync();

    /// <summary>
    /// Seeds the database with test data asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SeedAsync();

    /// <summary>
    /// Resets the database to its initial state asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResetAsync();
}
