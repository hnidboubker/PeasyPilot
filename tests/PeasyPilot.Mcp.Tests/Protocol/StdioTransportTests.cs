using PeasyPilot.Mcp.Protocol;
using PeasyPilot.Mcp.Transport;
using Xunit;

namespace PeasyPilot.Mcp.Tests.Protocol;

public class StdioTransportTests
{
    [Fact]
    public async Task StdioTransport_ReadsMessage()
    {
        var input = new StringReader("{\"jsonrpc\":\"2.0\",\"method\":\"initialize\",\"id\":\"1\"}");
        var output = new StringWriter();
        var transport = new StdioTransport(input, output);

        var message = await transport.ReadMessageAsync();

        Assert.NotNull(message);
        Assert.Equal("initialize", message.Method);
        Assert.Equal("1", message.Id);
    }

    [Fact]
    public async Task StdioTransport_WritesResponse()
    {
        var input = new StringReader("");
        var output = new StringWriter();
        var transport = new StdioTransport(input, output);

        var response = new JsonRpcResponse
        {
            Id = "1",
            Result = new { success = true }
        };

        await transport.WriteResponseAsync(response);

        var written = output.ToString();
        Assert.Contains("\"id\":\"1\"", written);
        Assert.Contains("\"success\":true", written);
    }

    [Fact]
    public async Task StdioTransport_WritesError()
    {
        var input = new StringReader("");
        var output = new StringWriter();
        var transport = new StdioTransport(input, output);

        await transport.WriteErrorAsync("1", "Test error", -32603);

        var written = output.ToString();
        Assert.Contains("\"id\":\"1\"", written);
        Assert.Contains("\"code\":-32603", written);
        Assert.Contains("\"message\":\"Test error\"", written);
    }

    [Fact]
    public void StdioTransport_ExtractsToolCall()
    {
        var message = new JsonRpcMessage
        {
            Method = "tools/call",
            Params = new ToolCallRequest
            {
                Tool = "analyze_code",
                Arguments = new Dictionary<string, object>
                {
                    { "typeName", "MyClass" }
                }
            },
            Id = "1"
        };

        var toolCall = StdioTransport.ExtractToolCall(message);

        Assert.Equal("analyze_code", toolCall.Tool);
        Assert.Contains("typeName", toolCall.Arguments.Keys);
    }

    [Fact]
    public async Task StdioTransport_HandlesInvalidJson()
    {
        var input = new StringReader("invalid json");
        var output = new StringWriter();
        var transport = new StdioTransport(input, output);

        var message = await transport.ReadMessageAsync();

        Assert.Null(message);
    }

    [Fact]
    public async Task StdioTransport_HandlesEmptyInput()
    {
        var input = new StringReader("");
        var output = new StringWriter();
        var transport = new StdioTransport(input, output);

        var message = await transport.ReadMessageAsync();

        Assert.Null(message);
    }
}
