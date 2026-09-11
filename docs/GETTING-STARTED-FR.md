# Premiers pas avec PeasyPilot

Bienvenue! Ce guide vous amènera de zéro à votre premier test fonctionnel 
en environ 25 minutes.

## Qu'est-ce que PeasyPilot?

PeasyPilot est un framework de test modulaire pour .NET qui vous permet 
d'écrire des tests unitaires, des tests d'intégration et des tests de 
style BDD en utilisant une API cohérente et épurée.

Considérez-le comme une **expérience de test unifiée** sur xUnit, NUnit 
et TUnit, avec un support intégré pour les mocks, la génération de fausses 
données, les tests d'intégration et les scénarios BDD.

## Pourquoi devrais-je m'en soucier?

- **Une API pour tous les tests** – Même style pour les tests unitaires, 
  d'intégration ou BDD
- **Utilitaires de test intégrés** – Builders, assertions, mocking, 
  génération de fausses données
- **Tests d'intégration simplifiés** – Fixtures de base de données, 
  réinitialisation automatique, isolation propre
- **Support BDD** – Écrivez des scénarios en Gherkin, liez les étapes 
  automatiquement
- **Agnostique au framework** – Fonctionne avec xUnit, NUnit ou TUnit

## Configuration système requise

- **.NET 8.0, 9.0 ou 10.0**
- **C# 10.0 ou version ultérieure**
- Votre framework de test préféré (xUnit, NUnit ou TUnit)

## Installation (3 minutes)

### Étape 1: Créer ou ouvrir un projet de test

```bash
dotnet new xunit -n MonProjet.Tests
cd MonProjet.Tests
```

### Étape 2: Ajouter les packages PeasyPilot

```bash
# Utilitaires de test principaux
dotnet add package PeasyPilot.Unit

# Intégration du framework (choisir un)
dotnet add package PeasyPilot.XUnit      # Pour xUnit
dotnet add package PeasyPilot.NUnit      # Pour NUnit
dotnet add package PeasyPilot.TUnit      # Pour TUnit

# Optionnel: mocking, fausses données, BDD
dotnet add package PeasyPilot.Moq        # Pour les mocks
dotnet add package PeasyPilot.Bogus      # Pour les fausses données
dotnet add package PeasyPilot.BDD        # Pour les tests BDD
```

### Étape 3: Vérifier l'installation

```bash
dotnet build
```

Résultat attendu: `Build succeeded. 0 Warning(s)`

---

## Votre premier test unitaire (8 minutes)

Écrivons un simple test unitaire pour vérifier que vous êtes prêt.

### Étape 1: Créer une classe à tester

Créez `src/Calculatrice.cs`:

```csharp
namespace MonProjet;

/// <summary>
/// Calculatrice simple pour les tests
/// </summary>
public class Calculatrice
{
    /// <summary>
    /// Ajoute deux nombres
    /// </summary>
    public int Ajouter(int a, int b) => a + b;
    
    /// <summary>
    /// Multiplie deux nombres
    /// </summary>
    public int Multiplier(int a, int b) => a * b;
}
```

### Étape 2: Créer votre premier test

Créez `Tests/CalculatriceTests.cs`:

```csharp
using Xunit;
using PeasyPilot.XUnit;
using MonProjet;

namespace MonProjet.Tests;

/// <summary>
/// Tests pour la classe Calculatrice
/// </summary>
public class CalculatriceTests : PeasyPilotTestBase
{
    private Calculatrice _calculatrice = null!;
    
    public override async Task InitializeAsync()
    {
        // Appeler l'initialisation de base d'abord
        await base.InitializeAsync();
        
        // Créer l'instance de calculatrice pour chaque test
        _calculatrice = new Calculatrice();
    }
    
    [Fact]
    public void Ajouter_AvecNombresPositifs_RetourneLaSomme()
    {
        // Arrange
        int a = 5;
        int b = 3;
        int attendu = 8;
        
        // Act
        int resultat = _calculatrice.Ajouter(a, b);
        
        // Assert
        Assert.Equal(attendu, resultat);
    }
    
    [Fact]
    public void Multiplier_AvecDeuxNombres_RetourneLeProduit()
    {
        // Arrange
        int a = 4;
        int b = 5;
        int attendu = 20;
        
        // Act
        int resultat = _calculatrice.Multiplier(a, b);
        
        // Assert
        Assert.Equal(attendu, resultat);
    }
}
```

### Étape 3: Exécuter le test

```bash
dotnet test
```

