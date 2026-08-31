using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Abstractions;

/// <summary>
/// Defines the contract for persisting and retrieving test run history.
/// </summary>
public interface ITestRunStore
{
    /// <summary>
    /// Saves a test run record to storage.
    /// </summary>
    /// <param name="record">The record to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveRunAsync(TestRunRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all historical test run records.
    /// </summary>
    /// <param name="limit">Optional maximum number of records to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of test run records ordered by executed time descending.</returns>
    Task<IReadOnlyCollection<TestRunRecord>> GetRunHistoryAsync(int? limit = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the most recent test run record.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The latest test run record or null if history is empty.</returns>
    Task<TestRunRecord?> GetLatestRunAsync(CancellationToken cancellationToken = default);
}
