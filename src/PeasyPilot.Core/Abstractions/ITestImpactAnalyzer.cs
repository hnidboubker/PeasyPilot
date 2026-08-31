namespace PeasyPilot.Core.Abstractions;

using PeasyPilot.Core.Models;

/// <summary>
/// Identifies the set of tests impacted by changed source files.
/// </summary>
public interface ITestImpactAnalyzer
{
    /// <summary>
    /// Returns the impacted tests for the changed paths.
    /// </summary>
    /// <param name="changedFiles">The changed file names or paths.</param>
    /// <param name="tests">The available tests.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The impacted tests.</returns>
    Task<IReadOnlyCollection<TestCase>> GetImpactedTestsAsync(IEnumerable<string> changedFiles, IEnumerable<TestCase> tests, CancellationToken cancellationToken = default);
}
