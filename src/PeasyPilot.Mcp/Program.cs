using PeasyPilot.Mcp.Protocol;

// Create and run the MCP server
var server = new McpServer();

try
{
    // Write startup info to stderr (not interfering with protocol)
    Console.Error.WriteLine("🚀 PeasyPilot MCP Server v1.0.0");
    Console.Error.WriteLine("Starting MCP protocol on stdio...");
    Console.Error.WriteLine();

    // Run the server (blocks until shutdown)
    await server.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error: {ex.Message}");
    Environment.Exit(1);
}
