namespace global::PeasyPilot.Core.Models;

/// <summary>
/// Defines the possible result states for a unified test run.
/// </summary>
public enum TestRunStatus
{
    NotStarted,
    Passed,
    Failed,
    PartiallyFailed,
    Skipped,
    Cancelled
}
