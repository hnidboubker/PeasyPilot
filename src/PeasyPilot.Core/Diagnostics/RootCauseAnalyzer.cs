using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Diagnostics;

/// <summary>
/// Root cause analyzer for inspecting <see cref="TestFailure"/> objects and identifying primary cause categories.
/// </summary>
public static class RootCauseAnalyzer
{
    /// <summary>
    /// Analyzes a failure and returns an inferenced root cause category description.
    /// </summary>
    /// <param name="failure">Test failure instance.</param>
    /// <returns>Root cause category string.</returns>
    public static string AnalyzeRootCause(TestFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);

        if (!string.IsNullOrEmpty(failure.Expected) || !string.IsNullOrEmpty(failure.Actual))
        {
            return "Assertion Mismatch: The computed result did not match the expected assertion value.";
        }

        if (failure.Message.Contains("NullReferenceException", StringComparison.OrdinalIgnoreCase) ||
            (failure.StackTrace?.Contains("NullReferenceException", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return "Null Reference Error: An unhandled null reference occurred during test execution.";
        }

        if (failure.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase) ||
            failure.Message.Contains("TaskCanceledException", StringComparison.OrdinalIgnoreCase))
        {
            return "Timeout Error: Execution exceeded the allocated time limit or cancellation threshold.";
        }

        return "Unhandled Execution Exception: An uncaught exception was thrown by the code under test.";
    }
}
