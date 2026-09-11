# MCP Documentation

Model Context Protocol integration for AI-assisted testing with Claude and other AI models.

---

## What is MCP?

**Model Context Protocol (MCP)** is a standard protocol that connects AI models to tools, resources, and data sources.

In plain English: **MCP lets Claude (or other AI) run tests, analyze code, and generate test cases using your test framework as if it were an API.**

---

## Quick Start

### 1. New to MCP?
Start with the **[MCP Overview](./mcp-overview.md)** (5 minutes)

Understand:
- What MCP is and why it matters
- How it connects AI to testing
- Architecture and components

### 2. Ready to integrate?
Follow **[MCP Integration Guide](./mcp-integration-guide.md)** (15 minutes)

Learn:
- Installation steps
- Configuring PeasyPilot MCP server
- Connecting to Claude or other AI
- Testing the connection

### 3. See it in action
Check **[MCP Examples](./mcp-examples.md)** (working examples)

Real-world scenarios:
- Running tests from Claude
- Generating test cases with AI
- Analyzing code coverage
- Using MCP resources

---

## Recommended Learning Path

### For First-Time Users

1. **[MCP Overview](./mcp-overview.md)** — Understand the concept (5 min)
2. **[MCP Integration Guide](./mcp-integration-guide.md)** — Set it up (15 min)
3. **[MCP Examples](./mcp-examples.md)** — See working code (10 min)
4. **[MCP Best Practices](./mcp-best-practices.md)** — Do it right (10 min)

### For Advanced Users

1. **[MCP API Reference](./mcp-api-reference.md)** — Complete API documentation
2. **[MCP Best Practices](./mcp-best-practices.md)** — Production patterns
3. **[MCP Examples](./mcp-examples.md)** — Advanced scenarios

---

## Documentation

### Overview & Concepts
**[MCP Overview](./mcp-overview.md)** | **[FR](./mcp-overview-FR.md)**

Learn what MCP is, why it matters, and how it works with PeasyPilot.

**Topics:**
- What is MCP?
- Why use MCP for testing?
- Architecture overview
- Workflow comparison

**Time:** 5 minutes | **Level:** Beginner

---

### Integration Guide
**[MCP Integration Guide](./mcp-integration-guide.md)** | **[FR](./mcp-integration-guide-FR.md)**

Step-by-step guide to set up MCP with PeasyPilot.

**Topics:**
- Installation
- Server configuration
- Exposing tools and resources
- Connection setup
- Verification

**Time:** 15 minutes | **Level:** Beginner | **Requirements:** .NET 8+

---

### Examples
**[MCP Examples](./mcp-examples.md)** | **[FR](./mcp-examples-FR.md)**

Working code examples showing MCP in action.

**Scenarios:**
- Running tests via MCP
- Generating test cases with AI
- Analyzing code and coverage
- Custom tool examples
- Resource exposure examples

**Time:** 10 minutes | **Level:** Beginner-Intermediate | **Examples:** 3+ working scenarios

---

### Best Practices
**[MCP Best Practices](./mcp-best-practices.md)** | **[FR](./mcp-best-practices-FR.md)**

Patterns and practices for production MCP implementations.

**Topics:**
- Design patterns for MCP tools
- Security considerations
- Performance optimization
- Error handling
- Testing MCP tools
- Common pitfalls

**Time:** 10 minutes | **Level:** Intermediate-Advanced

---

### API Reference
**[MCP API Reference](./mcp-api-reference.md)** | **[FR](./mcp-api-reference-FR.md)**

Complete API documentation for PeasyPilot MCP.

**References:**
- IMcpTool interface
- IMcpResource interface
- IStdioTransport interface
- ToolCallRequest
- ResourceRequest
- Response types
- Error codes

**Time:** Reference | **Level:** Advanced

---

## Use Cases

### Run Tests from Claude
Let Claude execute your test suite and analyze results in real-time.

