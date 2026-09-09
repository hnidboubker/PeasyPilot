namespace PeasyPilot.TestAssistant.Models;
public class TestableScenario
{
    public required ScenarioType Type { get; init; }
    public required string Description { get; init; }
    public string? SuggestedTestName { get; init; }
    public RiskLevel RiskLevel { get; init; } = RiskLevel.Medium;
    public string TestCategory { get; init; } = "Unit";
}
public enum ScenarioType { HappyPath, Boundary, Error, Regression, Concurrency, DependencyFailure }
public enum RiskLevel { Low, Medium, High, Critical }