Sortie attendue:
```
Test Run Successful.
Total tests: 2
     Passed: 2
     Failed: 0
```

✅ **Félicitations!** Vos premiers tests PeasyPilot fonctionnent!

---

## Anatomie du test: Ce qui vient de se passer?

Chaque test PeasyPilot suit le **modèle AAA**:

1. **Arrange** – Configurer les données de test et les préconditions
2. **Act** – Exécuter le code que vous testez
3. **Assert** – Vérifier les résultats

Votre test:
```csharp
public void Ajouter_AvecNombresPositifs_RetourneLaSomme()
{
    // Arrange: Créer les données d'entrée
    int a = 5;
    int b = 3;
    
    // Act: Appeler la méthode
    int resultat = _calculatrice.Ajouter(a, b);
    
    // Assert: Vérifier le résultat
    Assert.Equal(attendu: 8, actual: resultat);
}
```

Ce modèle garde les tests **lisibles**, **focalisés** et **maintenables**.

---

## Prochaines étapes (2 minutes)

Vous avez maîtrisé les bases! Voici où aller ensuite:

### Voulez-vous en savoir plus sur les tests unitaires?
📖 [Guide des tests unitaires](./GUIDES/unit-testing-guide-FR.md) – Modèles avancés, assertions, builders, mocking

### Voulez-vous tester du code de base de données?
📖 [Guide des tests d'intégration](./GUIDES/integration-testing-guide-FR.md) – Fixtures, bases de données, transactions

### Voulez-vous tester le comportement métier?
📖 [Guide des tests BDD](./GUIDES/bdd-testing-guide-FR.md) – Scénarios Gherkin, définitions d'étapes

### Voulez-vous générer des tests automatiquement?
📖 [Guide de génération de tests](./GUIDES/test-generation-guide-FR.md) – Génération de code TestAssistant

### Voulez-vous comprendre MCP?
📖 [Aperçu MCP](./MCP/mcp-overview-FR.md) – Ce qu'est MCP et pourquoi c'est important pour les tests

---

## Dépannage

### "PeasyPilotTestBase not found"
Vérifiez que vous avez installé le package spécifique au framework:
```bash
dotnet add package PeasyPilot.XUnit    # Pour xUnit
dotnet add package PeasyPilot.NUnit    # Pour NUnit
dotnet add package PeasyPilot.TUnit    # Pour TUnit
```

### "InitializeAsync is not overriding any method"
Assurez-vous que vous hériterez de `PeasyPilotTestBase` et que vous utilisez 
`async Task` (pas `void`):
```csharp
public class MesTests : PeasyPilotTestBase
{
    public override async Task InitializeAsync()  // ✅ Correct
    {
        await base.InitializeAsync();
    }
}
```

### Les tests ne s'exécutent pas
Assurez-vous que vous êtes dans le bon répertoire et exécutez:
```bash
dotnet test --verbose
```

---

## Concepts clés

### PeasyPilotTestBase
C'est votre classe de base pour tous les tests. Elle fournit:
- `TestContext` – Accès au contexte d'exécution du test
- `InitializeAsync()` – Configuration avant chaque test
- `DisposeAsync()` – Nettoyage après chaque test

### TestContext
Gère les données de test, l'enregistrement de l'injection de dépendances 
et le cycle de vie. Vous l'utiliserez dans des scénarios plus avancés.

### IAsyncLifetime
Interface de cycle de vie de test asynchrone de xUnit. PeasyPilotTestBase 
l'implémente pour que vos tests puissent utiliser `async/await`.

---

## Vous êtes prêt!

Vous savez maintenant:
✅ Comment installer PeasyPilot  
✅ Comment écrire votre premier test  
✅ Comment les tests sont structurés (modèle AAA)  
✅ Où en savoir plus (les guides ci-dessous)

**Choisissez un guide et approfondissez:**

| Voulez-vous... | Lisez... |
|---|---|
| Maîtriser les tests unitaires | [Guide des tests unitaires](./GUIDES/unit-testing-guide-FR.md) |
| Tester les bases de données | [Guide des tests d'intégration](./GUIDES/integration-testing-guide-FR.md) |
| Écrire des scénarios | [Guide des tests BDD](./GUIDES/bdd-testing-guide-FR.md) |
| Générer des tests | [Guide de génération de tests](./GUIDES/test-generation-guide-FR.md) |
| Choisir un framework | [Adaptateurs de framework](./GUIDES/framework-adapters-guide-FR.md) |

Bon test! 🚀
