namespace global::PeasyPilot.Core.Models;

/// <summary>
/// Represents the outcome of a unified test run.
/// </summary>
public class TestRunResult
{
    /// <summary>
    /// Gets or sets the total number of passed tests.
    /// </summary>
    public int Passed { get; set; }

    /// <summary>
    /// Gets or sets the total number of failed tests.
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// Gets or sets the total number of skipped tests.
    /// </summary>
    public int Skipped { get; set; }

    /// <summary>
    /// Gets or sets the total duration of the run.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the final status of the run.
    /// </summary>
    public TestRunStatus Status { get; set; }
}
