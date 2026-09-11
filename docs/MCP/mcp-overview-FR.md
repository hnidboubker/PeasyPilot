# Aperçu MCP

## Qu'est-ce que MCP?

**Model Context Protocol (MCP)** est un protocole standard qui connecte les modèles IA aux outils, ressources et sources de données. PeasyPilot utilise MCP pour exposer l'infrastructure de test à Claude.

**En langage simple:** MCP laisse Claude exécuter des tests, analyser du code et générer des cas de test comme s'il s'agissait d'une API.

---

## Pourquoi MCP pour les Tests?

**Avant:**
```
Développeur écrit le code
  ↓
Développeur écrit les tests manuellement
  ↓
Développeur exécute les tests
```

**Avec MCP:**
```
Développeur écrit le code
  ↓
Claude écrit les tests via les outils MCP
  ↓
Claude exécute les tests via MCP
  ↓
Claude analyse la couverture via les ressources MCP
```

**Bénéfices:**
- Génération de tests assistée par IA
- Analyse automatisée des tests
- Boucles de rétroaction en temps réel
- Intégration avec le raisonnement de Claude

---

## Architecture

```
┌─────────────────────────────────┐
│      Claude / Modèle IA         │
└──────────────┬──────────────────┘
               │ (Protocole MCP)
               ↓
┌─────────────────────────────────┐
│   Serveur MCP de PeasyPilot    │
│  Outils:                        │
│  - RunTests()                   │
│  - AnalyzeCode()                │
│  Ressources:                    │
│  - Test Results                 │
│  - Coverage Data                │
└──────────────┬──────────────────┘
               │
               ↓
┌─────────────────────────────────┐
│   Votre Infrastructure de Test  │
│  - dotnet test                  │
│  - TestAssistant                │
│  - Analyse de code              │
└─────────────────────────────────┘
```

---

## Concepts Clés

### Outils

Les outils sont des actions que le serveur MCP peut effectuer:

```csharp
public interface IMcpTool
{
    string Name { get; }
    string Description { get; }
    Task<dynamic> ExecuteAsync(params object[] args);
}

public class RunTestsTool : IMcpTool
{
    public string Name => "RunTests";
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        // Exécuter dotnet test
        return new { passed = 42, failed = 0 };
    }
}
```

Claude peut appeler: `RunTests()` et recevoir les résultats.

### Ressources

Les ressources sont des données que le serveur MCP expose:

```csharp
public class TestResultsResource
{
    public string Uri => "file:///tests/results.json";
    public async Task<string> ReadAsync()
    {
        return JsonSerializer.Serialize(new
        {
            totalTests = 42,
            passed = 42,
            failed = 0
        });
    }
}
```

Claude peut lire: `file:///tests/results.json` et obtenir les données de test.

### Transports

MCP peut communiquer via:
- **stdio** – Pipes de processus (local, rapide)
- **HTTP** – Endpoint REST (distant, flexible)
- **WebSocket** – Bidirectionnel (temps réel)

PeasyPilot supporte stdio par défaut.

---

## Cas d'Usage

### 1. Génération de Tests Assistée par IA

```
Claude: "Génère des tests pour la classe UserService"
  ↓
Claude appelle l'outil AnalyzeCode
  ↓
Claude appelle l'outil GenerateTests
  ↓
Claude: "Généré 8 tests couvrant le chemin heureux et les erreurs"
```

### 2. Analyse de Qualité

```
Claude: "Analyse la couverture de tests pour AuthenticationService"
  ↓
Claude lit la ressource CoverageMetrics
  ↓
Claude: "La couverture est 85%. Cas limites manquants: gestion des timeouts"
```

---

## Quand Utiliser MCP

✅ **Bon:**
- Développement assisté par IA
- Génération automatisée de tests
- Analyse continue du code
- Intégration aux workflows IA

❌ **Pas nécessaire pour:**
- Écriture manuelle de tests
- Exécution locale de tests
- CI/CD standard

---

## Prochaines Étapes

👉 [Guide d'Intégration](./mcp-integration-guide-FR.md)

MCP = L'API publique de votre infrastructure de test! 🤖

---

**[← Retour à la Documentation MCP](./README.md)** | **[← Retour au Hub Documentation](../README.md)**

**Version:** Français | **[English](./mcp-overview.md)**
