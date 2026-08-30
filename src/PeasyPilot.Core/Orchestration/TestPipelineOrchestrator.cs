using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Orchestration;

/// <summary>
/// Orchestrates the end-to-end test execution pipeline combining discovery, impact analysis,
/// filtering, scheduling, reporting, and failure diagnostics.
/// </summary>
public sealed class TestPipelineOrchestrator : ITestPipelineOrchestrator
{
    private readonly ITestDiscovery _discovery;
    private readonly ITestScheduler _scheduler;
    private readonly ITestImpactAnalyzer? _impactAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestPipelineOrchestrator"/> class.
    /// </summary>
    /// <param name="discovery">The test discovery provider.</param>
    /// <param name="scheduler">The test scheduler provider.</param>
    /// <param name="impactAnalyzer">The optional impact analyzer provider.</param>
    public TestPipelineOrchestrator(
        ITestDiscovery discovery,
        ITestScheduler scheduler,
        ITestImpactAnalyzer? impactAnalyzer = null)
    {
        _discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _impactAnalyzer = impactAnalyzer;
    }

    /// <inheritdoc />
    public async Task<TestPipelineResult> ExecutePipelineAsync(
        TestPipelineOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        var startTime = DateTime.UtcNow;

        // 1. Discovery
        var discoveredTests = await _discovery.DiscoverAsync(cancellationToken);
        var discoveredCount = discoveredTests.Count;

        // 2. Impact Analysis
        IEnumerable<TestCase> selectedTests = discoveredTests;
        if (options.ChangedFiles is { Count: > 0 } changedFiles && _impactAnalyzer != null)
        {
            selectedTests = await _impactAnalyzer.GetImpactedTestsAsync(changedFiles, discoveredTests, cancellationToken);
        }
        var impactedTestsList = selectedTests.ToList();
        var impactedCount = impactedTestsList.Count;

        // 3. Filtering
        IEnumerable<TestCase> filteredTests = impactedTestsList;
        if (options.Filter != null)
        {
            filteredTests = impactedTestsList.Where(t => options.Filter.IsMatch(t));
        }
        var scheduledTestsList = filteredTests.ToList();
        var scheduledCount = scheduledTestsList.Count;

        // 4. Scheduling & Execution
        var testResults = await _scheduler.ExecuteAsync(scheduledTestsList, cancellationToken);

        // 5. Aggregate Results
        var passed = testResults.Count(r => r.Status == TestRunStatus.Passed);
        var failed = testResults.Count(r => r.Status == TestRunStatus.Failed);
        var skipped = testResults.Count(r => r.Status == TestRunStatus.Skipped);

        var status = failed > 0
            ? TestRunStatus.Failed
            : skipped > 0 && passed == 0
                ? TestRunStatus.Skipped
                : TestRunStatus.Passed;

        var duration = DateTime.UtcNow - startTime;

        var aggregateRunResult = new TestRunResult
        {
            Passed = passed,
            Failed = failed,
            Skipped = skipped,
            Duration = duration,
            Status = status
        };

        // 6. Reporting
        foreach (var reporter in options.Reporters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await reporter.ReportAsync(aggregateRunResult, cancellationToken);
        }

        // 7. Diagnostics (if failed & enabled)
        var diagnosticResults = new List<TestDiagnosticResult>();
        if (options.RunDiagnosticsOnFailure && failed > 0)
        {
            foreach (var diagnosticProvider in options.Diagnostics)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var diagResult = await diagnosticProvider.DiagnoseAsync(aggregateRunResult, cancellationToken);
                diagnosticResults.Add(diagResult);
            }
        }

        return new TestPipelineResult
        {
            DiscoveredCount = discoveredCount,
            ImpactedCount = impactedCount,
            ScheduledCount = scheduledCount,
            TestResults = testResults,
            AggregateRunResult = aggregateRunResult,
            Diagnostics = diagnosticResults
        };
    }
}
