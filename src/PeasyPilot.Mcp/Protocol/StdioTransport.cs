using System.Text.Json;
using PeasyPilot.Mcp.Protocol;

namespace PeasyPilot.Mcp.Transport;

/// <summary>
/// Stdio transport for MCP protocol communication.
/// Reads JSON-RPC messages from stdin, writes responses to stdout.
/// </summary>
public class StdioTransport
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly JsonSerializerOptions _jsonOptions;

    public StdioTransport(TextReader? input = null, TextWriter? output = null)
    {
        _input = input ?? Console.In;
        _output = output ?? Console.Out;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Read a JSON-RPC message from stdin.
    /// </summary>
    public async Task<JsonRpcMessage?> ReadMessageAsync()
    {
        try
        {
            var line = await _input.ReadLineAsync();
            if (string.IsNullOrEmpty(line))
                return null;

            return JsonSerializer.Deserialize<JsonRpcMessage>(line, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error reading message: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Write a JSON-RPC response to stdout.
    /// </summary>
    public async Task WriteResponseAsync(JsonRpcResponse response)
    {
        try
        {
            var json = JsonSerializer.Serialize(response, _jsonOptions);
            await _output.WriteLineAsync(json);
            await _output.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error writing response: {ex.Message}");
        }
    }

    /// <summary>
    /// Write an error response.
    /// </summary>
    public async Task WriteErrorAsync(string id, string message, int code = -32603)
    {
        var response = new JsonRpcResponse
        {
            Id = id,
            Error = new JsonRpcError
            {
                Code = code,
                Message = message
            }
        };

        await WriteResponseAsync(response);
    }

    /// <summary>
    /// Convert JSON-RPC params to tool call request.
    /// </summary>
    public static ToolCallRequest ExtractToolCall(JsonRpcMessage message)
    {
        // Handle direct ToolCallRequest (for tests)
        if (message.Params is ToolCallRequest toolCall)
        {
            return toolCall;
        }

        // Handle JsonElement (from JSON deserialization)
        if (message.Params is JsonElement element)
        {
            var json = element.GetRawText();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = JsonSerializer.Deserialize<ToolCallRequest>(json, options)
                ?? throw new InvalidOperationException("Invalid tool call request");
            return request;
        }

        throw new InvalidOperationException("Params must be a JSON object or ToolCallRequest");
    }
}
