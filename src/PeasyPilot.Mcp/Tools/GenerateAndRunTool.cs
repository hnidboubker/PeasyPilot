using PeasyPilot.Mcp.Services;

namespace PeasyPilot.Mcp.Tools;

/// <summary>
/// MCP Tool: Full end-to-end workflow (Analyze → Generate → Challenge)
/// Input: typeName, methodName, namespace, framework (xunit|nunit|tunit)
/// Output: success, workflow steps, final quality assessment
/// </summary>
public class GenerateAndRunTool : IMcpTool
{
    public string Name => "generate_and_run_tests";
    public string Description => "Full workflow: Analyze → Generate → Challenge (magic tool)";

    public async Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        try
        {
            var typeName = parameters.TryGetValue("typeName", out var typeNameObj)
                ? typeNameObj?.ToString()
                : throw new ArgumentException("typeName is required");

            var methodName = parameters.TryGetValue("methodName", out var methodNameObj)
                ? methodNameObj?.ToString()
                : throw new ArgumentException("methodName is required");

            var @namespace = parameters.TryGetValue("namespace", out var nsObj)
                ? nsObj?.ToString() ?? "GeneratedTests"
                : "GeneratedTests";

            var framework = parameters.TryGetValue("framework", out var fwObj)
                ? fwObj?.ToString() ?? "xunit"
                : "xunit";

            var engine = TestAssistantFactory.GetTestEngineer();

            var iterations = new List<dynamic>();
            var startTime = DateTime.UtcNow;

            // Step 1: Analyze
            iterations.Add(new { step = "analyze", status = "in_progress" });
            var analysis = engine.AnalyzeMethod(typeName ?? throw new ArgumentException("typeName is required"),
                                                methodName ?? throw new ArgumentException("methodName is required"));
            iterations[0] = new
            {
                step = "analyze",
                status = "completed",
                methodName = analysis.MethodName,
                parameterCount = analysis.Parameters.Count,
                dependencyCount = analysis.Dependencies.Count,
                scenarioCount = analysis.TestableScenarios.Count
            };

            // Step 2: Plan
            iterations.Add(new { step = "plan", status = "in_progress" });
            var plan = engine.PlanTests(analysis);
            iterations[1] = new
            {
                step = "plan",
                status = "completed",
                estimatedTestCount = plan.TotalEstimatedTests,
                riskScore = plan.RiskScore,
                scenarioCount = plan.Scenarios.Count
            };

            // Step 3: Generate
            iterations.Add(new { step = "generate", status = "in_progress" });
            var generatedCode = engine.GenerateTests(plan, @namespace, framework);
            iterations[2] = new
            {
                step = "generate",
                status = "completed",
                framework = framework,
                codeLength = generatedCode.Length,
                lines = generatedCode.Split('\n').Length
            };

            // Step 4: Challenge
            iterations.Add(new { step = "challenge", status = "in_progress" });
            var challenge = engine.ChallengeTests(generatedCode, analysis);
            iterations[3] = new
            {
                step = "challenge",
                status = "completed",
                qualityScore = challenge.QualityScore,
                issueCount = challenge.Issues.Count,
                suggestionCount = challenge.Suggestions.Count
            };

            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return new
            {
                success = true,
                method = methodName,
                framework = framework,
                iterations = iterations,
                finalQualityScore = challenge.QualityScore,
                isRecommended = challenge.QualityScore >= 70,
                durationMs = durationMs,
                generatedCode = generatedCode,
                summary = new
                {
                    analyzed = true,
                    planned = true,
                    generated = true,
                    challenged = true,
                    quality = $"Score: {challenge.QualityScore}/100"
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
