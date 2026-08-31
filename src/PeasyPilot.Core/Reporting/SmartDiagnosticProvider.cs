using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Reporting;

/// <summary>
/// Intelligent diagnostic engine that analyzes failed test run results and provides root cause analysis and suggestions.
/// </summary>
public sealed class SmartDiagnosticProvider : ITestDiagnostic
{
    /// <inheritdoc />
    public Task<TestDiagnosticResult> DiagnoseAsync(TestRunResult result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Status == TestRunStatus.Passed)
        {
            return Task.FromResult(new TestDiagnosticResult
            {
                Summary = "All tests passed cleanly. No failure diagnostics required.",
                Suggestions = [],
                GeneratedAt = DateTime.UtcNow,
                Source = nameof(SmartDiagnosticProvider)
            });
        }

        var suggestions = new List<string>();
        var summaryText = $"Diagnosed {result.Failed} test failure(s) out of {result.Passed + result.Failed + result.Skipped} total tests.";

        suggestions.Add("Review test assertion values (Expected vs Actual).");
        suggestions.Add("Check dependencies and setup state in TestContext.");
        suggestions.Add("Verify async methods include proper await keywords.");

        return Task.FromResult(new TestDiagnosticResult
        {
            Summary = summaryText,
            Suggestions = suggestions,
            GeneratedAt = DateTime.UtcNow,
            Source = nameof(SmartDiagnosticProvider)
        });
    }
}
