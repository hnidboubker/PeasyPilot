using PeasyPilot.Mcp.Services;

namespace PeasyPilot.Mcp.Tools;

/// <summary>
/// MCP Tool: Analyze code and generate tests
/// Input: typeName, methodName, namespace, framework (xunit|nunit|tunit, default: xunit)
/// Output: generatedCode, testCount, scenarios, quality assessment
/// </summary>
public class GenerateTestsTool : IMcpTool
{
    public string Name => "generate_tests";
    public string Description => "Analyze code and generate comprehensive test suite";

    public async Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        try
        {
            var typeName = (parameters.TryGetValue("typeName", out var typeNameObj)
                ? typeNameObj?.ToString()
                : null) ?? throw new ArgumentException("typeName is required");

            var methodName = (parameters.TryGetValue("methodName", out var methodNameObj)
                ? methodNameObj?.ToString()
                : null) ?? throw new ArgumentException("methodName is required");

            var @namespace = parameters.TryGetValue("namespace", out var nsObj)
                ? nsObj?.ToString() ?? "GeneratedTests"
                : "GeneratedTests";

            var framework = parameters.TryGetValue("framework", out var fwObj)
                ? fwObj?.ToString() ?? "xunit"
                : "xunit";

            var engine = TestAssistantFactory.GetTestEngineer();

            // Analyze the method
            var analysis = engine.AnalyzeMethod(typeName!, methodName!);

            // Plan the tests
            var plan = engine.PlanTests(analysis);

            // Generate the test code
            var generatedCode = engine.GenerateTests(plan, @namespace, framework);

            // Challenge/validate the tests
            var challenge = engine.ChallengeTests(generatedCode, analysis);

            return new
            {
                success = true,
                framework = framework,
                method = methodName,
                generatedCode,
                scenarios = plan.Scenarios.Select(s => new
                {
                    description = s.Description,
                    s.Type,
                    s.RiskLevel
                }).ToList(),
                estimatedTestCount = plan.TotalEstimatedTests,
                riskScore = plan.RiskScore,
                quality = new
                {
                    qualityScore = challenge.QualityScore,
                    issues = challenge.Issues,
                    suggestions = challenge.Suggestions.Take(3).ToList()
                }
            };
        }
        catch (Exception ex)
        {
            return new
            {
                success = false,
                error = ex.Message
            };
        }
    }
}
