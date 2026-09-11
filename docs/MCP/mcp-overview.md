# MCP Overview

## What is MCP?

**Model Context Protocol (MCP)** is a standard protocol that connects AI models to tools, resources, and data sources. PeasyPilot uses MCP to expose test infrastructure to AI assistants.

**In plain English:** MCP lets Claude (or other AI) run tests, analyze code, and generate test cases using your test framework as if it were an API.

---

## Why MCP for Testing?

Traditional workflow:
```
Developer writes code
  ↓
Developer writes tests manually
  ↓
Developer runs tests
```

With MCP:
```
Developer writes code
  ↓
Claude writes tests via MCP tools
  ↓
Claude runs tests via MCP tools
  ↓
Claude analyzes coverage via MCP resources
```

**Benefits:**
- AI-assisted test generation
- Automated test analysis
- Real-time feedback loops
- Integration with Claude's reasoning

---

## Architecture

```
┌─────────────────────────────────────────┐
│          Claude / AI Model              │
└──────────────────┬──────────────────────┘
                   │ (Uses MCP Protocol)
                   ↓
┌─────────────────────────────────────────┐
│      PeasyPilot MCP Server              │
│  ┌───────────────────────────────────┐  │
│  │ Tools                             │  │
│  │ - RunTests()                      │  │
│  │ - AnalyzeCode()                   │  │
│  │ - GenerateTests()                 │  │
│  └───────────────────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │ Resources                         │  │
│  │ - Test Results                    │  │
│  │ - Code Metrics                    │  │
│  │ - Coverage Data                   │  │
│  └───────────────────────────────────┘  │
└──────────────────┬──────────────────────┘
                   │ (stdio, HTTP, WebSocket)
                   ↓
┌─────────────────────────────────────────┐
│      Your Test Infrastructure           │
│  - dotnet test                          │
│  - TestAssistant                        │
│  - Code analysis                        │
└─────────────────────────────────────────┘
```

---

## Key Concepts

### Tools

Tools are actions the MCP server can perform. Example:

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
    public string Description => "Execute the test suite";
    
    public async Task<dynamic> ExecuteAsync(params object[] args)
    {
        // Run dotnet test
        return new { passed = 42, failed = 0, duration = "1.5s" };
    }
}
```

Claude can call: `RunTests()` and receive structured results.

### Resources

Resources are data the MCP server exposes. Example:

```csharp
public class TestResultsResource
{
    public string Uri => "file:///tests/results.json";
    public string MimeType => "application/json";
    
    public async Task<string> ReadAsync()
    {
        // Return test results as JSON
        return JsonSerializer.Serialize(new
        {
            totalTests = 42,
            passed = 42,
            failed = 0,
            coverage = 0.95
        });
    }
}
```

Claude can read: `file:///tests/results.json` and get live test data.

### Transports

MCP can communicate via:

- **stdio** – Process pipes (local, fast)
- **HTTP** – REST endpoint (remote, flexible)
- **WebSocket** – Bidirectional (real-time)

PeasyPilot supports stdio by default (fastest for local development).

---

## Use Cases

### 1. AI-Assisted Test Generation

```
Claude: "Generate tests for the UserService class"
  ↓
[Claude calls AnalyzeCode tool]
  ↓
[Claude calls GenerateTests tool with analysis]
  ↓
Claude: "Generated 8 tests covering happy path, errors, and boundaries"
```

### 2. Test Quality Analysis

```
Claude: "Analyze test coverage for AuthenticationService"
  ↓
[Claude calls AnalyzeCoverage tool]
  ↓
[Claude reads CoverageMetrics resource]
  ↓
Claude: "Coverage is 85%. Missing edge cases: timeout handling, invalid tokens"
```

### 3. Continuous Feedback

```
Developer: "Run tests and analyze failures"
  ↓
Claude: [Runs tests via MCP]
  ↓
Claude: [Analyzes failures]
  ↓
Claude: "3 tests failing due to database connection timeout"
```

---

## When to Use MCP

✅ **Good fit:**
- AI-assisted development
- Automated test generation
- Continuous code analysis
- Integration with AI workflows

❌ **Not needed for:**
- Manual test writing
- Local test execution
- Standard CI/CD (use GitHub Actions instead)

---

## Getting Started

1. Install PeasyPilot.Mcp package
2. Define your MCP tools (what AI can do)
3. Expose resources (what AI can read)
4. Start the MCP server
5. Connect Claude or other AI model

👉 [Next: Integration Guide](./mcp-integration-guide.md)

---

## Key Takeaway

MCP bridges AI models and test infrastructure. It's the glue that lets Claude run tests, analyze code, and generate test cases as part of its reasoning process. 🤖
