using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Discovery;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Filters;
using PeasyPilot.Core.ImpactAnalysis;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Orchestration;
using PeasyPilot.Core.Reporting;
using PeasyPilot.Core.Scheduling;
using PeasyPilot.Core.Storage;

namespace PeasyPilot.CLI;

/// <summary>
/// CLI runner engine for processing arguments, executing pipeline, and formatting output.
/// </summary>
public static class CliRunner
{
    /// <summary>
    /// Executes the CLI command with the given argument list.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <param name="discoveryOverride">Optional test discovery override (for testing CLI).</param>
    /// <param name="schedulerOverride">Optional test scheduler override (for testing CLI).</param>
    /// <param name="storeOverride">Optional test run store override (for testing CLI).</param>
    /// <returns>Exit code (0 for success, 1 for failure/errors).</returns>
    public static async Task<int> RunAsync(
        string[] args,
        ITestDiscovery? discoveryOverride = null,
        ITestScheduler? schedulerOverride = null,
        ITestRunStore? storeOverride = null)
    {
        if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h" || args[0] == "help"))
        {
            PrintHelp();
            return 0;
        }

        var store = storeOverride ?? new FileTestRunStore();

        if (args.Length > 0 && args[0] == "history")
        {
            return await ShowHistoryAsync(store);
        }

        if (args.Length > 0 && args[0] == "suggest-tests")
        {
            return await SuggestTestsCommand.RunAsync(args[1..]);
        }

        // Parse arguments for test execution
        string? filterName = null;
        string[]? changedFiles = null;
        var format = "console";
        string? outputPath = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--filter" or "-f":
                    if (i + 1 < args.Length) filterName = args[++i];
                    break;
                case "--changed-files" or "-c":
                    if (i + 1 < args.Length)
                    {
                        changedFiles = args[++i].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    }
                    break;
                case "--format" or "-fmt":
                    if (i + 1 < args.Length) format = args[++i].ToLowerInvariant();
                    break;
                case "--output" or "-o":
                    if (i + 1 < args.Length) outputPath = args[++i];
                    break;
            }
        }

        var discovery = discoveryOverride ?? new DefaultTestDiscovery();
        var scheduler = schedulerOverride ?? new DefaultTestScheduler();
        var impactAnalyzer = new DefaultTestImpactAnalyzer();

        var orchestrator = new TestPipelineOrchestrator(discovery, scheduler, impactAnalyzer);

        var reporters = new List<ITestReporter> { new ConsoleReporter() };

        if (format == "json" || (!string.IsNullOrEmpty(outputPath) && outputPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
        {
            reporters.Add(new JsonFileReporter(outputPath));
        }
        else if (format == "junit" || (!string.IsNullOrEmpty(outputPath) && outputPath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
        {
            reporters.Add(new JUnitXmlReporter(outputPath));
        }

        var pipelineOptions = new TestPipelineOptions
        {
            ChangedFiles = changedFiles,
            Filter = !string.IsNullOrWhiteSpace(filterName) ? new NameTestFilter(filterName) : null,
            Reporters = reporters,
            RunDiagnosticsOnFailure = true
        };

        Console.WriteLine("[PeasyPilot CLI] Executing Test Pipeline...");
        var result = await orchestrator.ExecutePipelineAsync(pipelineOptions);

        // Save history
        var record = new TestRunRecord
        {
            ExecutedAt = DateTime.UtcNow,
            PipelineResult = result
        };
        await store.SaveRunAsync(record);

        Console.WriteLine($"[PeasyPilot CLI] Status: {result.Status} | Discovered: {result.DiscoveredCount} | Scheduled: {result.ScheduledCount} | Passed: {result.AggregateRunResult.Passed} | Failed: {result.AggregateRunResult.Failed}");

        return result.Status == TestRunStatus.Failed ? 1 : 0;
    }

    private static async Task<int> ShowHistoryAsync(ITestRunStore store)
    {
        Console.WriteLine("[PeasyPilot CLI] Test Execution History:");
        var history = await store.GetRunHistoryAsync(limit: 10);

        if (history.Count == 0)
        {
            Console.WriteLine("No historical test runs recorded.");
            return 0;
        }

        foreach (var run in history)
        {
            Console.WriteLine($"[{run.ExecutedAt:yyyy-MM-dd HH:mm:ss}] Run ID: {run.RunId} | Status: {run.PipelineResult.Status} | Discovered: {run.PipelineResult.DiscoveredCount} | Passed: {run.PipelineResult.AggregateRunResult.Passed} | Failed: {run.PipelineResult.AggregateRunResult.Failed}");
        }

        return 0;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("PeasyPilot CLI - Unified Test Runner");
        Console.WriteLine("Usage:");
        Console.WriteLine("  peasypilot [options]                                       Run tests");
        Console.WriteLine("  peasypilot history                                         Show test execution history");
        Console.WriteLine("  peasypilot suggest-tests --assembly <path> --type <name>  Generate test suite proposal");
        Console.WriteLine();
        Console.WriteLine("Test Execution Options:");
        Console.WriteLine("  -f, --filter <name>         Filter tests by name (case insensitive)");
        Console.WriteLine("  -c, --changed-files <f1,f2> Run impact analysis for comma-separated changed files");
        Console.WriteLine("  -fmt, --format <format>     Output format: console, json, junit");
        Console.WriteLine("  -o, --output <path>         File path to save JSON or JUnit report");
        Console.WriteLine();
        Console.WriteLine("Test Suggestion Options:");
        Console.WriteLine("  -a, --assembly <path>       Assembly path to analyze (required)");
        Console.WriteLine("  -t, --type <name>           Target type name to generate tests for (required)");
        Console.WriteLine("  -fw, --framework <fw>       Target framework: xunit, nunit, tunit (default: xunit)");
        Console.WriteLine("  -o, --output-dir <dir>      Output directory (default: ./generated-tests)");
        Console.WriteLine("  -fmt, --format <fmt>        Output format: json, cs, both (default: json)");
        Console.WriteLine("  --force                     Overwrite existing proposal files");
        Console.WriteLine();
        Console.WriteLine("  -h, --help                  Show help information");
    }
}
