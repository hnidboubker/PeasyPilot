using System.Dynamic;
using PeasyPilot.Mcp.Services;

namespace PeasyPilot.Mcp.Tools;

/// <summary>
/// MCP Tool: Find gaps in existing tests via analysis
/// Input: testCode, typeName, methodName
/// Output: gaps, qualityScore, issues, suggestions
/// </summary>
public class ChallengeTestsTool : IMcpTool
{
    public string Name => "challenge_tests";
    public string Description => "Find coverage gaps via mutation simulation";

    public async Task<dynamic> ExecuteAsync(Dictionary<string, object> parameters)
    {
        try
        {
            var testCode = (parameters.TryGetValue("testCode", out var codeObj)
                ? codeObj?.ToString()
                : null) ?? throw new ArgumentException("testCode is required");

            var typeName = (parameters.TryGetValue("typeName", out var typeNameObj)
                ? typeNameObj?.ToString()
                : null) ?? throw new ArgumentException("typeName is required");

            var methodName = (parameters.TryGetValue("methodName", out var methodNameObj)
                ? methodNameObj?.ToString()
                : null) ?? throw new ArgumentException("methodName is required");

            var engine = TestAssistantFactory.GetTestEngineer();

            // Analyze the method being tested
            var analysis = engine.AnalyzeMethod(typeName!, methodName!);

            // Challenge the provided test code
            var challengeResult = engine.ChallengeTests(testCode!, analysis);

            dynamic response = new ExpandoObject();
            response.success = true;
            response.method = methodName;
            response.qualityScore = challengeResult.QualityScore;
            response.issues = challengeResult.Issues;
            response.suggestions = challengeResult.Suggestions;
            response.missingScenarios = challengeResult.MissingScenarios;
            response.estimatedCoverage = challengeResult.EstimatedCoverage;
            response.isHealthy = challengeResult.QualityScore >= 70;
            return response;
        }
        catch (Exception ex)
        {
            dynamic response = new ExpandoObject();
            response.success = false;
            response.error = ex.Message;
            return response;
        }
    }
}
