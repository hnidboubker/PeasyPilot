namespace PeasyPilot.Core.Models;

/// <summary>
/// Represents standardized failure details for a failed test.
/// </summary>
public sealed record TestFailure
{
    /// <summary>
    /// Gets the failure error message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the stack trace associated with the failure.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    /// Gets the expected value string representation, if applicable.
    /// </summary>
    public string? Expected { get; init; }

    /// <summary>
    /// Gets the actual value string representation, if applicable.
    /// </summary>
    public string? Actual { get; init; }
}
