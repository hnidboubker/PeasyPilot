using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Generation;
using PeasyPilot.TestAssistant.Models;
using PeasyPilot.TestAssistant.Planning;
using PeasyPilot.TestAssistant.Validation;

namespace PeasyPilot.TestAssistant.Orchestration;

/// <summary>
/// Complete AI Test Engineer - orchestrates all tiers.
/// </summary>
public class AITestEngineer : IAITestEngineer
{
    private readonly ICodeAnalyzer _analyzer;
    private readonly ITestPlanBuilder _planner;
    private readonly TestGeneratorRegistry _generatorRegistry;
    private readonly ITestChallenger _challenger;

    public AITestEngineer(
        ICodeAnalyzer? analyzer = null,
        ITestPlanBuilder? planner = null,
        TestGeneratorRegistry? generatorRegistry = null,
        ITestChallenger? challenger = null)
    {
        _analyzer = analyzer ?? new CSharpCodeAnalyzer();
        _planner = planner ?? new TestPlanBuilder();
        _generatorRegistry = generatorRegistry ?? new TestGeneratorRegistry();
        _challenger = challenger ?? new TestChallenger();
    }

    public MethodTestModel AnalyzeMethod(string typeName, string methodName)
    {
        // Try to resolve the type, but allow null for mock/dummy analyzers
        var type = Type.GetType(typeName);
        if (type == null && !typeName.Contains("."))
            throw new ArgumentException($"Type '{typeName}' not found");

        var result = _analyzer.AnalyzeMethodAsync(type, methodName).Result;
        return result ?? throw new InvalidOperationException($"Method '{methodName}' not found on type '{typeName}'");
    }

    public List<MethodTestModel> AnalyzeType(Type type)
    {
        return _analyzer.AnalyzeTypeAsync(type).Result.ToList();
    }

    public TestPlan PlanTests(MethodTestModel analysis)
    {
        return _planner.BuildPlan(analysis);
    }

    public string GenerateTests(TestPlan plan, string @namespace, string framework = "xunit")
    {
        var generator = _generatorRegistry.GetGenerator(framework);
        return generator.GenerateTestClass(plan, @namespace);
    }

    public TestChallengeResult ChallengeTests(string testCode, MethodTestModel model)
    {
        return _challenger.ChallengeTest(testCode, model);
    }

    public AITestEngineerResult ExecuteCompleteWorkflow(string typeName, string methodName, string @namespace, string framework = "xunit")
    {
        // Step 1: Analyze
        var analysis = AnalyzeMethod(typeName, methodName);

        // Step 2: Plan
        var plan = PlanTests(analysis);

        // Step 3: Generate
        var generatedCode = GenerateTests(plan, @namespace, framework);

        // Step 4: Challenge
        var challenge = ChallengeTests(generatedCode, analysis);

        // Step 5: Calculate overall quality
        var overallScore = (challenge.QualityScore + plan.RiskScore * 10) / 2;
        var isProduction = challenge.PassesBasicValidation && overallScore >= 70;

        // Step 6: Generate next steps
        var nextSteps = GenerateNextSteps(challenge, plan);

        return new AITestEngineerResult
        {
            MethodName = methodName,
            Analysis = analysis,
            Plan = plan,
            GeneratedCode = generatedCode,
            Challenge = challenge,
            TotalQualityScore = (int)overallScore,
            IsRecommendedForProduction = isProduction,
            NextSteps = nextSteps
        };
    }

    private List<string> GenerateNextSteps(TestChallengeResult challenge, TestPlan plan)
    {
        var steps = new List<string>();

        if (challenge.Issues.Any())
            steps.Add($"Fix {challenge.Issues.Count} identified issues");

        if (challenge.Suggestions.Any())
            steps.Add($"Consider {challenge.Suggestions.Count} improvement suggestions");

        if (plan.CoverageGaps.Any())
            steps.Add($"Add tests for {plan.CoverageGaps.Count} coverage gaps");

        if (!challenge.PassesBasicValidation)
            steps.Add("Review and refactor test structure");

        if (steps.Count == 0)
            steps.Add("Tests are ready for production - add to test suite!");

        return steps;
    }
}
