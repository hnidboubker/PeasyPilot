namespace PeasyPilot.Core.Models;

using PeasyPilot.Core.Eums;

/// <summary>
/// Represents the execution outcome of a single unified test case.
/// </summary>
public class TestResult
{
    /// <summary>
    /// Gets or sets the test name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category or suite.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the execution status.
    /// </summary>
    public TestRunStatus Status { get; set; } = TestRunStatus.Passed;

    /// <summary>
    /// Gets or sets an optional execution message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets standardized failure details when status is Failed.
    /// </summary>
    public TestFailure? Failure { get; set; }

    /// <summary>
    /// Gets or sets the execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}
