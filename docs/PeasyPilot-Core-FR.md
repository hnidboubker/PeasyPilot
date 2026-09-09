# PeasyPilot Core — Guide Complet

Abstractions de base, contexte de test, découverte, orchestration, rapports et intégration DI.

---

## Qu'est-ce que PeasyPilot.Core ?

**PeasyPilot.Core** est la fondation du framework PeasyPilot. Il fournit :

- ✅ Abstractions de test et classes de base
- ✅ Découverte de tests (trouver tous les tests)
- ✅ Orchestration de tests (pipeline d'exécution)
- ✅ Rapports de test (formatage des résultats)
- ✅ Intégration d'injection de dépendances
- ✅ Assertions fluentes

---

## Comment ça fonctionne

### Pipeline

```
1. Découverte
   ↓
   Analyser les assemblies pour les tests
   ↓
2. Orchestration
   ↓
   Préparer l'environnement
   Exécuter chaque test
   Collecter les résultats
   ↓
3. Rapports
   ↓
   Formater les résultats (JSON, JUnit, Console)
```

---

## Composants clés

### ITestCase

Représente un cas de test unique.

```csharp
public interface ITestCase
{
    string Name { get; }
    string? Description { get; }
    Type TargetType { get; }
    MethodInfo Method { get; }
}
```

### Assert — Assertions fluentes

```csharp
using PeasyPilot.Core.Assertions;

Assert.That(valeur)
    .IsNotNull()
    .IsGreaterThan(0)
    .IsLessThan(100);

Assert.That(texte)
    .Contains("attendu")
    .StartsWith("préfixe");
```

### TestContext

Contexte pour les métadonnées de test et DI.

```csharp
var services = new ServiceCollection();
services.AddScoped<IUserRepository, TestUserRepository>();

var context = new TestContext(services);
var repo = context.GetService<IUserRepository>();
```

---

## Utilisation

```csharp
// Dans votre adapter de framework
var discovery = new XUnitTestDiscovery();
var orchestrator = new XUnitTestOrchestrator();

var tests = await discovery.DiscoverAsync(typeof(MyTests).Assembly);
var resultats = await orchestrator.RunAsync(tests);
```

---

## Architecture

```
PeasyPilot.Core/
├── Abstractions/
├── Assertions/
├── Reporting/
└── Context/
```

---

**Voir aussi :** PeasyPilot-XUnit, PeasyPilot-NUnit, PeasyPilot-TUnit
