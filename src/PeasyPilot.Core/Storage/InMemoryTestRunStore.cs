using System.Collections.Concurrent;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Storage;

/// <summary>
/// In-memory implementation of <see cref="ITestRunStore"/> for storing test execution history during application lifetime.
/// </summary>
public sealed class InMemoryTestRunStore : ITestRunStore
{
    private readonly ConcurrentBag<TestRunRecord> _records = [];

    /// <inheritdoc />
    public Task SaveRunAsync(TestRunRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        _records.Add(record);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<TestRunRecord>> GetRunHistoryAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        var ordered = _records.OrderByDescending(r => r.ExecutedAt);
        IReadOnlyCollection<TestRunRecord> result = limit.HasValue
            ? ordered.Take(limit.Value).ToList()
            : ordered.ToList();

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<TestRunRecord?> GetLatestRunAsync(CancellationToken cancellationToken = default)
    {
        var latest = _records.OrderByDescending(r => r.ExecutedAt).FirstOrDefault();
        return Task.FromResult(latest);
    }
}
