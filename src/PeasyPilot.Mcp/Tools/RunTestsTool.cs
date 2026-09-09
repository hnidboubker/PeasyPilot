using System.Diagnostics;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace PeasyPilot.Mcp.Tools;

/// <summary>
/// MCP Tool: Execute tests and report results
/// Input: projectPath, testFilter (optional), framework (optional)
/// Output: total, passed, failed, failures, duration
/// </summary>
public class RunTestsTool : IMcpTool
{
    public string Name => "run_tests";
    public string Description => "Execute tests and report results";

    public async Task<dynamic> ExecuteAsync(Dictionary<string, object> parameters)
    {
        try
        {
            var projectPath = parameters.TryGetValue("projectPath", out var projObj)
                ? projObj?.ToString()
                : throw new ArgumentException("projectPath is required");

            var testFilter = parameters.TryGetValue("testFilter", out var filterObj)
                ? filterObj?.ToString()
                : null;

            var framework = parameters.TryGetValue("framework", out var fwObj)
                ? fwObj?.ToString() ?? "xunit"
                : "xunit";

            if (!Directory.Exists(projectPath))
                throw new DirectoryNotFoundException($"Project path not found: {projectPath}");

            var result = await ExecuteTests(projectPath, testFilter, framework);

            dynamic response = new ExpandoObject();
            response.success = true;
            response.total = result.Total;
            response.passed = result.Passed;
            response.failed = result.Failed;
            response.failures = result.Failures;
            response.durationMs = result.DurationMs;
            response.summary = $"{result.Passed}/{result.Total} tests passed";
            return response;
        }
        catch (Exception ex)
        {
            dynamic response = new ExpandoObject();
            response.success = false;
            response.error = ex.Message;
            return response;
        }
    }

    private async Task<TestRunResult> ExecuteTests(string projectPath, string? testFilter, string framework)
    {
        var startTime = DateTime.UtcNow;

        var args = new List<string>
        {
            "test",
            projectPath,
            "--logger=console",
            "--no-build"
        };

        if (!string.IsNullOrEmpty(testFilter))
        {
            args.Add("--filter");
            args.Add(testFilter);
        }

        var psi = new ProcessStartInfo("dotnet", string.Join(" ", args))
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet test");

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var result = ParseTestOutput(output, error);
        result.DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

        return result;
    }

    private TestRunResult ParseTestOutput(string output, string error)
    {
        var result = new TestRunResult();

        // Try to parse the summary line: "X passed, Y failed"
        var summaryMatch = Regex.Match(output, @"(\d+)\s+passed(?:,\s+(\d+)\s+failed)?");
        if (summaryMatch.Success)
        {
            result.Passed = int.Parse(summaryMatch.Groups[1].Value);
            result.Failed = int.TryParse(summaryMatch.Groups[2].Value, out var failed) ? failed : 0;
            result.Total = result.Passed + result.Failed;
        }

        // Parse failures if any
        if (!string.IsNullOrEmpty(error))
        {
            result.Failures.Add(new { message = error, type = "EXECUTION_ERROR" });
        }

        return result;
    }

    private class TestRunResult
    {
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public List<object> Failures { get; set; } = new();
        public double DurationMs { get; set; }
    }
}
