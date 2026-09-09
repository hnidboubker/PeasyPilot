namespace PeasyPilot.TestAssistant.Models;

public class TestChallengeResult
{
    public required string TestName { get; init; }

    public int QualityScore { get; init; }

    public List<string> Issues { get; init; } = new();

    public List<string> Suggestions { get; init; } = new();

    public List<string> MissingScenarios { get; init; } = new();

    public bool PassesBasicValidation { get; init; }

    public decimal EstimatedCoverage { get; init; }

    public int ComplexityScore { get; init; }

    public List<string> RecommendedImprovements { get; init; } = new();
}

public class TestChallengeReport
{
    public required string MethodName { get; init; }

    public int TotalTestsChallenged { get; init; }

    public int PassedCount { get; init; }

    public decimal AverageQualityScore { get; init; }

    public List<TestChallengeResult> Results { get; init; } = new();

    public List<string> CriticalIssues { get; init; } = new();

    public string OverallRecommendation { get; init; } = string.Empty;
}
