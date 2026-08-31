using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Reporting;
/// <summary>
/// Provides a default diagnostic strategy for failed results.
/// </summary>
public sealed class DefaultDiagnostic : ITestDiagnostic
{
    public Task<TestDiagnosticResult> DiagnoseAsync(TestRunResult result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        cancellationToken.ThrowIfCancellationRequested();

        var summary = result.Failed > 0
            ? $"Failed test run with {result.Failed} failing test(s)."
            : "No diagnostics required.";

        var rootCause = result.Failed > 0
            ? "The result indicates at least one failed execution; investigate the failing assertion or setup path."
            : null;

        return Task.FromResult(new TestDiagnosticResult(
            summary,
            rootCause,
            Array.Empty<string>(),
            result.Failed > 0
                ? ["Check the failing setup", "Review the assertion path", "Inspect the execution context"]
                : Array.Empty<string>()));
    }
}
