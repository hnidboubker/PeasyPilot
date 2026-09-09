using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Planning;

public class TestQualityScorer
{
    public int ScoreRisk(MethodTestModel model)
    {
        int score = 0;

        if (model.IsAsync) score += 2;
        if (model.Dependencies.Any(d => d.IsRequired)) score += 2;
        if (model.PossibleExceptions.Any()) score += model.PossibleExceptions.Count;
        if (model.Parameters.Any(p => p.IsNullable)) score += 1;
        if (model.ComplexityScore > 5) score += model.ComplexityScore - 5;

        return Math.Min(score, 10);
    }

    public decimal CalculateCoverage(MethodTestModel model)
    {
        decimal base_coverage = 0.5m;

        int scenario_count = model.TestableScenarios.Count;
        scenario_count = Math.Max(scenario_count, 1);
        decimal scenario_bonus = Math.Min(scenario_count * 0.1m, 0.3m);

        bool has_dependency_coverage = model.Dependencies.All(d => d.IsRequired);
        decimal dependency_bonus = has_dependency_coverage ? 0.1m : 0m;

        bool has_exception_coverage = model.PossibleExceptions.Any();
        decimal exception_bonus = has_exception_coverage ? 0.05m : 0m;

        return Math.Min(base_coverage + scenario_bonus + dependency_bonus + exception_bonus, 1m);
    }

    public List<string> IdentifyGaps(MethodTestModel model)
    {
        var gaps = new List<string>();

        if (!model.TestableScenarios.Any(s => s.Type == ScenarioType.HappyPath))
            gaps.Add("Missing happy path scenario");

        if (!model.TestableScenarios.Any(s => s.Type == ScenarioType.Boundary))
            gaps.Add("Missing boundary value scenarios");

        if (model.PossibleExceptions.Any() && !model.TestableScenarios.Any(s => s.Type == ScenarioType.Error))
            gaps.Add("Missing error/exception scenarios");

        if (model.Dependencies.Any() && !model.TestableScenarios.Any(s => s.Type == ScenarioType.DependencyFailure))
            gaps.Add("Missing dependency failure scenarios");

        if (model.IsAsync && !model.TestableScenarios.Any(s => s.Type == ScenarioType.Concurrency))
            gaps.Add("Missing concurrency/async scenarios for async methods");

        return gaps;
    }

    public int EstimateTestCount(MethodTestModel model)
    {
        int base_count = 1;

        base_count += model.Parameters.Count;
        base_count += model.TestableScenarios.Count(s => s.Type == ScenarioType.Boundary);
        base_count += model.PossibleExceptions.Count;
        base_count += model.Dependencies.Count(d => d.IsRequired);

        if (model.IsAsync) base_count += 1;

        return Math.Max(base_count, 2);
    }
}
