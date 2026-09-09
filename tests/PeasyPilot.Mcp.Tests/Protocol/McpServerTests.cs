using PeasyPilot.Mcp.Protocol;
using PeasyPilot.Mcp.Services;
using Xunit;

namespace PeasyPilot.Mcp.Tests.Protocol;

public class McpServerTests
{
    [Fact]
    public void McpServer_CreatesInstance()
    {
        var server = new McpServer();
        Assert.NotNull(server);
    }

    [Fact]
    public async Task McpServer_HandlesInitializeRequest()
    {
        var server = new McpServer();
        var message = new JsonRpcMessage
        {
            Jsonrpc = "2.0",
            Method = "initialize",
            Params = new { },
            Id = "1"
        };

        // Use reflection to call HandleMessageAsync (it's private)
        var method = typeof(McpServer).GetMethod("HandleMessageAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = await (Task<JsonRpcResponse?>)method!.Invoke(server, new object[] { message })!;

        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task McpServer_HandlesListToolsRequest()
    {
        var server = new McpServer();

        // First initialize
        var initMsg = new JsonRpcMessage
        {
            Jsonrpc = "2.0",
            Method = "initialize",
            Params = new { },
            Id = "1"
        };

        var method = typeof(McpServer).GetMethod("HandleMessageAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        await (Task<JsonRpcResponse?>)method!.Invoke(server, new object[] { initMsg })!;

        // Then list tools
        var listMsg = new JsonRpcMessage
        {
            Jsonrpc = "2.0",
            Method = "tools/list",
            Params = new { },
            Id = "2"
        };

        var result = await (Task<JsonRpcResponse?>)method!.Invoke(server, new object[] { listMsg })!;

        Assert.NotNull(result);
        Assert.Equal("2", result.Id);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task McpServer_RejectsToolCallBeforeInitialization()
    {
        var server = new McpServer();
        var message = new JsonRpcMessage
        {
            Jsonrpc = "2.0",
            Method = "tools/call",
            Params = new { tool = "analyze_code", arguments = new Dictionary<string, object>() },
            Id = "1"
        };

        var method = typeof(McpServer).GetMethod("HandleMessageAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = await (Task<JsonRpcResponse?>)method!.Invoke(server, new object[] { message })!;

        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Contains("not initialized", result.Error.Message);
    }

    [Fact]
    public async Task McpServer_ReturnsErrorForUnknownMethod()
    {
        var server = new McpServer();
        var message = new JsonRpcMessage
        {
            Jsonrpc = "2.0",
            Method = "unknown",
            Params = new { },
            Id = "1"
        };

        var method = typeof(McpServer).GetMethod("HandleMessageAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var result = await (Task<JsonRpcResponse?>)method!.Invoke(server, new object[] { message })!;

        Assert.NotNull(result);
        Assert.NotNull(result.Error);
    }
}
