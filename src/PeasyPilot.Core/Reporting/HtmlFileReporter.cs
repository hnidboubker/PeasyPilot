using System.Text;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Reporting;

/// <summary>
/// Generates an interactive standalone HTML dashboard report for test run results.
/// </summary>
public sealed class HtmlFileReporter : ITestReporter
{
    private readonly string? _filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlFileReporter"/> class.
    /// </summary>
    /// <param name="filePath">Optional file path to save the HTML report.</param>
    public HtmlFileReporter(string? filePath = null)
    {
        _filePath = filePath;
    }

    /// <inheritdoc />
    public async Task<string> ReportAsync(TestRunResult result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var total = result.Passed + result.Failed + result.Skipped;
        var passRate = total > 0 ? (double)result.Passed / total * 100 : 0;

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <title>PeasyPilot Test Report</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #0f172a; color: #f8fafc; padding: 2rem; }");
        sb.AppendLine("    .card { background: #1e293b; border-radius: 8px; padding: 1.5rem; margin-bottom: 1rem; border: 1px solid #334155; }");
        sb.AppendLine("    .grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1rem; }");
        sb.AppendLine("    .stat { text-align: center; }");
        sb.AppendLine("    .stat-val { font-size: 2rem; font-weight: bold; }");
        sb.AppendLine("    .passed { color: #4ade80; } .failed { color: #f87171; } .skipped { color: #fbbf24; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <h1>🚀 PeasyPilot Test Execution Dashboard</h1>");
        sb.AppendLine("  <div class=\"grid\">");
        sb.AppendLine($"    <div class=\"card stat\"><div class=\"stat-val\">{total}</div><div>Total Tests</div></div>");
        sb.AppendLine($"    <div class=\"card stat\"><div class=\"stat-val passed\">{result.Passed}</div><div>Passed</div></div>");
        sb.AppendLine($"    <div class=\"card stat\"><div class=\"stat-val failed\">{result.Failed}</div><div>Failed</div></div>");
        sb.AppendLine($"    <div class=\"card stat\"><div class=\"stat-val skipped\">{result.Skipped}</div><div>Skipped</div></div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine($"    <h3>Execution Status: <span class=\"{(result.Failed > 0 ? "failed" : "passed")}\">{result.Status}</span></h3>");
        sb.AppendLine($"    <p>Pass Rate: <strong>{passRate:F1}%</strong> | Duration: <strong>{result.Duration.TotalSeconds:F2}s</strong></p>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        var html = sb.ToString();

        if (!string.IsNullOrWhiteSpace(_filePath))
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(_filePath, html, cancellationToken);
        }

        return html;
    }
}
