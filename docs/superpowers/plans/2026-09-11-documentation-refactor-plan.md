# Documentation Refactor Implementation Plan

> **For agentic workers:** RECOMMENDED SUB-SKILL: Use `superpowers:subagent-driven-development` (one fresh subagent per task) or `superpowers:executing-plans` (batch execution with checkpoints).

**Goal:** Transform PeasyPilot documentation into a professional, user-centric learning system with progressive guides, comprehensive MCP documentation, and 50+ code examples.

**Architecture:** Hub-and-spoke structure:
- Entry point: README.md → GETTING-STARTED.md (25 min path)
- Progressive learning: GETTING-STARTED → GUIDES (5 detailed guides)
- Advanced topics: MCP documentation (5 files), REFERENCE (enhanced), ADVANCED
- Bilingual: Each file exists in EN + FR variant

**Tech Stack:** Markdown, code examples (C#), no external dependencies

**Spec:** `docs/superpowers/specs/2026-09-11-documentation-refactor-design.md`

## Global Constraints

- **File naming:** Hyphens only, no underscores (e.g., `unit-testing-guide.md` not `unit_testing_guide.md`)
- **Bilingual:** Every content file must have EN + FR pair (`file.md` + `file-FR.md`)
- **.NET versions:** Support 8.0, 9.0, 10.0
- **Code examples:** Must be complete, runnable, or clearly marked as pseudo-code
- **Links:** All internal links must be relative paths
- **Convention:** All paths use forward slashes (/) even on Windows

---

## File Structure

### Directories (to create/update):
```
docs/
├─ README.md (update)
├─ GETTING-STARTED.md (create)
├─ GETTING-STARTED-FR.md (create)
├─ GUIDES/ (create directory)
│  ├─ unit-testing-guide.md
│  ├─ unit-testing-guide-FR.md
│  ├─ integration-testing-guide.md
│  ├─ integration-testing-guide-FR.md
│  ├─ bdd-testing-guide.md
│  ├─ bdd-testing-guide-FR.md
│  ├─ test-generation-guide.md
│  ├─ test-generation-guide-FR.md
│  ├─ framework-adapters-guide.md
│  └─ framework-adapters-guide-FR.md
├─ MCP/ (create directory)
│  ├─ mcp-overview.md
│  ├─ mcp-overview-FR.md
│  ├─ mcp-integration-guide.md
│  ├─ mcp-integration-guide-FR.md
│  ├─ mcp-examples.md
│  ├─ mcp-examples-FR.md
│  ├─ mcp-best-practices.md
│  ├─ mcp-best-practices-FR.md
│  ├─ mcp-api-reference.md
│  └─ mcp-api-reference-FR.md
├─ REFERENCE/ (create directory)
│  ├─ core-package-reference.md
│  ├─ core-package-reference-FR.md
│  ├─ unit-package-reference.md
│  ├─ unit-package-reference-FR.md
│  ├─ integration-package-reference.md
│  ├─ integration-package-reference-FR.md
│  ├─ bdd-package-reference.md
│  ├─ bdd-package-reference-FR.md
│  ├─ moq-bogus-coverage-reference.md
│  ├─ moq-bogus-coverage-reference-FR.md
│  ├─ cli-reference.md
│  ├─ cli-reference-FR.md
│  ├─ test-assistant-reference.md
│  └─ test-assistant-reference-FR.md
├─ ADVANCED/ (create directory)
│  ├─ custom-extensions.md
│  ├─ custom-extensions-FR.md
│  ├─ performance-tuning.md
│  ├─ performance-tuning-FR.md
│  ├─ ci-cd-integration.md
│  └─ ci-cd-integration-FR.md
├─ superpowers/specs/ (already created)
└─ superpowers/plans/ (for this plan)
```

---

# PHASE 1: FOUNDATIONS

## Task 1: Create Directory Structure

**Files:**
- Create: `docs/GUIDES/` (directory)
- Create: `docs/MCP/` (directory)
- Create: `docs/REFERENCE/` (directory)
- Create: `docs/ADVANCED/` (directory)

- [ ] **Step 1: Create GUIDES directory**

```bash
mkdir -p docs/GUIDES
```

- [ ] **Step 2: Create MCP directory**

```bash
mkdir -p docs/MCP
```

- [ ] **Step 3: Create REFERENCE directory**

```bash
mkdir -p docs/REFERENCE
```

- [ ] **Step 4: Create ADVANCED directory**

```bash
mkdir -p docs/ADVANCED
```

- [ ] **Step 5: Verify directories created**

```bash
ls -la docs/ | grep -E "GUIDES|MCP|REFERENCE|ADVANCED"
```

Expected: All 4 directories listed

- [ ] **Step 6: Commit**

```bash
git add docs/
git commit -m "chore: Create documentation directory structure

- docs/GUIDES/ for progressive learning guides
- docs/MCP/ for Model Context Protocol documentation
- docs/REFERENCE/ for API reference
- docs/ADVANCED/ for advanced topics"
```

---

## Task 2: Update README.md (Landing Page)

**Files:**
- Modify: `README.md` (lines 1-100)

**Step-by-step content for README.md:**

- [ ] **Step 1: Backup current README**

```bash
cp README.md README.md.backup
```

- [ ] **Step 2: Replace Overview section**

Open `README.md` and replace the "Overview" section (currently lines 14-50) with:

```markdown
## Overview

PeasyPilot is composed of focused packages that work together to provide 
a lightweight, extensible testing foundation for .NET projects.

**New to PeasyPilot?** Start with [Getting Started](./docs/GETTING-STARTED.md) 
(25 minutes to your first test).

### Three Paths Forward

- **Unit Testing?** → [Unit Testing Guide](./docs/GUIDES/unit-testing-guide.md)
- **Integration Testing?** → [Integration Testing Guide](./docs/GUIDES/integration-testing-guide.md)
- **BDD Testing?** → [BDD Testing Guide](./docs/GUIDES/bdd-testing-guide.md)

### Included Packages

- **PeasyPilot.Core** – core abstractions, test context, discovery, orchestration, reporting, and DI integration
- **PeasyPilot.CLI** – command-line runner for filtering and scheduling tests
- **PeasyPilot.Unit** – builder-oriented utilities and shared unit-test helpers
- **PeasyPilot.Integration** – integration testing support and fixtures
- **PeasyPilot.Bogus** – fake data generation via Bogus
- **PeasyPilot.Moq** – mock factory abstractions for Moq
- **PeasyPilot.BDD** – BDD-style feature and scenario model
- **PeasyPilot.TestAssistant** – intelligent test case generation and code scaffolding
- **PeasyPilot.Coverage** – coverage reporting support
- **PeasyPilot.XUnit** – xUnit base class integration
- **PeasyPilot.NUnit** – NUnit base class integration
- **PeasyPilot.TUnit** – TUnit base class integration
```

- [ ] **Step 3: Add Documentation section before Installation**

Add this new section after the "Overview" section and before "Features":

```markdown
## Documentation

### Getting Started
- [Getting Started Guide](./docs/GETTING-STARTED.md) – 25 minutes to your first test

### Learning Guides
- [Unit Testing Guide](./docs/GUIDES/unit-testing-guide.md)
- [Integration Testing Guide](./docs/GUIDES/integration-testing-guide.md)
- [BDD Testing Guide](./docs/GUIDES/bdd-testing-guide.md)
- [Test Generation Guide](./docs/GUIDES/test-generation-guide.md)
- [Framework Adapters](./docs/GUIDES/framework-adapters-guide.md)

### MCP Integration
- [MCP Overview](./docs/MCP/mcp-overview.md) – What is MCP and why it matters
- [MCP Integration Guide](./docs/MCP/mcp-integration-guide.md) – Step-by-step setup
- [MCP Examples](./docs/MCP/mcp-examples.md) – Real working examples
- [MCP Best Practices](./docs/MCP/mcp-best-practices.md)
- [MCP API Reference](./docs/MCP/mcp-api-reference.md)

### API Reference
- [Core Package Reference](./docs/REFERENCE/core-package-reference.md)
- [Unit Package Reference](./docs/REFERENCE/unit-package-reference.md)
- [Integration Package Reference](./docs/REFERENCE/integration-package-reference.md)
- [BDD Package Reference](./docs/REFERENCE/bdd-package-reference.md)
- [CLI Reference](./docs/REFERENCE/cli-reference.md)
- [TestAssistant Reference](./docs/REFERENCE/test-assistant-reference.md)

### Advanced Topics
- [Custom Extensions](./docs/ADVANCED/custom-extensions.md)
- [Performance Tuning](./docs/ADVANCED/performance-tuning.md)
- [CI/CD Integration](./docs/ADVANCED/ci-cd-integration.md)
```

- [ ] **Step 4: Verify changes**

```bash
head -150 README.md | tail -50  # Check the Overview section is updated
```

- [ ] **Step 5: Commit**

```bash
git add README.md
git commit -m "docs: Update README with new documentation structure

- Add 'Getting Started' CTA
- Add three learning paths (Unit, Integration, BDD)
- Add comprehensive documentation section with links
- Add MCP documentation links
- Reorganize package list for clarity"
```

---

## Task 3: Create GETTING-STARTED.md (English)

**Files:**
- Create: `docs/GETTING-STARTED.md`

**Purpose:** Progressive entry point (25 minutes, zero assumptions)

- [ ] **Step 1: Create file with headers**

Create `docs/GETTING-STARTED.md`:

```markdown
# Getting Started with PeasyPilot

Welcome! This guide will take you from zero to your first working test in about 25 minutes.

## What is PeasyPilot?

PeasyPilot is a modular .NET testing framework that lets you write unit tests, 
integration tests, and BDD-style tests using a consistent, clean API.

Think of it as a **unified testing experience** across xUnit, NUnit, and TUnit, 
with built-in support for mocking, fake data generation, integration testing, 
and BDD scenarios.

## Why Should You Care?

- **One API for all tests** – Same style whether you're writing unit, integration, or BDD tests
- **Built-in test utilities** – Builders, assertions, mocking, fake data generation
- **Integration testing made easy** – Database fixtures, automatic reset, clean test isolation
- **BDD support** – Write scenarios in Gherkin, bind steps automatically
- **Framework agnostic** – Works with xUnit, NUnit, or TUnit

## System Requirements

- **.NET 8.0, 9.0, or 10.0**
- **C# 10.0 or later**
- Your favorite test framework (xUnit, NUnit, or TUnit)

## Installation (3 minutes)

### Step 1: Create or open a test project

```bash
dotnet new xunit -n MyProject.Tests
cd MyProject.Tests
```

### Step 2: Add PeasyPilot packages

```bash
# Core testing utilities
dotnet add package PeasyPilot.Unit

# Framework integration (choose one)
dotnet add package PeasyPilot.XUnit      # For xUnit
dotnet add package PeasyPilot.NUnit      # For NUnit
dotnet add package PeasyPilot.TUnit      # For TUnit

# Optional: mocking, fake data, BDD
dotnet add package PeasyPilot.Moq        # For mocking
dotnet add package PeasyPilot.Bogus      # For fake data
dotnet add package PeasyPilot.BDD        # For BDD tests
```

### Step 3: Verify installation

```bash
dotnet build
```

Expected: `Build succeeded. 0 Warning(s)`

---

## Your First Unit Test (8 minutes)

Let's write a simple unit test to verify you're all set.

### Step 1: Create a class to test

Create `src/Calculator.cs` (or use an existing class):

```csharp
namespace MyProject;

/// <summary>
/// Simple calculator for testing
/// </summary>
public class Calculator
{
    /// <summary>
    /// Adds two numbers
    /// </summary>
    public int Add(int a, int b) => a + b;
    
    /// <summary>
    /// Multiplies two numbers
    /// </summary>
    public int Multiply(int a, int b) => a * b;
}
```

### Step 2: Create your first test

Create `Tests/CalculatorTests.cs`:

```csharp
using Xunit;
using PeasyPilot.XUnit;
using MyProject;

namespace MyProject.Tests;

/// <summary>
/// Tests for Calculator class
/// </summary>
public class CalculatorTests : PeasyPilotTestBase
{
    private Calculator _calculator = null!;
    
    public override async Task InitializeAsync()
    {
        // Call base initialization first
        await base.InitializeAsync();
        
        // Create the calculator instance for each test
        _calculator = new Calculator();
    }
    
    [Fact]
    public void Add_WithPositiveNumbers_ReturnSum()
    {
        // Arrange
        int a = 5;
        int b = 3;
        int expected = 8;
        
        // Act
        int result = _calculator.Add(a, b);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void Multiply_WithTwoNumbers_ReturnsProduct()
    {
        // Arrange
        int a = 4;
        int b = 5;
        int expected = 20;
        
        // Act
        int result = _calculator.Multiply(a, b);
        
        // Assert
        Assert.Equal(expected, result);
    }
}
```

### Step 3: Run the test

```bash
dotnet test
```

Expected output:
```
Test Run Successful.
Total tests: 2
     Passed: 2
     Failed: 0
```

✅ **Congratulations!** Your first PeasyPilot tests are passing!

---

## Test Anatomy: What Just Happened?

Every PeasyPilot test follows the **AAA pattern**:

1. **Arrange** – Set up test data and preconditions
2. **Act** – Execute the code you're testing
3. **Assert** – Verify the results

Your test:
```csharp
public void Add_WithPositiveNumbers_ReturnSum()
{
    // Arrange: Create input data
    int a = 5;
    int b = 3;
    
    // Act: Call the method
    int result = _calculator.Add(a, b);
    
    // Assert: Check the result
    Assert.Equal(expected: 8, actual: result);
}
```

This pattern keeps tests **readable**, **focused**, and **maintainable**.

---

## Next Steps (2 minutes)

You've mastered the basics! Here's where to go next:

### Want to Learn More About Unit Testing?
📖 [Unit Testing Guide](./GUIDES/unit-testing-guide.md) – Deeper patterns, assertions, builders, mocking

### Want to Test Database Code?
📖 [Integration Testing Guide](./GUIDES/integration-testing-guide.md) – Fixtures, databases, transactions

### Want to Test Business Behavior?
📖 [BDD Testing Guide](./GUIDES/bdd-testing-guide.md) – Gherkin scenarios, step definitions

### Want to Generate Tests Automatically?
📖 [Test Generation Guide](./GUIDES/test-generation-guide.md) – TestAssistant code generation

### Want to Understand MCP?
📖 [MCP Overview](./MCP/mcp-overview.md) – What MCP is and why it matters for testing

---

## Troubleshooting

### "PeasyPilotTestBase not found"
Check that you installed the framework-specific package:
```bash
dotnet add package PeasyPilot.XUnit    # For xUnit
dotnet add package PeasyPilot.NUnit    # For NUnit
dotnet add package PeasyPilot.TUnit    # For TUnit
```

### "InitializeAsync is not overriding any method"
Make sure you're inheriting from `PeasyPilotTestBase` and using `async Task` (not `void`):
```csharp
public class MyTests : PeasyPilotTestBase
{
    public override async Task InitializeAsync()  // ✅ Correct
    {
        await base.InitializeAsync();
    }
}
```

### Tests aren't running
Make sure you're in the correct directory and run:
```bash
dotnet test --verbose
```

---

## Key Concepts

### PeasyPilotTestBase
This is your base class for all tests. It provides:
- `TestContext` – Access to test execution context
- `InitializeAsync()` – Setup before each test
- `DisposeAsync()` – Cleanup after each test

### TestContext
Manages test data, DI registration, and lifecycle. You'll use it in more advanced scenarios.

### IAsyncLifetime
xUnit's async test lifecycle interface. PeasyPilotTestBase implements this so your tests can use `async/await`.

---

## You're Ready!

You now know:
✅ How to install PeasyPilot  
✅ How to write your first test  
✅ How tests are structured (AAA pattern)  
✅ Where to learn more (the guides below)

**Pick a guide and dive deeper:**

| Want to... | Read... |
|-----------|---------|
| Master unit testing | [Unit Testing Guide](./GUIDES/unit-testing-guide.md) |
| Test databases | [Integration Testing Guide](./GUIDES/integration-testing-guide.md) |
| Write scenarios | [BDD Testing Guide](./GUIDES/bdd-testing-guide.md) |
| Generate tests | [Test Generation Guide](./GUIDES/test-generation-guide.md) |
| Choose a framework | [Framework Adapters](./GUIDES/framework-adapters-guide.md) |

Happy testing! 🚀
```

- [ ] **Step 2: Save the file**

The file is created with complete content above.

- [ ] **Step 3: Verify the file exists**

```bash
wc -l docs/GETTING-STARTED.md  # Should be ~250+ lines
```

- [ ] **Step 4: Commit**

```bash
git add docs/GETTING-STARTED.md
git commit -m "docs: Create GETTING-STARTED.md (English)

Progressive entry point for new users:
- System requirements and installation (3 min)
- Your first unit test from scratch (8 min)
- Test anatomy and AAA pattern (2 min)
- Navigation to deeper guides (2 min)
- Troubleshooting FAQ
- Total: 25 minutes to first passing test"
```

---

## Task 4: Create GETTING-STARTED-FR.md (French)

**Files:**
- Create: `docs/GETTING-STARTED-FR.md`

- [ ] **Step 1: Create French version**

Create `docs/GETTING-STARTED-FR.md` with French translation of GETTING-STARTED.md:

```markdown
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
```

- [ ] **Step 2: Verify file size**

```bash
wc -l docs/GETTING-STARTED-FR.md  # Should match EN version (≈250+ lines)
```

- [ ] **Step 3: Commit**

```bash
git add docs/GETTING-STARTED-FR.md
git commit -m "docs: Create GETTING-STARTED-FR.md (French translation)

Professional French translation of GETTING-STARTED.md:
- Complete parallel content (installation, first test, etc.)
- Code examples localized where appropriate
- Technical terms properly translated
- Navigation links to French guides (to be created)"
```

---

# PHASE 2: GUIDES (5 Core Guides × 2 languages = 10 files)

Due to length constraints, I'll provide the structure and first guide. Each subsequent guide follows the same pattern.

## Task 5: Create unit-testing-guide.md

**Files:**
- Create: `docs/GUIDES/unit-testing-guide.md`

**Length:** ~2500 words, 7 code examples

**Content structure:**

```markdown
# Unit Testing Guide

## Overview
You'll learn how to:
- Write clean, focused unit tests
- Use assertions effectively
- Apply builder patterns for test data
- Integrate mocking
- Best practices for maintainability

**Prerequisites:** GETTING-STARTED.md completed
**Time:** 30 minutes
**Frameworks:** xUnit, NUnit, TUnit

## Why Unit Testing Matters

[2 paragraphs: Real-world scenario, pain points solved]

## Core Concepts

### 1. Arrange-Act-Assert (AAA) Pattern
[Explanation with diagram]

### 2. Test Naming
[Naming convention: Should_Condition_ExpectedBehavior]

### 3. Single Responsibility
[One assertion per test (preferably)]

## Getting Started

### Basic Test Structure
[Code example 1: Simple assert]

### Multiple Assertions
[Code example 2: Grouped assertions]

## Advanced Patterns

### Using Builders for Complex Objects
[Code example 3: TestDataBuilder]

### Mocking Dependencies
[Code example 4: Moq integration]

### Parameterized Tests
[Code example 5: Test cases]

### Testing Exceptions
[Code example 6: Assert.Throws]

## Best Practices

[5-7 do's and don'ts with code]

## Troubleshooting

[Common issues and solutions]

## Next Steps

[Links to integration testing, BDD, TestAssistant]
```

[Create this file with detailed content following the structure above]

- [ ] **Step 1-5:** [Create file with complete content per structure]

- [ ] **Step 6: Commit**

```bash
git add docs/GUIDES/unit-testing-guide.md
git commit -m "docs: Create unit-testing-guide.md (English)

Comprehensive guide for unit testing:
- Core concepts (AAA, naming, single responsibility)
- 7 working code examples
- Builder patterns for test data
- Mocking integration with Moq
- Parameterized tests
- Exception testing
- Best practices and troubleshooting"
```

---

## Task 6-10: Create Remaining 4 Guides (EN)

Following the same pattern as Task 5, create:

- `docs/GUIDES/integration-testing-guide.md`
- `docs/GUIDES/bdd-testing-guide.md`
- `docs/GUIDES/test-generation-guide.md`
- `docs/GUIDES/framework-adapters-guide.md`

[Each file: ~2000-3000 words, 5-8 code examples, same structure]

Each task includes:
- Create file with detailed content
- Verify file exists
- Commit with descriptive message

---

## Task 11-20: Create French Translations (5 Guides × FR)

Create French versions of all 5 guides:

- `docs/GUIDES/unit-testing-guide-FR.md`
- `docs/GUIDES/integration-testing-guide-FR.md`
- `docs/GUIDES/bdd-testing-guide-FR.md`
- `docs/GUIDES/test-generation-guide-FR.md`
- `docs/GUIDES/framework-adapters-guide-FR.md`

Each is a professional translation of the corresponding EN file.

---

# PHASE 3: MCP DOCUMENTATION (5 files × 2 languages = 10 files)

## Task 21: Create mcp-overview.md (English)

**Files:**
- Create: `docs/MCP/mcp-overview.md`

**Length:** ~1500 words, diagrams, 3 code examples

**Content:**
- What is MCP (Model Context Protocol)?
- Why PeasyPilot uses it
- Architecture overview
- Key concepts (tools, resources, transports, prompts)
- Use cases for testing
- Diagram: MCP flow with PeasyPilot

---

## Tasks 22-25: Create MCP documentation (EN)

- `docs/MCP/mcp-integration-guide.md` (Step-by-step, 2000 words, 5 examples)
- `docs/MCP/mcp-examples.md` (3 working examples, 3000+ words)
- `docs/MCP/mcp-best-practices.md` (1500 words, patterns)
- `docs/MCP/mcp-api-reference.md` (2000 words, technical reference)

---

## Tasks 26-30: Create MCP Translations (FR)

French translations of all 5 MCP documentation files, maintaining technical accuracy.

---

# PHASE 4: REFERENCE DOCUMENTATION

## Task 31: Enhance core-package-reference.md

Take existing `PeasyPilot-Core.md` and enhance:
- Add 3-5 code examples per major class
- Add "When to use" guidance
- Cross-link to GUIDES
- Keep full technical detail
- Rename and update file location to `docs/REFERENCE/core-package-reference.md`

Same pattern for remaining packages:
- unit-package-reference.md
- integration-package-reference.md
- bdd-package-reference.md
- moq-bogus-coverage-reference.md
- cli-reference.md
- test-assistant-reference.md

Plus French translations for each.

---

# PHASE 5: ADVANCED DOCUMENTATION

## Task 37: Create custom-extensions.md

**Files:**
- Create: `docs/ADVANCED/custom-extensions.md`

**Content:**
- Extending PeasyPilot (custom assertions, builders)
- Creating custom test fixtures
- Custom DI configuration
- Code examples for each pattern

---

## Tasks 38-39: Create Remaining Advanced Topics

- `docs/ADVANCED/performance-tuning.md` (optimization, parallel tests)
- `docs/ADVANCED/ci-cd-integration.md` (GitHub Actions, coverage, MCP in CI)

Plus French translations.

---

# PHASE 6: VALIDATION & LINKING

## Task 40: Verify All Internal Links

- [ ] **Step 1: Create link verification script**

```bash
#!/bin/bash
# Check all markdown links
grep -r "\[" docs/ | grep "\.md" | while read line; do
    link=$(echo "$line" | grep -oP '\]\(\K[^)]+' | head -1)
    if [ -n "$link" ]; then
        if [[ $link != http* ]]; then
            # Check relative link exists
            file=$(dirname "${line%:*}/$link" | sed 's#//#/#g')
            if [ ! -f "$file" ]; then
                echo "BROKEN: $line -> $file"
            fi
        fi
    fi
done
```

- [ ] **Step 2: Run verification**

```bash
bash verify-links.sh
```

Expected: No BROKEN links

- [ ] **Step 3: Commit**

```bash
git add -A
git commit -m "docs: Verify all internal documentation links

All relative links validated:
- docs/GUIDES/ → docs/MCP/
- docs/MCP/ → docs/REFERENCE/
- docs/REFERENCE/ → docs/GUIDES/
- docs/ADVANCED/ → all sections
- README.md → all entry points

No broken links found."
```

---

## Task 41: Verify All Code Examples Run

- [ ] **Step 1: Extract and test all code examples**

```bash
# Create test project from examples
mkdir -p docs/test-examples
cd docs/test-examples
dotnet new xunit

# Copy extracted examples and verify they compile
```

- [ ] **Step 2: Run example code**

```bash
dotnet build
dotnet test
```

Expected: All examples compile and pass

---

## Task 42: Final Documentation Review

- [ ] **Step 1: Checklist completion**

```markdown
✅ File Structure
  - [ ] All EN files created (25 files)
  - [ ] All FR files created (25 files)
  - [ ] No underscores in filenames
  - [ ] Proper directory structure

✅ Content Quality
  - [ ] GETTING-STARTED is progressive (25 min)
  - [ ] All guides have 5+ examples
  - [ ] MCP docs are comprehensive
  - [ ] Code examples are runnable
  - [ ] Cross-links are consistent

✅ Bilingual
  - [ ] EN + FR pairs exist
  - [ ] Professional translations
  - [ ] No untranslated sections

✅ User Experience
  - [ ] Clear landing page (README.md)
  - [ ] Progressive learning path
  - [ ] Three entry points (Unit/Integration/BDD)
  - [ ] MCP is clearly explained

✅ Links & Navigation
  - [ ] No broken internal links
  - [ ] All navigation buttons work
  - [ ] Code examples link to source
  - [ ] Cross-package references are correct
```

- [ ] **Step 2: Final commit**

```bash
git add -A
git commit -m "docs: Complete documentation refactor

SUMMARY
=======
✅ Hub-and-spoke structure implemented
✅ 50 files created (25 EN + 25 FR)
✅ ~25,000+ words of content
✅ 50+ runnable code examples
✅ Professional MCP documentation (8000+ words)
✅ Progressive learning paths
✅ All links verified
✅ All examples tested

STRUCTURE
=========
- README.md (updated landing page)
- GETTING-STARTED.md + FR (25-min entry)
- GUIDES/ (5 guides × 2 languages)
- MCP/ (5 MCP docs × 2 languages)
- REFERENCE/ (6 API refs × 2 languages)
- ADVANCED/ (3 advanced topics × 2 languages)

NEW USER EXPERIENCE
===================
1. Arrives at README.md
2. Clicks 'Get Started' → GETTING-STARTED.md
3. 25 minutes to first passing test
4. Chooses learning path (Unit/Integration/BDD)
5. Dives into comprehensive guides
6. References API docs as needed
7. Learns MCP for advanced integration

CONVENTIONS
===========
- Hyphen naming (no underscores)
- Bilingual parity (EN + FR)
- Code examples everywhere
- Clear structure (progressive → advanced)
- Links between all sections"
```

---

## Task 43: Commit File Structure Changes

Once all documentation is created, commit the new directory structure:

```bash
git add docs/GUIDES/ docs/MCP/ docs/REFERENCE/ docs/ADVANCED/
git commit -m "chore: Documentation directory structure complete

All documentation files in place:
- 25 English content files
- 25 French translation files
- Professional structure (GUIDES, MCP, REFERENCE, ADVANCED)
- Ready for publication"
```

---

# Execution Summary

**Total Tasks:** 43  
**Total Files:** 50 (25 EN + 25 FR)  
**Total Content:** ~25,000+ words  
**Code Examples:** 50+  
**Estimated Time:** 40-60 hours (solo), 15-20 hours (distributed)

**Recommended Execution:**
1. **Subagent-driven:** One task per fresh subagent (recommended for quality)
2. **Inline batched:** 5-7 tasks per checkpoint (faster but requires review)

---

