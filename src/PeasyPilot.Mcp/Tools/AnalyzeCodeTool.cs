using PeasyPilot.Mcp.Services;

namespace PeasyPilot.Mcp.Tools;

/// <summary>
/// MCP Tool: Extract testable patterns from code
/// Input: typeName (required), methodName (optional)
/// Output: methods[], scenarios[], dependencies[], testableElements
/// </summary>
public class AnalyzeCodeTool : IMcpTool
{
    public string Name => "analyze_code";
    public string Description => "Extract testable patterns and method signatures";

    public async Task<object> ExecuteAsync(Dictionary<string, object> parameters)
    {
        try
        {
            var typeName = (parameters.TryGetValue("typeName", out var typeNameObj)
                ? typeNameObj?.ToString()
                : null) ?? throw new ArgumentException("typeName is required");

            var methodName = parameters.TryGetValue("methodName", out var methodNameObj)
                ? methodNameObj?.ToString()
                : null;

            var engine = TestAssistantFactory.GetTestEngineer();

            if (!string.IsNullOrEmpty(methodName))
            {
                var analysis = engine.AnalyzeMethod(typeName!, methodName!);
                return new
                {
                    success = true,
                    method = new
                    {
                        name = analysis.MethodName,
                        returnType = analysis.ReturnTypeName,
                        parameters = analysis.Parameters.Select(p => new
                        {
                            p.Name,
                            type = p.TypeName,
                            p.IsNullable
                        }).ToList(),
                        dependencies = analysis.Dependencies.Select(d => new
                        {
                            dependencyType = d.InterfaceName,
                            d.SuggestedStrategy
                        }).ToList(),
                        scenarios = analysis.TestableScenarios.Select(s => new
                        {
                            description = s.Description,
                            s.Type,
                            s.RiskLevel
                        }).ToList()
                    },
                    totalScenarios = analysis.TestableScenarios.Count,
                    totalDependencies = analysis.Dependencies.Count
                };
            }
            else
            {
                var type = Type.GetType(typeName)
                    ?? throw new InvalidOperationException($"Type '{typeName}' not found");

                var analyses = engine.AnalyzeType(type);
                return new
                {
                    success = true,
                    type = typeName,
                    methodCount = analyses.Count,
                    methods = analyses.Select(m => new
                    {
                        name = m.MethodName,
                        returnType = m.ReturnTypeName,
                        parameterCount = m.Parameters.Count,
                        dependencyCount = m.Dependencies.Count,
                        scenarioCount = m.TestableScenarios.Count
                    }).ToList()
                };
            }
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
