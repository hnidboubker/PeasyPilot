namespace PeasyPilot.TestAssistant.Models;

/// <summary>
/// Represents a testable scenario for a method.
/// </summary>
public class TestableScenario
{
    /// <summary>
    /// Type of scenario
    /// </summary>
    public required ScenarioType Type { get; init; }

    /// <summary>
    /// Human-readable description of the scenario
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Suggested test method name
    /// </summary>
    public string? SuggestedTestName { get; init; }

    /// <summary>
    /// Risk level if this scenario is not tested
    /// </summary>
    public RiskLevel RiskLevel { get; init; } = RiskLevel.Medium;

    /// <summary>
    /// Test category (Unit, Integration, BDD, etc.)
    /// </summary>
    public string TestCategory { get; init; } = "Unit";
}

/// <summary>
/// Type of test scenario
/// </summary>
public enum ScenarioType
{
    /// <summary>
    /// Happy path / nominal case
    /// </summary>
    HappyPath,

    /// <summary>
    /// Boundary condition (min, max, zero, etc.)
    /// </summary>
    Boundary,

    /// <summary>
    /// Error / exception case
    /// </summary>
    Error,

    /// <summary>
    /// Regression risk
    /// </summary>
    Regression,

    /// <summary>
    /// Async / concurrency
    /// </summary>
    Concurrency,

    /// <summary>
    /// Dependency failure
    /// </summary>
    DependencyFailure
}

/// <summary>
/// Risk level for unimplemented test
/// </summary>
public enum RiskLevel
{
    /// <summary>
    /// Low risk
    /// </summary>
    Low,

    /// <summary>
    /// Medium risk
    /// </summary>
    Medium,

    /// <summary>
    /// High risk
    /// </summary>
    High,

    /// <summary>
    /// Critical risk
    /// </summary>
    Critical
}