**See:** [MCP Examples](./mcp-examples.md)

### Generate Test Cases
Claude analyzes your code and generates test cases automatically.

**See:** [MCP Integration Guide](./mcp-integration-guide.md) + [MCP Examples](./mcp-examples.md)

### Analyze Code Coverage
Expose coverage reports as MCP resources for AI analysis.

**See:** [MCP API Reference](./mcp-api-reference.md)

### Smart Test Suggestions
Claude suggests test cases and patterns based on your code.

**See:** [MCP Examples](./mcp-examples.md)

### CI/CD Integration
Integrate MCP into your CI/CD pipeline for AI-assisted testing.

**See:** [MCP Best Practices](./mcp-best-practices.md)

---

## FAQ

**Q: Do I need MCP for PeasyPilot?**  
A: No, MCP is optional. It's useful if you want AI-assisted testing with Claude.

**Q: Can I use MCP with other AI models?**  
A: Yes! MCP is a standard protocol. Claude is just one example.

**Q: Is MCP secure?**  
A: See [Best Practices](./mcp-best-practices.md) for security guidelines.

**Q: How do I debug MCP issues?**  
A: Check [Troubleshooting](../ADVANCED/troubleshooting-debugging.md) and [API Reference](./mcp-api-reference.md).

**Q: Can I expose custom tools?**  
A: Yes! See [MCP API Reference](./mcp-api-reference.md) and [Best Practices](./mcp-best-practices.md).

---

## Quick Reference

### All MCP Files

| File | Purpose | Time |
|------|---------|------|
| [Overview](./mcp-overview.md) | Understand MCP concept | 5 min |
| [Integration Guide](./mcp-integration-guide.md) | Set up MCP | 15 min |
| [Examples](./mcp-examples.md) | Working code | 10 min |
| [Best Practices](./mcp-best-practices.md) | Production patterns | 10 min |
| [API Reference](./mcp-api-reference.md) | Complete API docs | Reference |

### French Versions (Versions Françaises)

| Fichier | Objectif | Durée |
|---------|----------|-------|
| [Aperçu](./mcp-overview-FR.md) | Comprendre MCP | 5 min |
| [Guide d'Intégration](./mcp-integration-guide-FR.md) | Configurer MCP | 15 min |
| [Exemples](./mcp-examples-FR.md) | Code fonctionnel | 10 min |
| [Bonnes Pratiques](./mcp-best-practices-FR.md) | Patterns de production | 10 min |
| [Référence API](./mcp-api-reference-FR.md) | Documentation API complète | Référence |

---

## Getting Started

### Step 1: Understand MCP (5 min)
Read **[MCP Overview](./mcp-overview.md)**

### Step 2: Install & Configure (15 min)
Follow **[MCP Integration Guide](./mcp-integration-guide.md)**

### Step 3: See It Work (10 min)
Review **[MCP Examples](./mcp-examples.md)**

### Step 4: Go Deeper
Read **[Best Practices](./mcp-best-practices.md)** and **[API Reference](./mcp-api-reference.md)**

---

## Resources

- **[Back to Documentation Hub](../README.md)**
- **[Getting Started](../GETTING-STARTED.md)** — Getting started with PeasyPilot
- **[Learning Guides](../GUIDES/README.md)** — Testing patterns and best practices
- **[API References](../REFERENCE/README.md)** — Complete API documentation
- **[Advanced Topics](../ADVANCED/README.md)** — Advanced patterns and troubleshooting
- **[Main README](../../README.md)** — Project overview

---

## Need Help?

- **Troubleshooting:** [Troubleshooting Guide](../ADVANCED/troubleshooting-debugging.md)
- **API Questions:** [API Reference](./mcp-api-reference.md)
- **Issues:** [GitHub Issues](https://github.com/hnidboubker/PeasyPilot/issues)

---

**Ready to connect AI to your tests?** Start with [MCP Overview](./mcp-overview.md)! 🤖
