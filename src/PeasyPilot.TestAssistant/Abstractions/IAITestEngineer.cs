using PeasyPilot.TestAssistant.Models;
using PeasyPilot.TestAssistant.Validation;

namespace PeasyPilot.TestAssistant.Abstractions;

/// <summary>
/// Main API for the AI Test Engineer - orchestrates all components.
/// </summary>
public interface IAITestEngineer
{
    /// <summary>
    /// Analyzes a method and returns complete test insights.
    /// </summary>
    MethodTestModel AnalyzeMethod(string typeName, string methodName);

    /// <summary>
    /// Analyzes a type and returns test insights for all public methods.
    /// </summary>
    List<MethodTestModel> AnalyzeType(Type type);

    /// <summary>
    /// Builds a comprehensive test plan from analyzed method.
    /// </summary>
    TestPlan PlanTests(MethodTestModel analysis);

    /// <summary>
    /// Generates test code for the given plan.
    /// </summary>
    string GenerateTests(TestPlan plan, string @namespace, string framework = "xunit");

    /// <summary>
    /// Validates and challenges the generated test code.
    /// </summary>
    TestChallengeResult ChallengeTests(string testCode, MethodTestModel model);

    /// <summary>
    /// Complete end-to-end workflow: Analyze → Plan → Generate → Challenge.
    /// </summary>
    AITestEngineerResult ExecuteCompleteWorkflow(string typeName, string methodName, string @namespace, string framework = "xunit");
}

/// <summary>
/// Complete result from end-to-end workflow.
/// </summary>
public class AITestEngineerResult
{
    public required string MethodName { get; init; }

    public required MethodTestModel Analysis { get; init; }

    public required TestPlan Plan { get; init; }

    public required string GeneratedCode { get; init; }

    public required TestChallengeResult Challenge { get; init; }

    public int TotalQualityScore { get; init; }

    public bool IsRecommendedForProduction { get; init; }

    public List<string> NextSteps { get; init; } = new();
}
