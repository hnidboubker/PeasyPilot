# Référence API MCP

## Interface IMcpTool

```csharp
public interface IMcpTool
{
    string Name { get; }                              // e.g., "RunTests"
    string Description { get; }                       // Description pour Claude
    Task<dynamic> ExecuteAsync(params object[] args); // Retourne JSON-sérialisable
}
```

## Interface IMcpResource

```csharp
public interface IMcpResource
{
    string Uri { get; }                  // file:///...
    string MimeType { get; }             // application/json, etc.
    Task<string> ReadAsync();            // Retourne le contenu
}
```

## Classe McpServer

```csharp
public class McpServer
{
    public void RegisterTool(IMcpTool tool);
    public void RegisterResource(IMcpResource resource);
    public IReadOnlyList<IMcpTool> Tools { get; }
    public IReadOnlyList<IMcpResource> Resources { get; }
}
```

## Transports

### StdioTransport (Défaut)
```csharp
var transport = new StdioTransport(server);
await transport.StartAsync();
```

### HttpTransport (Optionnel)
```csharp
var transport = new HttpTransport(server, "http://localhost:5000");
await transport.StartAsync();
```

---

## Format de Réponse des Outils

```csharp
// Succès
{ "success": true, "data": { /* spécifique à l'outil */ } }

// Erreur
{ "success": false, "error": "message", "type": "ExceptionType" }
```

## Format de Réponse des Ressources

```json
{
    "timestamp": "2026-09-11T12:00:00Z",
    "data": { /* spécifique à la ressource */ }
}
```

---

## Flux d'Exécution d'un Outil

```
1. Claude appelle outil via MCP
2. Serveur MCP cherche IMcpTool par nom
3. Appelle tool.ExecuteAsync(args)
4. Outil retourne objet dynamic
5. MCP sérialise en JSON
6. Retourne à Claude
7. Claude utilise le résultat
```

## Flux de Lecture d'une Ressource

```
1. Claude demande ressource via URI
2. Serveur MCP cherche IMcpResource par Uri
3. Appelle resource.ReadAsync()
4. Ressource retourne string
5. MCP retourne le contenu
6. Claude l'analyse
```

---

## Patterns Courants

### Outil de Requête (Retourne des Données)
```csharp
public class GetTestResultsTool : IMcpTool
{
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var results = await _testRunner.GetLastResults();
        return new { results };
    }
}
```

### Outil d'Action (Modifie l'État)
```csharp
public class RunTestsTool : IMcpTool
{
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var results = await _testRunner.RunAsync();
        return new { passed = results.Passed, failed = results.Failed };
    }
}
```

### Outil d'Analyse (Calcule)
```csharp
public class AnalyzeCoverageTool : IMcpTool
{
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        var coverage = await _analyzer.ComputeCoverageAsync();
        return new { percentage = coverage.Percentage };
    }
}
```

---

## Codes d'Erreur

| Code | Signification |
|------|---|
| 400 | Arguments invalides |
| 404 | Outil/Ressource non trouvé |
| 500 | Outil échoué |
| 503 | Serveur indisponible |

---

## Checklist d'Intégration

- [ ] Définir les outils
- [ ] Définir les ressources
- [ ] Enregistrer avec McpServer
- [ ] Choisir transport (stdio/HTTP)
- [ ] Démarrer le serveur
- [ ] Configurer la connexion Claude
- [ ] Tester l'exécution
- [ ] Vérifier les logs

---

MCP = L'API publique de votre infra de test! 🚀

---

**[← Retour à la Documentation MCP](./README.md)** | **[← Retour au Hub Documentation](../README.md)**

**Version:** Français | **[English](./mcp-api-reference.md)**
