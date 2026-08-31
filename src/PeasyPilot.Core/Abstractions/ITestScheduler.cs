namespace PeasyPilot.Core.Abstractions;

using PeasyPilot.Core.Models;

/// <summary>
/// Schedules and executes discovered tests in a framework-agnostic way.
/// </summary>
public interface ITestScheduler
{
    /// <summary>
    /// Executes a group of tests.
    /// </summary>
    /// <param name="tests">The tests to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The execution results.</returns>
    Task<IReadOnlyCollection<TestResult>> ExecuteAsync(IEnumerable<TestCase> tests, CancellationToken cancellationToken = default);
}
