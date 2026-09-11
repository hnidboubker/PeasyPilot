# MCP API Reference

## IMcpTool Interface

```csharp
public interface IMcpTool
{
    /// Tool name (e.g., "RunTests", "AnalyzeCoverage")
    string Name { get; }
    
    /// Human-readable description
    string Description { get; }
    
    /// Execute the tool with arguments
    /// Returns JSON-serializable object
    Task<dynamic> ExecuteAsync(params object[] args);
}
```

## IMcpResource Interface

```csharp
public interface IMcpResource
{
    /// Uniform resource identifier (file:///...)
    string Uri { get; }
    
    /// MIME type (application/json, text/plain, etc.)
    string MimeType { get; }
    
    /// Read resource content
    Task<string> ReadAsync();
}
```

## McpServer Class

```csharp
public class McpServer
{
    /// Register a tool
    public void RegisterTool(IMcpTool tool);
    
    /// Register a resource
    public void RegisterResource(IMcpResource resource);
    
    /// Get registered tools
    public IReadOnlyList<IMcpTool> Tools { get; }
    
    /// Get registered resources
    public IReadOnlyList<IMcpResource> Resources { get; }
}
```

## Transport Implementations

### StdioTransport (Default)
```csharp
public class StdioTransport
{
    public StdioTransport(McpServer server);
    public Task StartAsync();
    public Task StopAsync();
}
```

### HttpTransport (Optional)
```csharp
public class HttpTransport
{
    public HttpTransport(McpServer server, string baseUrl);
    public Task StartAsync();
}
```

## Tool Response Format

All tools return JSON-serializable objects:

```csharp
// Success
{
    "success": true,
    "data": { /* tool-specific */ }
}

// Error
{
    "success": false,
    "error": "Error message",
    "type": "ExceptionType"
}
```

## Resource Response Format

All resources return JSON strings:

```json
{
    "timestamp": "2026-09-11T12:00:00Z",
    "data": { /* resource-specific */ }
}
```

## Tool Execution Flow

```
1. Claude calls tool via MCP
   → MCP server receives tool name + args
   
2. McpServer looks up IMcpTool by name
   
3. Calls tool.ExecuteAsync(args)
   
4. Tool returns dynamic object
   
5. MCP serializes to JSON
   
6. Returns to Claude
   
7. Claude uses result in reasoning
```

## Resource Reading Flow

```
1. Claude requests resource at URI
   → MCP server receives file:///path
   
2. McpServer looks up IMcpResource by Uri
   
3. Calls resource.ReadAsync()
   
4. Resource returns string (JSON/XML/text)
   
5. MCP returns content to Claude
   
6. Claude parses and uses data
```

## Common Tool Patterns

### Query Tool (Returns Data)
```csharp
public class GetTestResultsTool : IMcpTool
{
    public string Name => "GetTestResults";
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        // Just return data, don't modify state
        var results = await _testRunner.GetLastResults();
        return new { results };
    }
}
```

### Action Tool (Modifies State)
```csharp
public class RunTestsTool : IMcpTool
{
    public string Name => "RunTests";
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        // Execute tests, return status
        var results = await _testRunner.RunAsync();
        return new { passed = results.Passed, failed = results.Failed };
    }
}
```

### Analysis Tool (Computes)
```csharp
public class AnalyzeCoverageTool : IMcpTool
{
    public string Name => "AnalyzeCoverage";
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        // Compute metrics
        var coverage = await _analyzer.ComputeCoverageAsync();
        return new { percentage = coverage.Percentage, gaps = coverage.Gaps };
    }
}
```

## Error Codes

| Code | Meaning |
|------|---------|
| 400 | Invalid arguments |
| 404 | Tool/Resource not found |
| 500 | Tool execution failed |
| 503 | Server unavailable |

## Best Practices (Summary)

- Tool names: PascalCase (GetTests, RunAnalysis)
- Descriptions: Clear, imperative (Run the test suite)
- URIs: file:/// prefix consistently
- Timeouts: 5-30 seconds max
- Errors: Return structured error objects

---

## Integration Checklist

- [ ] Define tools (implement IMcpTool)
- [ ] Define resources (implement IMcpResource)
- [ ] Register with McpServer
- [ ] Choose transport (stdio/HTTP)
- [ ] Start server in long-running process
- [ ] Configure Claude/AI connection
- [ ] Test tool execution manually
- [ ] Monitor logs for errors

---

## Debugging

**Enable verbose logging:**
```csharp
var server = new McpServer();
server.Logger.Level = LogLevel.Debug;
```

**Inspect registered tools:**
```csharp
foreach (var tool in server.Tools)
{
    Console.WriteLine($"{tool.Name}: {tool.Description}");
}
```

**Test tool directly:**
```csharp
var tool = new RunTestsTool();
var result = await tool.ExecuteAsync();
Console.WriteLine(JsonConvert.SerializeObject(result));
```

---

**Full MCP Documentation:** [MCP Official Docs](https://modelcontextprotocol.io)

MCP = Your test infrastructure's public API! 🚀
