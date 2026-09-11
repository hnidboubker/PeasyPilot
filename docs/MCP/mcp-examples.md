# MCP Examples

## Example 1: Test Runner Tool

```csharp
public class TestRunnerTool : IMcpTool
{
    public string Name => "RunTestsWithFilter";
    public string Description => "Run tests matching a filter pattern";
    
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var filter = args[0]?.ToString() ?? "";
        var cmd = $"dotnet test --filter \"{filter}\"";
        
        var proc = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"test --filter \"{filter}\" --logger json",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        
        using var process = Process.Start(proc);
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        return new
        {
            filter = filter,
            success = process.ExitCode == 0,
            output = output,
            exitCode = process.ExitCode
        };
    }
}

// Usage:
// Claude: "Run all authentication tests"
// → RunTestsWithFilter("*Authentication*")
```

## Example 2: Code Quality Tool

```csharp
public class CodeQualityTool : IMcpTool
{
    public string Name => "AnalyzeTestCoverage";
    public string Description => "Analyze test coverage and identify gaps";
    
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var analyzer = new CoverageAnalyzer();
        var coverage = await analyzer.AnalyzeAsync();
        
        return new
        {
            overallCoverage = coverage.Percentage,
            uncoveredLines = coverage.UncoveredCount,
            recommendations = coverage.Recommendations,
            files = coverage.FileDetails.Select(f => new
            {
                path = f.Path,
                coverage = f.CoveragePercentage,
                gaps = f.UncoveredMethods
            }).ToList()
        };
    }
}

// Usage:
// Claude: "What's our test coverage?"
// → AnalyzeTestCoverage()
// → Returns coverage metrics and recommendations
```

## Example 3: Test Generation Tool

```csharp
public class GenerateTestsTool : IMcpTool
{
    public string Name => "GenerateTestsForType";
    public string Description => "Generate test cases for a given type";
    
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var typeName = args[0].ToString();
        var type = Type.GetType(typeName);
        
        var analyzer = new ReflectionTestScenarioAnalyzer();
        var proposal = analyzer.Analyze(type);
        
        var registry = new TestBatteryRendererRegistry();
        var renderer = registry.GetRenderer("xunit");
        var code = renderer.Render(proposal, new RenderOptions
        {
            OutputNamespace = "GeneratedTests"
        });
        
        return new
        {
            typeName = typeName,
            testCount = proposal.Methods.Count * 3, // ~3 tests per method
            generatedCode = code,
            scenarios = proposal.Scenarios
        };
    }
}

// Usage:
// Claude: "Generate tests for the UserService class"
// → GenerateTestsForType("MyApp.UserService")
// → Returns generated test code
```

---

## Resource Examples

### Example: Test Results Resource

```csharp
public class TestResultsResource
{
    public string Uri => "file:///tests/results.json";
    public string MimeType => "application/json";
    
    public async Task<string> ReadAsync()
    {
        var results = new
        {
            timestamp = DateTime.UtcNow,
            totalTests = 42,
            passed = 42,
            failed = 0,
            skipped = 0,
            duration = "1.5s",
            coverage = 0.92,
            frameworks = new[] { "net8.0", "net9.0", "net10.0" }
        };
        
        return JsonSerializer.Serialize(results, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
```

### Example: Code Metrics Resource

```csharp
public class CodeMetricsResource
{
    public string Uri => "file:///metrics/code.json";
    public string MimeType => "application/json";
    
    public async Task<string> ReadAsync()
    {
        var assembly = Assembly.Load("MyApp");
        var types = assembly.GetTypes();
        
        return JsonSerializer.Serialize(new
        {
            typeCount = types.Length,
            methodCount = types.Sum(t => t.GetMethods().Length),
            averageMethodsPerType = types.Average(t => t.GetMethods().Length),
            publicTypes = types.Count(t => t.IsPublic)
        });
    }
}
```

---

## Workflow Example

```
Developer: "Write tests for the OrderService"

Claude:
  1. Calls GenerateTestsForType("OrderService")
  2. Reads Code Metrics from file:///metrics/code.json
  3. Analyzes dependencies
  4. Generates 12 test cases
  5. Calls RunTestsWithFilter("Order*")
  6. Analyzes coverage gap
  
Claude: "Generated 12 tests. Current coverage: 92%. 
Recommended additional tests for edge cases: 
- Empty order list, Concurrent orders, Payment timeout"
```

---

## Integration Points

- **CI/CD:** Trigger MCP tools in GitHub Actions
- **IDE:** Call MCP tools from editor extensions
- **Chat:** Ask Claude to run tests and analyze results
- **Dashboard:** Display MCP resources as real-time metrics

The key: Once you define tools and resources, Claude can access your entire test infrastructure! 🚀

---

**[← Back to MCP Documentation](./README.md)** | **[← Back to Documentation Hub](../README.md)**

**Version:** English | **[Français](./mcp-examples-FR.md)**
