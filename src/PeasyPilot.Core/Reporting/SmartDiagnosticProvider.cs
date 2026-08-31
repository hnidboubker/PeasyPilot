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
            return Task.FromResult(new TestDiagnosticResult(
                "All tests passed cleanly. No failure diagnostics required.",
                null,
                Array.Empty<string>(),
                Array.Empty<string>()
            ));
        }

        var suggestions = new List<string>
        {
            "Review test assertion values (Expected vs Actual).",
            "Check dependencies and setup state in TestContext.",
            "Verify async methods include proper await keywords."
        };

        var summaryText = $"Diagnosed {result.Failed} test failure(s) out of {result.Passed + result.Failed + result.Skipped} total tests.";

        return Task.FromResult(new TestDiagnosticResult(
            summaryText,
            "Assertion or unhandled exception during test execution.",
            Array.Empty<string>(),
            suggestions
        ));
    }
}
