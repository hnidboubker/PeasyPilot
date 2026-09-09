using PeasyPilot.Mcp.Models;
using PeasyPilot.Mcp.Tools;

namespace PeasyPilot.Mcp.Services;

public class TestAssistantService
{
    private readonly McpToolRegistry _toolRegistry;

    public TestAssistantService(McpToolRegistry toolRegistry)
    {
        _toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
    }

    public async Task<ToolResponse> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var tool = _toolRegistry.GetTool(toolName);
            if (tool == null)
                return new ToolResponse
                {
                    Success = false,
                    Result = null!,
                    ErrorMessage = $"Tool '{toolName}' not found",
                    ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
                };

            var result = await tool.ExecuteAsync(parameters);
            return new ToolResponse
            {
                Success = true,
                Result = result,
                ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
        catch (Exception ex)
        {
            return new ToolResponse
            {
                Success = false,
                Result = null!,
                ErrorMessage = ex.Message,
                ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
    }
}
