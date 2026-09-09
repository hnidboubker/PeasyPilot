namespace PeasyPilot.Mcp.Protocol;

/// <summary>
/// JSON-RPC 2.0 message structure for MCP protocol.
/// </summary>
public class JsonRpcMessage
{
    public required string Jsonrpc { get; init; } = "2.0";
    public required string Method { get; init; }
    public object? Params { get; init; }
    public string? Id { get; init; }
}

/// <summary>
/// JSON-RPC response message.
/// </summary>
public class JsonRpcResponse
{
    public string Jsonrpc { get; init; } = "2.0";
    public object? Result { get; init; }
    public JsonRpcError? Error { get; init; }
    public string? Id { get; init; }
}

/// <summary>
/// JSON-RPC error structure.
/// </summary>
public class JsonRpcError
{
    public required int Code { get; init; }
    public required string Message { get; init; }
    public object? Data { get; init; }
}

/// <summary>
/// MCP Initialize request.
/// </summary>
public class InitializeRequest
{
    public required string ProtocolVersion { get; init; }
    public required Dictionary<string, string> Capabilities { get; init; }
    public ClientInfo? ClientInfo { get; init; }
}

/// <summary>
/// Client information.
/// </summary>
public class ClientInfo
{
    public required string Name { get; init; }
    public string? Version { get; init; }
}

/// <summary>
/// MCP Initialize response.
/// </summary>
public class InitializeResponse
{
    public required string ProtocolVersion { get; init; }
    public required Dictionary<string, string> Capabilities { get; init; }
    public ServerInfo? ServerInfo { get; init; }
}

/// <summary>
/// Server information.
/// </summary>
public class ServerInfo
{
    public required string Name { get; init; }
    public string? Version { get; init; }
}

/// <summary>
/// Tool call request.
/// </summary>
public class ToolCallRequest
{
    public required string Tool { get; init; }
    public Dictionary<string, object> Arguments { get; init; } = new();
}

/// <summary>
/// Tool call response.
/// </summary>
public class ToolCallResponse
{
    public required bool Success { get; init; }
    public object? Result { get; init; }
    public string? Error { get; init; }
}
