namespace PeasyPilot.Core.Models;

/// <summary>
/// Represents a historical record of a test pipeline run.
/// </summary>
public sealed class TestRunRecord
{
    /// <summary>
    /// Gets the unique identifier for the test run.
    /// </summary>
    public string RunId { get; init; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the date and time when the test run was executed.
    /// </summary>
    public DateTime ExecutedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the pipeline execution result.
    /// </summary>
    public TestPipelineResult PipelineResult { get; init; } = new();

    /// <summary>
    /// Gets custom metadata associated with the test run.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
