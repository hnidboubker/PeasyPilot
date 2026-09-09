using PeasyPilot.Mcp.Services;
using PeasyPilot.Mcp.Transport;

namespace PeasyPilot.Mcp.Protocol;

/// <summary>
/// MCP Server that handles protocol initialization and tool execution.
/// </summary>
public class McpServer
{
    private readonly StdioTransport _transport;
    private readonly TestAssistantService _testAssistant;
    private readonly McpToolRegistry _toolRegistry;
    private bool _initialized;

    public McpServer(StdioTransport? transport = null, TestAssistantService? testAssistant = null)
    {
        _transport = transport ?? new StdioTransport();
        _toolRegistry = new McpToolRegistry();
        _toolRegistry.RegisterDefaults();
        _testAssistant = testAssistant ?? new TestAssistantService(_toolRegistry);
        _initialized = false;
    }

    /// <summary>
    /// Start the MCP server and begin processing messages.
    /// </summary>
    public async Task RunAsync()
    {
        while (true)
        {
            try
            {
                var message = await _transport.ReadMessageAsync();
                if (message == null)
                    continue;

                var response = await HandleMessageAsync(message);
                if (response != null)
                {
                    await _transport.WriteResponseAsync(response);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Server error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handle an incoming JSON-RPC message.
    /// </summary>
    private async Task<JsonRpcResponse?> HandleMessageAsync(JsonRpcMessage message)
    {
        if (string.IsNullOrEmpty(message.Id))
            return null; // Notifications don't require responses

        try
        {
            return message.Method switch
            {
                "initialize" => HandleInitialize(message),
                "tools/list" => HandleListTools(message),
                "tools/call" => await HandleToolCall(message),
                _ => CreateErrorResponse(message.Id, $"Unknown method: {message.Method}")
            };
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(message.Id, ex.Message);
        }
    }

    /// <summary>
    /// Handle initialize request.
    /// </summary>
    private JsonRpcResponse HandleInitialize(JsonRpcMessage message)
    {
        _initialized = true;

        var response = new InitializeResponse
        {
            ProtocolVersion = "2024-11-05",
            Capabilities = new Dictionary<string, string>
            {
                { "tools", "1.0" }
            },
            ServerInfo = new ServerInfo
            {
                Name = "PeasyPilot MCP Server",
                Version = "1.0.0"
            }
        };

        return new JsonRpcResponse
        {
            Id = message.Id,
            Result = response
        };
    }

    /// <summary>
    /// Handle tools/list request.
    /// </summary>
    private JsonRpcResponse HandleListTools(JsonRpcMessage message)
    {
        if (!_initialized)
            return CreateErrorResponse(message.Id, "Server not initialized");

        var tools = _toolRegistry.GetAllTools()
            .Select(t => new
            {
                name = t.Name,
                description = t.Description
            })
            .ToList();

        return new JsonRpcResponse
        {
            Id = message.Id,
            Result = new { tools }
        };
    }

    /// <summary>
    /// Handle tool call request.
    /// </summary>
    private async Task<JsonRpcResponse> HandleToolCall(JsonRpcMessage message)
    {
        if (!_initialized)
            return CreateErrorResponse(message.Id, "Server not initialized");

        try
        {
            var toolCall = StdioTransport.ExtractToolCall(message);
            var result = await _testAssistant.ExecuteToolAsync(toolCall.Tool, toolCall.Arguments);

            return new JsonRpcResponse
            {
                Id = message.Id,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(message.Id, ex.Message);
        }
    }

    /// <summary>
    /// Create an error response.
    /// </summary>
    private static JsonRpcResponse CreateErrorResponse(string? id, string message)
    {
        return new JsonRpcResponse
        {
            Id = id,
            Error = new JsonRpcError
            {
                Code = -32603,
                Message = message
            }
        };
    }
}
