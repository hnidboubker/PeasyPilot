using PeasyPilot.Mcp.Tools;

namespace PeasyPilot.Mcp.Services;

public class McpToolRegistry
{
    private readonly Dictionary<string, IMcpTool> _tools = new();

    public void Register(IMcpTool tool) => _tools[tool.Name] = tool;

    public IMcpTool? GetTool(string name) => _tools.TryGetValue(name, out var tool) ? tool : null;

    public IEnumerable<IMcpTool> GetAllTools() => _tools.Values;

    public void RegisterDefaults()
    {
        Register(new GenerateTestsTool());
        Register(new RunTestsTool());
        Register(new AnalyzeCodeTool());
        Register(new AnalyzeFailureTool());
        Register(new ChallengeTestsTool());
        Register(new GenerateAndRunTool());
    }
}
