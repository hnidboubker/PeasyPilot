# Guide d'Intégration MCP

## Étapes d'Installation

### Étape 1: Installer le Package
```bash
dotnet add package PeasyPilot.Mcp
```

### Étape 2: Définir les Outils MCP
```csharp
using PeasyPilot.Mcp;

public class RunTestsTool : IMcpTool
{
    public string Name => "RunTests";
    public string Description => "Exécuter la suite de tests";
    
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var proc = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "test --no-build",
            RedirectStandardOutput = true
        };
        
        using var process = Process.Start(proc);
        await process.WaitForExitAsync();
        
        return new { exitCode = process.ExitCode, success = process.ExitCode == 0 };
    }
}

public class AnalyzeCodeTool : IMcpTool
{
    public string Name => "AnalyzeCode";
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var filePath = args[0].ToString();
        var analyzer = new ReflectionTestScenarioAnalyzer();
        var result = analyzer.Analyze(Type.GetType(filePath));
        
        return new { methods = result.Methods.Count, dependencies = result.Dependencies.Count };
    }
}
```

### Étape 3: Exposer les Ressources
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
```

### Étape 4: Enregistrer et Démarrer le Serveur
```csharp
using PeasyPilot.Mcp.Transport;

var server = new McpServer();
server.RegisterTool(new RunTestsTool());
server.RegisterTool(new AnalyzeCodeTool());
server.RegisterResource(new TestResultsResource());

var transport = new StdioTransport(server);
await transport.StartAsync();
```

### Étape 5: Connecter depuis Claude
Dans Claude Code:
```json
{
  "tools": {
    "mcp": {
      "command": "dotnet run --project MyApp.Mcp"
    }
  }
}
```

Claude peut maintenant appeler vos outils! 🎯

---

## Bonnes Pratiques

✅ **FAIRE**
- Outils focalisés (une action = un outil)
- Retourner des objets sérialisables JSON
- Gérer les erreurs gracieusement
- Documenter les paramètres d'outil
- Journaliser l'exécution

❌ **NE PAS FAIRE**
- Outils qui font trop de choses
- Retourner du texte non-structuré
- Ignorer les timeouts
- Bloquer longtemps
- Exposer des données sensibles

---

## Dépannage

**"Claude ne trouve pas mes outils"**
→ Vérifiez: `server.RegisterTool(myTool);`

**"L'exécution de l'outil expire"**
→ Ajoutez un paramètre de timeout, faites les opérations longues vraiment asynchrones

**"Claude reçoit des données mal formées"**
→ Assurez-vous que les outils retournent des objets sérialisables JSON

---

👉 [Exemples](./mcp-examples-FR.md)
