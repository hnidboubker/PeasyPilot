# Meilleures Pratiques MCP

## Conception des Outils

**Focalisés**
```csharp
// ❌ BAD: Trop de responsabilités
// ✅ GOOD: Une responsabilité chacun
class RunTestsTool { }
class AnalyzeCoverageTool { }
class GenerateTestsTool { }
```

**Retourner des données structurées**
```csharp
// ❌ "Tests passed: 42"
// ✅ new { passed = 42, failed = 0 }
```

**Gérer les erreurs**
```csharp
try
{
    return new { success = true, result };
}
catch (Exception ex)
{
    return new { success = false, error = ex.Message };
}
```

---

## Gestion des Ressources

**URIs cohérents**
```csharp
public string Uri => "file:///tests/results.json";
public string Uri => "file:///metrics/coverage.json";
```

**Mettre en cache quand c'est approprié**
```csharp
private DateTime _lastComputed;
private string _cached;

public async Task<string> ReadAsync()
{
    if (DateTime.UtcNow.Subtract(_lastComputed).TotalSeconds < 5)
        return _cached;
    
    _cached = await ComputeAsync();
    _lastComputed = DateTime.UtcNow;
    return _cached;
}
```

---

## Performance

**Timeouts**
- Outils: <5 secondes
- Ressources: <2 secondes

**Vraiment asynchrone**
```csharp
// ❌ Thread.Sleep(5000); // Bloque!
// ✅ await Task.Delay(5000); // Non-bloquant
```

---

## Sécurité

**Valider les entrées**
```csharp
if (args == null || args.Length == 0)
    return new { error = "No args" };

var filter = args[0].ToString();
if (!IsValidFilter(filter))
    return new { error = "Invalid filter" };
```

**Ne pas exposer les secrets**
```csharp
// ❌ return new { connectionString = config["DbConnection"] };
// ✅ return new { status = "connected", host = "localhost" };
```

---

## Journalisation

```csharp
Logger.Info($"[{Name}] Starting");
try
{
    var result = await DoWork();
    Logger.Info($"[{Name}] Completed");
    return result;
}
catch (Exception ex)
{
    Logger.Error($"[{Name}] Failed: {ex.Message}");
    throw;
}
```

---

## Pièges Courants

❌ Outils >10 secondes  
❌ Texte non-structuré  
❌ Pas de validation  
❌ Exposer des config sensibles  
❌ Pas vraiment asynchrone  

✅ Outils rapides  
✅ Retourner JSON  
✅ Valider les entrées  
✅ Sécuriser les données  
✅ Async partout  

---

Bons outils = outils qu'on peut faire confiance à Claude! 🤖

---

**[← Retour à la Documentation MCP](./README.md)** | **[← Retour au Hub Documentation](../README.md)**

**Version:** Français | **[English](./mcp-best-practices.md)**
