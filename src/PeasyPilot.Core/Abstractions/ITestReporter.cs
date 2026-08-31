namespace PeasyPilot.Core.Abstractions;

using PeasyPilot.Core.Models;

/// <summary>
/// Reports the result of a test run in a framework-agnostic way.
/// </summary>
public interface ITestReporter
{
    /// <summary>
    /// Writes the run result to an output destination.
    /// </summary>
    /// <param name="result">The test run result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The serialized output.</returns>
    Task<string> ReportAsync(TestRunResult result, CancellationToken cancellationToken = default);
}
