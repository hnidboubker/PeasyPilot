namespace PeasyPilot.Mcp.Models;

public class ToolRequest
{
    public required string ToolName { get; init; }
    public required string ProjectPath { get; init; }
    public Dictionary<string, object> Parameters { get; init; } = new();
}
