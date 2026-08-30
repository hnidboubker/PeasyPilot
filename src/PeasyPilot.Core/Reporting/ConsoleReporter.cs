using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Reporting;
/// <summary>
/// Simple console reporter for a test run result.
/// </summary>
public sealed class ConsoleReporter : ITestReporter
{
    public Task<string> ReportAsync(TestRunResult result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        cancellationToken.ThrowIfCancellationRequested();

        var summary = $"PeasyPilot Result | Passed: {result.Passed} | Failed: {result.Failed} | Skipped: {result.Skipped} | Duration: {result.Duration:c} | Status: {result.Status}";
        return Task.FromResult(summary);
    }
}
