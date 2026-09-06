namespace PeasyPilot.BDD.Execution;

/// <summary>
/// Execution status for a scenario.
/// </summary>
public enum ScenarioStatus
{
    /// <summary>
    /// Scenario passed all steps.
    /// </summary>
    Passed,

    /// <summary>
    /// Scenario failed (step validation or exception).
    /// </summary>
    Failed,

    /// <summary>
    /// Scenario was skipped.
    /// </summary>
    Skipped
}

/// <summary>
/// Result of executing a BDD scenario.
/// </summary>
public class ScenarioExecutionResult
{
    /// <summary>
    /// Gets the scenario name.
    /// </summary>
    public string ScenarioName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the execution status.
    /// </summary>
    public ScenarioStatus Status { get; set; }

    /// <summary>
    /// Gets the execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets the error message (if failed).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the step-level results.
    /// </summary>
    public IReadOnlyList<StepExecutionResult> Steps { get; set; } = new List<StepExecutionResult>();
}

/// <summary>
/// Result of executing a single step.
/// </summary>
public class StepExecutionResult
{
    /// <summary>
    /// Gets the step text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets the step type (Given, When, Then, etc).
    /// </summary>
    public StepType Type { get; set; }

    /// <summary>
    /// Gets whether the step passed.
    /// </summary>
    public bool Passed { get; set; }

    /// <summary>
    /// Gets the error (if step failed).
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}
