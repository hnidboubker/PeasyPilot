using PeasyPilot.Core.Eums;

namespace PeasyPilot.Core.Models;

/// <summary>
/// Represents the aggregated result of a full test pipeline execution.
/// </summary>
public sealed class TestPipelineResult
{
    /// <summary>
    /// Gets the number of tests discovered.
    /// </summary>
    public int DiscoveredCount { get; init; }

    /// <summary>
    /// Gets the number of tests selected after impact analysis.
    /// </summary>
    public int ImpactedCount { get; init; }

    /// <summary>
    /// Gets the number of tests scheduled for execution after filtering.
    /// </summary>
    public int ScheduledCount { get; init; }

    /// <summary>
    /// Gets the list of individual test execution results.
    /// </summary>
    public IReadOnlyCollection<TestResult> TestResults { get; init; } = [];

    /// <summary>
    /// Gets the aggregate test run result.
    /// </summary>
    public TestRunResult AggregateRunResult { get; init; } = new();

    /// <summary>
    /// Gets the list of diagnostic results generated during the run.
    /// </summary>
    public IReadOnlyCollection<TestDiagnosticResult> Diagnostics { get; init; } = [];

    /// <summary>
    /// Gets the overall status of the pipeline execution.
    /// </summary>
    public TestRunStatus Status => AggregateRunResult.Status;
}
