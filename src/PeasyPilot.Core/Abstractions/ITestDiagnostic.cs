namespace PeasyPilot.Core.Abstractions;

using PeasyPilot.Core.Models;

/// <summary>
/// Produces a diagnostic explanation for a failed test result.
/// </summary>
public interface ITestDiagnostic
{
    /// <summary>
    /// Diagnoses a test result.
    /// </summary>
    /// <param name="result">The failed result to analyze.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The diagnostic result.</returns>
    Task<TestDiagnosticResult> DiagnoseAsync(TestRunResult result, CancellationToken cancellationToken = default);
}
