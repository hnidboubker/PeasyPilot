# Exemples MCP

## Outil 1: Test Runner

```csharp
public class RunTestsWithFilterTool : IMcpTool
{
    public string Name => "RunTestsWithFilter";
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var filter = args[0]?.ToString() ?? "";
        var proc = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"test --filter \"{filter}\"",
            RedirectStandardOutput = true
        };
        
        using var process = Process.Start(proc);
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        return new { filter, success = process.ExitCode == 0, output };
    }
}
```

## Outil 2: Analyse de Qualité

```csharp
public class AnalyzeTestCoverageTool : IMcpTool
{
    public string Name => "AnalyzeTestCoverage";
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var analyzer = new CoverageAnalyzer();
        var coverage = await analyzer.AnalyzeAsync();
        
        return new
        {
            overallCoverage = coverage.Percentage,
            uncoveredLines = coverage.UncoveredCount,
            recommendations = coverage.Recommendations
        };
    }
}
```

## Outil 3: Génération de Tests

```csharp
public class GenerateTestsForTypeTool : IMcpTool
{
    public string Name => "GenerateTestsForType";
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var typeName = args[0].ToString();
        var type = Type.GetType(typeName);
        
        var analyzer = new ReflectionTestScenarioAnalyzer();
        var proposal = analyzer.Analyze(type);
        
        var registry = new TestBatteryRendererRegistry();
        var renderer = registry.GetRenderer("xunit");
        var code = renderer.Render(proposal, new RenderOptions { OutputNamespace = "GeneratedTests" });
        
        return new { typeName, testCount = proposal.Methods.Count * 3, generatedCode = code };
    }
}
```

## Ressource: Résultats de Tests

```csharp
public class TestResultsResource
{
    public string Uri => "file:///tests/results.json";
    public string MimeType => "application/json";
    
    public async Task<string> ReadAsync()
    {
        return JsonSerializer.Serialize(new
        {
            timestamp = DateTime.UtcNow,
            totalTests = 42,
            passed = 42,
            failed = 0,
            coverage = 0.92
        });
    }
}
```

## Workflow Exemple

```
Développeur: "Écris les tests pour OrderService"

Claude:
  1. Appelle GenerateTestsForType("OrderService")
  2. Lit les métriques de code
  3. Génère 12 cas de test
  4. Appelle RunTestsWithFilter("Order*")
  5. Analyse les lacunes de couverture
  
Claude: "Généré 12 tests. Couverture: 92%. 
Tests supplémentaires recommandés pour les cas limites."
```

---

Une fois les outils définis, Claude a accès à toute votre infrastructure de test! 🚀

---

**[← Retour à la Documentation MCP](./README.md)** | **[← Retour au Hub Documentation](../README.md)**

**Version:** Français | **[English](./mcp-examples.md)**
