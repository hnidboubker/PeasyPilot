using System.Dynamic;

namespace PeasyPilot.Mcp.Tools;

public interface IMcpTool
{
    string Name { get; }
    string Description { get; }
    Task<dynamic> ExecuteAsync(Dictionary<string, object> parameters);
}
