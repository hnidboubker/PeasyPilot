using System.Collections.Concurrent;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Diagnostics;

/// <summary>
/// Performance profiling tracker that monitors test execution durations and flags regressions against baseline thresholds.
/// </summary>
public sealed class PerformanceTracker
{
    private readonly ConcurrentDictionary<string, double> _baselinesMs = new();

    /// <summary>
    /// Sets a performance duration baseline for a test.
    /// </summary>
    /// <param name="testName">Test case name.</param>
    /// <param name="baselineDurationMs">Baseline duration in milliseconds.</param>
    public void SetBaseline(string testName, double baselineDurationMs)
    {
        _baselinesMs[testName] = baselineDurationMs;
    }

    /// <summary>
    /// Evaluates if a test result exhibits a performance regression (exceeding threshold multiplier).
    /// </summary>
    /// <param name="result">The test result to evaluate.</param>
    /// <param name="thresholdMultiplier">Regression factor multiplier (default 2.0x).</param>
    /// <returns>True if test execution regressed significantly; otherwise false.</returns>
    public bool IsPerformanceRegressed(TestResult result, double thresholdMultiplier = 2.0)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (_baselinesMs.TryGetValue(result.Name, out var baseline))
        {
            return result.Duration.TotalMilliseconds > (baseline * thresholdMultiplier);
        }

        return false;
    }
}
