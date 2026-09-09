namespace PeasyPilot.Mcp.Models;

public class ToolResponse
{
    public bool Success { get; init; }
    public required object Result { get; init; }
    public string? ErrorMessage { get; init; }
    public double ExecutionTimeMs { get; init; }
}
