# MCP Integration Guide

## Setup Steps

### Step 1: Install Package

```bash
dotnet add package PeasyPilot.Mcp
```

### Step 2: Define MCP Tools

```csharp
using PeasyPilot.Mcp;

public class RunTestsTool : IMcpTool
{
    public string Name => "RunTests";
    public string Description => "Execute test suite and return results";
    
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var process = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "test --no-build",
            RedirectStandardOutput = true
        };
        
        using var proc = Process.Start(process);
        await proc.WaitForExitAsync();
        
        return new
        {
            exitCode = proc.ExitCode,
            success = proc.ExitCode == 0,
            timestamp = DateTime.UtcNow
        };
    }
}

public class AnalyzeCodeTool : IMcpTool
{
    public string Name => "AnalyzeCode";
    public string Description => "Analyze code structure and dependencies";
    
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var filePath = args[0].ToString();
        var analyzer = new ReflectionTestScenarioAnalyzer();
        var result = analyzer.Analyze(Type.GetType(filePath));
        
        return new
        {
            methods = result.Methods.Count,
            dependencies = result.Dependencies.Count,
            complexity = "medium"
        };
    }
}
```

### Step 3: Expose Resources

```csharp
public class TestResultsResource
{
    public string Uri => "file:///tests/results.json";
    public string MimeType => "application/json";
    
    public async Task<string> ReadAsync()
    {
        var results = await new TestRunner().RunAsync();
        return JsonSerializer.Serialize(results);
    }
}

public class CoverageMetricsResource
{
    public string Uri => "file:///metrics/coverage.json";
    public string MimeType => "application/json";
    
    public async Task<string> ReadAsync()
    {
        var coverage = new CoverageAnalyzer().AnalyzeCoverage();
        return JsonSerializer.Serialize(coverage);
    }
}
```

### Step 4: Register and Start Server

```csharp
using PeasyPilot.Mcp.Transport;
using PeasyPilot.Mcp.Server;

var server = new McpServer();

// Register tools
server.RegisterTool(new RunTestsTool());
server.RegisterTool(new AnalyzeCodeTool());

// Register resources
server.RegisterResource(new TestResultsResource());
server.RegisterResource(new CoverageMetricsResource());

// Start stdio transport (default)
var transport = new StdioTransport(server);
await transport.StartAsync();
```

### Step 5: Connect from Claude

In Claude Code settings:

```json
{
  "tools": {
    "mcp": {
      "command": "dotnet run --project MyApp.Mcp",
      "args": [],
      "env": {
        "ASPNETCORE_ENVIRONMENT": "MCP"
      }
    }
  }
}
```

Claude can now call your tools:

```
Claude: "Run the tests and analyze coverage"
[Calls RunTests via MCP]
[Reads CoverageMetrics resource]
Claude: "42 tests passing. Coverage is 92%."
```

---

## Tool Design Best Practices

✅ **DO**
- Keep tools focused (one action per tool)
- Return structured data (JSON-serializable)
- Handle errors gracefully
- Document tool parameters
- Log tool execution for debugging

❌ **DON'T**
- Make tools that do too much
- Return unstructured text
- Ignore timeouts
- Make tools that block for long
- Expose sensitive data

---

## Resource Best Practices

✅ **DO**
- Use `file:///` URIs for consistency
- Return valid JSON/XML
- Cache results when appropriate
- Keep resources under 1MB

❌ **DON'T**
- Return binary data
- Make resources that take >5s to compute
- Change resource URIs between versions

---

## Troubleshooting

**"Claude can't find my tools"**
→ Verify tools are registered: `server.RegisterTool(myTool);`

**"Tool execution times out"**
→ Add timeout parameter, make long operations async

**"Claude gets malformed data"**
→ Ensure tools return JSON-serializable objects

---

## Next Steps

📖 [MCP Examples](./mcp-examples.md) – Real working examples  
📖 [MCP Best Practices](./mcp-best-practices.md) – Patterns and optimization  
📖 [MCP API Reference](./mcp-api-reference.md) – Complete API

---

**[← Back to MCP Documentation](./README.md)** | **[← Back to Documentation Hub](../README.md)**

**Version:** English | **[Français](./mcp-integration-guide-FR.md)**  
