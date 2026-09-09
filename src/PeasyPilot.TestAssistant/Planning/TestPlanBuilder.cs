using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Planning;

public class TestPlanBuilder : ITestPlanBuilder
{
    private readonly TestQualityScorer _scorer;
    private readonly DependencyAnalyzer _dependencyAnalyzer;

    public TestPlanBuilder(TestQualityScorer? scorer = null, DependencyAnalyzer? dependencyAnalyzer = null)
    {
        _scorer = scorer ?? new TestQualityScorer();
        _dependencyAnalyzer = dependencyAnalyzer ?? new DependencyAnalyzer();
    }

    public TestPlan BuildPlan(MethodTestModel analysis)
    {
        var plan = new TestPlan
        {
            TypeName = analysis.TypeName,
            MethodName = analysis.MethodName,
            Scenarios = analysis.TestableScenarios,
            ComplexityScore = analysis.ComplexityScore,
            TotalEstimatedTests = _scorer.EstimateTestCount(analysis),
            RiskScore = _scorer.ScoreRisk(analysis),
            EstimatedCoverage = _scorer.CalculateCoverage(analysis),
            CoverageGaps = _scorer.IdentifyGaps(analysis),
            RecommendedFramework = analysis.SuggestedFramework,
            RequiresIntegration = DetermineIntegrationNeed(analysis),
            RequiresMocking = DetermineMockingNeed(analysis),
            RecommendedPatterns = BuildRecommendedPatterns(analysis)
        };

        return plan;
    }

    public TestPlan EnrichPlan(TestPlan plan, List<DependencyInfo> dependencies)
    {
        if (!dependencies.Any())
            return plan;

        var grouped = _dependencyAnalyzer.GroupByStrategy(dependencies);

        var enrichedPlan = new TestPlan
        {
            TypeName = plan.TypeName,
            MethodName = plan.MethodName,
            Scenarios = plan.Scenarios,
            ComplexityScore = plan.ComplexityScore,
            TotalEstimatedTests = plan.TotalEstimatedTests,
            RiskScore = plan.RiskScore,
            EstimatedCoverage = plan.EstimatedCoverage,
            CoverageGaps = plan.CoverageGaps,
            RecommendedFramework = plan.RecommendedFramework,
            RequiresIntegration = plan.RequiresIntegration || grouped.ContainsKey("IntegrationTestFixture"),
            RequiresMocking = plan.RequiresMocking || grouped.ContainsKey("Moq.Mock"),
            RecommendedPatterns = EnrichPatterns(plan.RecommendedPatterns, grouped)
        };

        return enrichedPlan;
    }

    private bool DetermineIntegrationNeed(MethodTestModel model)
    {
        return model.Dependencies.Any(d =>
            d.InterfaceName.Contains("DbContext") ||
            d.InterfaceName.Contains("Repository") ||
            d.InterfaceName.Contains("Database"));
    }

    private bool DetermineMockingNeed(MethodTestModel model)
    {
        return model.Dependencies.Any(d =>
            !d.InterfaceName.Contains("DbContext") &&
            !d.InterfaceName.Contains("Database"));
    }

    private List<string> BuildRecommendedPatterns(MethodTestModel model)
    {
        var patterns = new List<string>();

        if (model.Parameters.Any())
            patterns.Add("Arrange-Act-Assert");

        if (model.Dependencies.Any())
            patterns.Add("Dependency Injection");

        if (model.IsAsync)
            patterns.Add("Async Testing");

        if (model.PossibleExceptions.Any())
            patterns.Add("Exception Handling");

        if (model.TestableScenarios.Any(s => s.Type == ScenarioType.Boundary))
            patterns.Add("Boundary Value Testing");

        return patterns;
    }

    private List<string> EnrichPatterns(List<string> basePatterns, Dictionary<string, List<DependencyInfo>> grouped)
    {
        var enriched = new List<string>(basePatterns);

        if (grouped.ContainsKey("Moq.Mock"))
            enriched.Add("Mocking Pattern");

        if (grouped.ContainsKey("IntegrationTestFixture"))
            enriched.Add("Integration Testing");

        return enriched.Distinct().ToList();
    }
}
