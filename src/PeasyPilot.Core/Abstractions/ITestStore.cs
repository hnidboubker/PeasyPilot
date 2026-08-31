namespace PeasyPilot.Core.Abstractions;

/// <summary>
/// Defines a framework-agnostic in-memory test store abstraction for seeding, querying, and resetting entity state during tests.
/// </summary>
public interface ITestStore
{
    /// <summary>
    /// Resets and clears all entities from the store.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ResetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds entities into the store.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entities">The collection of entities to seed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SeedAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Finds an entity by predicate from the store.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="predicate">Filter predicate to select the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching entity or null.</returns>
    Task<T?> FindAsync<T>(Func<T, bool> predicate, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Retrieves all entities of type T stored in memory.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of stored entities.</returns>
    Task<IReadOnlyCollection<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : class;
}
