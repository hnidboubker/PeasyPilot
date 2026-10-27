using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Execution;

/// <summary>
/// Merges the results of several test executables (for example TUnit and xUnit v3 assemblies) into one run.
/// </summary>
public static class TestRunAggregator
{
    /// <summary>
    /// Combines run results: totals and durations are added, per-test results are concatenated,
    /// and the status follows the same rule as a single run.
    /// </summary>
    /// <param name="runs">The run results to merge.</param>
    /// <returns>A single run result covering all of them.</returns>
    public static TestRunResult Merge(IEnumerable<TestRunResult> runs)
    {
        ArgumentNullException.ThrowIfNull(runs);

        var all = runs.ToList();
        var passed = all.Sum(r => r.Passed);
        var failed = all.Sum(r => r.Failed);
        var skipped = all.Sum(r => r.Skipped);

        return new TestRunResult
        {
            Passed = passed,
            Failed = failed,
            Skipped = skipped,
            Duration = all.Aggregate(TimeSpan.Zero, (total, r) => total + r.Duration),
            Status = passed + failed + skipped == 0
                ? TestRunStatus.NotStarted
                : failed > 0
                    ? TestRunStatus.Failed
                    : TestRunStatus.Passed,
            Tests = all.SelectMany(r => r.Tests).ToList()
        };
    }
}
