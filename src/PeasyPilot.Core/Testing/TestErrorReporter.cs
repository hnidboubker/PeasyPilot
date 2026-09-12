using System.Text;

namespace PeasyPilot.Core.Testing;

/// <summary>
/// Centralized error reporter that captures and persists test errors for CI/CD visibility.
/// Generates markdown reports and console output for failed tests.
/// </summary>
public static class TestErrorReporter
{
    private static readonly List<TestFailureRecord> _failures = new();
    private static readonly object _lockObject = new object();
    private const string FAILURES_FILE = "test-failures.md";
    private const string ERRORS_DIR = ".test-errors";

    /// <summary>
    /// Registers a test failure with full error details.
    /// </summary>
    public static void RegisterFailure(string testName, string testClass, List<TestError> errors, long durationMs)
    {
        lock (_lockObject)
        {
            var record = new TestFailureRecord
            {
                TestName = testName,
                TestClass = testClass,
                Errors = new List<TestError>(errors),
                Duration = durationMs,
                Timestamp = DateTime.Now
            };
            _failures.Add(record);
        }
    }

    /// <summary>
    /// Generates and saves error report for CI/CD.
    /// </summary>
    public static void GenerateReport()
    {
        lock (_lockObject)
        {
            if (_failures.Count == 0)
                return;

            var report = BuildReport();
            SaveReport(report);
            PrintToConsole(report);
        }
    }

    /// <summary>
    /// Gets all registered failures.
    /// </summary>
    public static IReadOnlyList<TestFailureRecord> GetFailures() => _failures.AsReadOnly();

    /// <summary>
    /// Clears all registered failures.
    /// </summary>
    public static void Clear()
    {
        lock (_lockObject)
        {
            _failures.Clear();
        }
    }

    private static string BuildReport()
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Test Failure Report");
        sb.AppendLine();
        sb.AppendLine($"**Generated**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"**Total Failures**: {_failures.Count}");
        sb.AppendLine();

        foreach (var failure in _failures)
        {
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine($"## {failure.TestClass}.{failure.TestName}");
            sb.AppendLine();
            sb.AppendLine($"- **Time**: {failure.Timestamp:HH:mm:ss.fff}");
            sb.AppendLine($"- **Duration**: {failure.Duration}ms");
            sb.AppendLine($"- **Errors**: {failure.Errors.Count}");
            sb.AppendLine();

            sb.AppendLine("### Error Details");
            sb.AppendLine();

            for (int i = 0; i < failure.Errors.Count; i++)
            {
                var error = failure.Errors[i];
                sb.AppendLine($"**Error {i + 1}**: `{error.ErrorType}`");
                sb.AppendLine();
                sb.AppendLine($"```");
                sb.AppendLine(error.Message);
                sb.AppendLine("```");
                sb.AppendLine();

                if (!string.IsNullOrEmpty(error.StackTrace))
                {
                    sb.AppendLine("**Stack Trace**:");
                    sb.AppendLine();
                    sb.AppendLine("```");
                    sb.AppendLine(error.StackTrace);
                    sb.AppendLine("```");
                    sb.AppendLine();
                }
            }
        }

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine($"- Total test failures: **{_failures.Count}**");
        sb.AppendLine($"- Total errors: **{_failures.Sum(f => f.Errors.Count)}**");
        sb.AppendLine($"- Total time: **{_failures.Sum(f => f.Duration)}ms**");

        return sb.ToString();
    }

    private static void SaveReport(string report)
    {
        try
        {
            // Create directory if needed
            Directory.CreateDirectory(ERRORS_DIR);

            // Save main report
            var reportPath = Path.Combine(ERRORS_DIR, FAILURES_FILE);
            File.WriteAllText(reportPath, report);

            // Save JSON summary for CI parsing
            SaveJsonSummary();

            Console.WriteLine($"✅ Failure report saved to: {reportPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to save report: {ex.Message}");
        }
    }

    private static void SaveJsonSummary()
    {
        try
        {
            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"  \"timestamp\": \"{DateTime.Now:O}\",");
            json.AppendLine($"  \"totalFailures\": {_failures.Count},");
            json.AppendLine($"  \"totalErrors\": {_failures.Sum(f => f.Errors.Count)},");
            json.AppendLine("  \"failures\": [");

            for (int i = 0; i < _failures.Count; i++)
            {
                var failure = _failures[i];
                json.AppendLine("    {");
                json.AppendLine($"      \"testClass\": \"{failure.TestClass}\",");
                json.AppendLine($"      \"testName\": \"{failure.TestName}\",");
                json.AppendLine($"      \"timestamp\": \"{failure.Timestamp:O}\",");
                json.AppendLine($"      \"duration\": {failure.Duration},");
                json.AppendLine($"      \"errorCount\": {failure.Errors.Count}");
                json.AppendLine($"    }}{(i < _failures.Count - 1 ? "," : "")}");
            }

            json.AppendLine("  ]");
            json.AppendLine("}");

            var jsonPath = Path.Combine(ERRORS_DIR, "failures-summary.json");
            File.WriteAllText(jsonPath, json.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to save JSON summary: {ex.Message}");
        }
    }

    private static void PrintToConsole(string report)
    {
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    TEST FAILURE REPORT                          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine(report);
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║  ❌ {_failures.Count} test(s) failed with {_failures.Sum(f => f.Errors.Count)} total error(s)");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
    }
}

/// <summary>
/// Record of a test failure for reporting.
/// </summary>
public class TestFailureRecord
{
    /// <summary>
    /// Test class name.
    /// </summary>
    public string TestClass { get; set; } = string.Empty;

    /// <summary>
    /// Test method name.
    /// </summary>
    public string TestName { get; set; } = string.Empty;

    /// <summary>
    /// Errors that occurred during the test.
    /// </summary>
    public List<TestError> Errors { get; set; } = new();

    /// <summary>
    /// Test execution duration in milliseconds.
    /// </summary>
    public long Duration { get; set; }

    /// <summary>
    /// When the test failed.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
