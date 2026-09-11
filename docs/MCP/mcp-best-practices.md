# MCP Best Practices

## Tool Design

**Keep tools focused**
```csharp
// ❌ BAD: Does too much
public class DoEverythingTool : IMcpTool
{
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        // Run tests, analyze, generate, report...
    }
}

// ✅ GOOD: Single responsibility
public class RunTestsTool : IMcpTool { }
public class AnalyzeCoverageTool : IMcpTool { }
public class GenerateTestsTool : IMcpTool { }
```

**Return structured data**
```csharp
// ❌ BAD: Unstructured response
return "Tests passed: 42 failed: 0";

// ✅ GOOD: Structured response
return new
{
    passed = 42,
    failed = 0,
    duration = "1.5s"
};
```

**Handle errors gracefully**
```csharp
try
{
    var result = await RunTests();
    return new { success = true, result };
}
catch (Exception ex)
{
    return new
    {
        success = false,
        error = ex.Message,
        type = ex.GetType().Name
    };
}
```

---

## Resource Management

**Use consistent URIs**
```csharp
// All resources under file:///
public string Uri => "file:///tests/results.json";
public string Uri => "file:///metrics/coverage.json";
public string Uri => "file:///analysis/complexity.json";
```

**Cache when appropriate**
```csharp
private DateTime _lastComputed = DateTime.MinValue;
private string _cachedResult = null;

public async Task<string> ReadAsync()
{
    if (DateTime.UtcNow.Subtract(_lastComputed).TotalSeconds < 5)
    {
        return _cachedResult;
    }
    
    _cachedResult = await ComputeAsync();
    _lastComputed = DateTime.UtcNow;
    return _cachedResult;
}
```

---

## Performance

**Timeout considerations**
- Tools should complete in <5 seconds
- Long operations: return status + async ID

**Async/await properly**
```csharp
// ❌ WRONG: Blocks main thread
public async Task<dynamic> ExecuteAsync(params object[] args)
{
    Thread.Sleep(5000); // Blocks!
    return "done";
}

// ✅ RIGHT: Truly async
public async Task<dynamic> ExecuteAsync(params object[] args)
{
    await Task.Delay(5000); // Non-blocking
    return "done";
}
```

---

## Security

**Validate inputs**
```csharp
public async Task<dynamic> ExecuteAsync(params object[] args)
{
    if (args == null || args.Length == 0)
        return new { error = "No arguments provided" };
    
    var filter = args[0].ToString();
    if (!IsValidFilter(filter))
        return new { error = "Invalid filter pattern" };
    
    // Safe to use filter
}
```

**Don't expose secrets**
```csharp
// ❌ BAD: Exposes connection string
return new { connectionString = config["DbConnection"] };

// ✅ GOOD: Safe response
return new { status = "connected", host = "localhost" };
```

---

## Logging

**Log tool execution**
```csharp
public async Task<dynamic> ExecuteAsync(params object[] args)
{
    var toolName = this.Name;
    var startTime = DateTime.UtcNow;
    
    try
    {
        Logger.Info($"[{toolName}] Starting with args: {JsonConvert.SerializeObject(args)}");
        var result = await DoWork();
        
        var duration = DateTime.UtcNow.Subtract(startTime);
        Logger.Info($"[{toolName}] Completed in {duration.TotalMilliseconds}ms");
        
        return result;
    }
    catch (Exception ex)
    {
        Logger.Error($"[{toolName}] Failed: {ex.Message}");
        throw;
    }
}
```

---

## Testing MCP Integration

**Unit test tools**
```csharp
[Fact]
public async Task RunTestsTool_WithValidFilter_ReturnsResults()
{
    var tool = new RunTestsTool();
    var result = await tool.ExecuteAsync("*UserService*");
    
    Assert.NotNull(result);
    Assert.True(result.success);
}
```

---

## Common Pitfalls

❌ **Making tools that take >10 seconds**
❌ **Returning unstructured text instead of JSON**
❌ **Not handling missing parameters**
❌ **Exposing sensitive configuration**
❌ **Blocking on I/O (not using async)**

✅ **Keep tools fast**
✅ **Return structured objects**
✅ **Validate all inputs**
✅ **Filter sensitive data**
✅ **Use async/await throughout**

---

## Summary

Good MCP tools are:
- **Fast** – Complete in <5 seconds
- **Focused** – One responsibility each
- **Structured** – Return JSON-serializable objects
- **Reliable** – Handle errors gracefully
- **Secure** – Validate inputs, don't expose secrets

Build tools that Claude can trust! 🤖

---

**[← Back to MCP Documentation](./README.md)** | **[← Back to Documentation Hub](../README.md)**

**Version:** English | **[Français](./mcp-best-practices-FR.md)**
