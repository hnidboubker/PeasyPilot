using PeasyPilot.Mcp.Services;

var registry = new McpToolRegistry();
registry.RegisterDefaults();

var service = new TestAssistantService(registry);

Console.WriteLine("🚀 PeasyPilot MCP Server");
Console.WriteLine("=======================");
Console.WriteLine();
Console.WriteLine("Available Tools:");
foreach (var tool in registry.GetAllTools())
{
    Console.WriteLine($"  • {tool.Name} - {tool.Description}");
}
Console.WriteLine();
Console.WriteLine("Server ready to accept MCP requests...");

// Placeholder for MCP server loop
while (true)
{
    await Task.Delay(1000);
}
