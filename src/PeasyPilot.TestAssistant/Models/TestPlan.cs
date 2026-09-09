namespace PeasyPilot.TestAssistant.Models;

public class TestPlan
{
    public required string MethodName { get; init; }
    public required string TypeName { get; init; }

    public List<TestableScenario> Scenarios { get; init; } = new();

    public int TotalEstimatedTests { get; init; }

    public int RiskScore { get; init; }

    public decimal EstimatedCoverage { get; init; }

    public List<string> CoverageGaps { get; init; } = new();

    public string RecommendedFramework { get; init; } = "xunit";

    public bool RequiresIntegration { get; init; }

    public bool RequiresMocking { get; init; }

    public int ComplexityScore { get; init; }

    public List<string> RecommendedPatterns { get; init; } = new();
}
