namespace PeasyPilot.Mcp.Tools;

public interface IMcpTool
{
    string Name { get; }
    string Description { get; }
    Task<object> ExecuteAsync(Dictionary<string, object> parameters);
}
