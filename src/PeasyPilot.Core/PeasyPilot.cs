namespace PeasyPilot.Core;

using global::PeasyPilot.Core.Assertions;
using global::PeasyPilot.Core.Models;

/// <summary>
/// Entry point for framework-free, attribute-less testing.
/// </summary>
public static class PeasyPilot
{
    /// <summary>
    /// Executes a test body without requiring xUnit/NUnit/TUnit attributes.
    /// </summary>
    /// <param name="name">The logical name of the test.</param>
    /// <param name="testAction">The test action to execute.</param>
    /// <returns>The aggregated result of the test execution.</returns>
    public static TestRunResult Test(string name, Action testAction)
    {
        ArgumentNullException.ThrowIfNull(testAction);

        var startedAt = DateTime.UtcNow;

        try
        {
            testAction();

            return new TestRunResult
            {
                Passed = 1,
                Failed = 0,
                Skipped = 0,
                Duration = DateTime.UtcNow - startedAt,
                Status = TestRunStatus.Passed
            };
        }
        catch
        {
            return new TestRunResult
            {
                Passed = 0,
                Failed = 1,
                Skipped = 0,
                Duration = DateTime.UtcNow - startedAt,
                Status = TestRunStatus.Failed
            };
        }
    }

    /// <summary>
    /// Creates a fluent assertion builder without requiring a framework-specific assertion type.
    /// </summary>
    /// <typeparam name="T">The type under test.</typeparam>
    /// <param name="actual">The actual value.</param>
    /// <param name="message">Optional assertion message.</param>
    /// <returns>A fluent assertion builder.</returns>
    public static AssertThat<T> Expect<T>(T actual, string? message = null)
        => new(actual, message);
}
