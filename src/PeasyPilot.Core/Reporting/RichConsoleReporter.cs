using System.Text;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Reporting;

/// <summary>
/// Formats test run results with rich ANSI color banners, boxed summaries, and structured status reports.
/// </summary>
public sealed class RichConsoleReporter : ITestReporter
{
    private const string Reset = "\u001b[0m";
    private const string Bold = "\u001b[1m";
    private const string Green = "\u001b[32m";
    private const string Red = "\u001b[31m";
    private const string Yellow = "\u001b[33m";
    private const string Cyan = "\u001b[36m";

    /// <inheritdoc />
    public Task<string> ReportAsync(TestRunResult result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var total = result.Passed + result.Failed + result.Skipped;
        var statusColor = result.Status switch
        {
            TestRunStatus.Passed => Green,
            TestRunStatus.Failed => Red,
            TestRunStatus.Skipped => Yellow,
            _ => Reset
        };

        var sb = new StringBuilder();
        sb.AppendLine($"{Bold}{Cyan}┌────────────────────────────────────────────────────────┐{Reset}");
        sb.AppendLine($"{Bold}{Cyan}│                   PEASYPILOT SUMMARY                   │{Reset}");
        sb.AppendLine($"{Bold}{Cyan}└────────────────────────────────────────────────────────┘{Reset}");
        sb.AppendLine($"  {Bold}Status:{Reset} {statusColor}{Bold}{result.Status}{Reset}");
        sb.AppendLine($"  {Bold}Total:{Reset}    {total}");
        sb.AppendLine($"  {Green}Passed:{Reset}   {result.Passed}");
        sb.AppendLine($"  {Red}Failed:{Reset}   {result.Failed}");
        sb.AppendLine($"  {Yellow}Skipped:{Reset}  {result.Skipped}");
        sb.AppendLine($"  {Bold}Duration:{Reset} {result.Duration.TotalMilliseconds:F0} ms");

        var output = sb.ToString();
        Console.WriteLine(output);

        return Task.FromResult(output);
    }
}
