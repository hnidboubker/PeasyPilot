# Learning Guides

Progressive, hands-on guides to master PeasyPilot testing patterns.

---

## Recommended Learning Path

Start here and progress through each guide:

### 1. Unit Testing Guide
**[Unit Testing Guide](./unit-testing-guide.md)** | **[FR](./unit-testing-guide-FR.md)**

Learn to write clean, focused unit tests using PeasyPilot.

**Topics:**
- Arrange-Act-Assert (AAA) pattern
- Fluent assertions
- Builder patterns for test data
- Mocking with Moq
- Test edge cases
- Best practices

**Time:** 30 minutes | **Level:** Beginner | **Examples:** 8 working code examples

---

### 2. Integration Testing Guide
**[Integration Testing Guide](./integration-testing-guide.md)** | **[FR](./integration-testing-guide-FR.md)**

Master testing with real databases, fixtures, and multi-component integration.

**Topics:**
- Test fixtures and setup
- Database isolation
- Integration test fixtures
- Async integration tests
- Real-world patterns
- Performance considerations

**Prerequisites:** [Unit Testing Guide](./unit-testing-guide.md)  
**Time:** 30 minutes | **Level:** Intermediate | **Examples:** 5 working code examples

---

### 3. BDD Testing Guide
**[BDD Testing Guide](./bdd-testing-guide.md)** | **[FR](./bdd-testing-guide-FR.md)**

Write tests in business language using Gherkin feature files and automatic step binding.

**Topics:**
- Gherkin feature file syntax
- Scenario writing
- Step binding with attributes
- Pattern matching and parameters
- Feature file organization
- Integration with fixtures

**Prerequisites:** [Unit Testing Guide](./unit-testing-guide.md)  
**Time:** 30 minutes | **Level:** Intermediate | **Examples:** 5 working code examples

---

### 4. Test Generation Guide
**[Test Generation Guide](./test-generation-guide.md)** | **[FR](./test-generation-guide-FR.md)**

Use intelligent test scaffolding to generate test cases automatically.

**Topics:**
- TestAssistant overview
- Analyzing types for test generation
- Framework-specific code generation
- Test battery creation
- Coverage suggestions
- Generated test structure

**Prerequisites:** [Unit Testing Guide](./unit-testing-guide.md)  
**Time:** 20 minutes | **Level:** Beginner-Intermediate | **Examples:** Working generation examples

---

### 5. Framework Adapters Guide
**[Framework Adapters Guide](./framework-adapters-guide.md)** | **[FR](./framework-adapters-guide-FR.md)**

Understand differences between xUnit, NUnit, and TUnit. Choose the right framework for your needs.

**Topics:**
- Framework comparison
- Lifecycle differences (IAsyncLifetime, SetUp/TearDown, etc.)
- Base class integration
- Async support in each framework
- Migration patterns
- Best practices per framework

**Prerequisites:** [Unit Testing Guide](./unit-testing-guide.md)  
**Time:** 25 minutes | **Level:** Beginner-Intermediate | **Examples:** 3 framework-specific examples

---

## Quick Reference

### By Testing Need

| Need | Guide | Time |
|------|-------|------|
| Write unit tests | [Unit Testing](./unit-testing-guide.md) | 30 min |
| Test with databases | [Integration Testing](./integration-testing-guide.md) | 30 min |
| Write BDD scenarios | [BDD Testing](./bdd-testing-guide.md) | 30 min |
| Auto-generate tests | [Test Generation](./test-generation-guide.md) | 20 min |
| Compare frameworks | [Framework Adapters](./framework-adapters-guide.md) | 25 min |

### By Experience Level

| Level | Start With | Then Read |
|-------|-----------|-----------|
| **Beginner** | [Unit Testing](./unit-testing-guide.md) | [Test Generation](./test-generation-guide.md) |
| **Intermediate** | [Integration Testing](./integration-testing-guide.md) | [BDD Testing](./bdd-testing-guide.md) |
| **Advanced** | All of the above | [Advanced Patterns](../ADVANCED/testing-patterns.md) |

### By Framework

| Framework | See |
|-----------|-----|
| **xUnit** | [Framework Adapters Guide](./framework-adapters-guide.md) → xUnit section |
| **NUnit** | [Framework Adapters Guide](./framework-adapters-guide.md) → NUnit section |
| **TUnit** | [Framework Adapters Guide](./framework-adapters-guide.md) → TUnit section |

---

## Learning Outcomes

After reading all 5 guides, you'll be able to:

✅ Write clean, isolated unit tests  
✅ Test code with real databases and dependencies  
✅ Write BDD scenarios in Gherkin  
✅ Automatically bind steps to test code  
✅ Generate test cases intelligently  
✅ Choose the right framework for your project  
✅ Apply best practices across all testing patterns  

---

## Getting Started

**New to PeasyPilot?** Start here:

1. Read [Getting Started Guide](../GETTING-STARTED.md) (25 minutes)
2. Read [Unit Testing Guide](./unit-testing-guide.md) (30 minutes)
3. Choose your next topic based on your needs

---

## All Guides

### English Versions
- [Unit Testing Guide](./unit-testing-guide.md)
- [Integration Testing Guide](./integration-testing-guide.md)
- [BDD Testing Guide](./bdd-testing-guide.md)
- [Test Generation Guide](./test-generation-guide.md)
- [Framework Adapters Guide](./framework-adapters-guide.md)

### French Versions (Versions Françaises)
- [Guide des Tests Unitaires](./unit-testing-guide-FR.md)
- [Guide des Tests d'Intégration](./integration-testing-guide-FR.md)
- [Guide BDD](./bdd-testing-guide-FR.md)
- [Guide de Génération de Tests](./test-generation-guide-FR.md)
- [Guide des Adaptateurs de Framework](./framework-adapters-guide-FR.md)

---

## Resources

- **[Back to Documentation Hub](../README.md)**
- **[Getting Started](../GETTING-STARTED.md)** — 25 minutes to your first test
- **[API References](../REFERENCE/README.md)** — Detailed API documentation
- **[Advanced Topics](../ADVANCED/README.md)** — Performance, troubleshooting, extensibility
- **[Main README](../../README.md)** — Project overview and quick start

---

**Ready to test?** Pick a guide above and start learning! 🚀
