using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Storage;
using PeasyPilot.Integration.Abstractions;

namespace PeasyPilot.Integration.Fixtures;

/// <summary>
/// In-memory database implementation of <see cref="ITestDatabase"/> backed by <see cref="ITestStore"/>.
/// </summary>
public class InMemoryTestDatabase : ITestDatabase
{
    private readonly ITestStore _store;

    /// <summary>
    /// Gets the underlying in-memory test store.
    /// </summary>
    public ITestStore Store => _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryTestDatabase"/> class.
    /// </summary>
    /// <param name="store">Optional custom test store instance.</param>
    public InMemoryTestDatabase(ITestStore? store = null)
    {
        _store = store ?? new InMemoryTestStore();
    }

    /// <inheritdoc />
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CleanupAsync()
    {
        return _store.ResetAsync();
    }

    /// <inheritdoc />
    public Task SeedAsync()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ResetAsync()
    {
        return _store.ResetAsync();
    }
}
