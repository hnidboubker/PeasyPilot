using System.Collections.Concurrent;
using PeasyPilot.Core.Abstractions;

namespace PeasyPilot.Core.Storage;

/// <summary>
/// Default in-memory implementation of <see cref="ITestStore"/> for decoupling test data fixtures from physical databases.
/// </summary>
public sealed class InMemoryTestStore : ITestStore
{
    private readonly ConcurrentDictionary<Type, List<object>> _store = new();

    /// <inheritdoc />
    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        _store.Clear();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SeedAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entities);
        cancellationToken.ThrowIfCancellationRequested();

        var list = _store.GetOrAdd(typeof(T), _ => new List<object>());
        lock (list)
        {
            foreach (var entity in entities)
            {
                list.Add(entity);
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<T?> FindAsync<T>(Func<T, bool> predicate, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_store.TryGetValue(typeof(T), out var list))
        {
            return Task.FromResult<T?>(null);
        }

        lock (list)
        {
            var match = list.OfType<T>().FirstOrDefault(predicate);
            return Task.FromResult(match);
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_store.TryGetValue(typeof(T), out var list))
        {
            return Task.FromResult<IReadOnlyCollection<T>>(Array.Empty<T>());
        }

        lock (list)
        {
            IReadOnlyCollection<T> result = list.OfType<T>().ToList();
            return Task.FromResult(result);
        }
    }
}
