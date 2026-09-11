# Documentation Refactor Design Specification

**Date:** 2026-09-11  
**Owner:** Houssine  
**Scope:** Complete restructuring of PeasyPilot documentation  
**Status:** APPROVED FOR IMPLEMENTATION  

---

## 1. Overview & Goals

### Problem Statement
Current documentation lacks professional structure, clarity, and comprehensive MCP coverage. New users struggle to understand:
- Where to start learning
- How to use each feature progressively
- How to integrate MCP
- Real-world code examples

### Goals
1. **User-Centric Structure** — New users follow a clear path: Getting Started → Guides → Advanced
2. **Progressive Learning** — 5-minute quick start → 30-minute comprehensive guides → Deep references
3. **MCP Excellence** — Professional, complete MCP documentation with examples and integration patterns
4. **Code Examples Everywhere** — Every concept has working, real code samples
5. **Bilingual** — English (primary) + French (maintained alongside)

---

## 2. New Documentation Structure

### File Organization (Convention: hyphens, no underscores)

```
docs/
├─ README.md (updated landing page)
├─ GETTING-STARTED.md (EN)
├─ GETTING-STARTED-FR.md (FR)
│
├─ GUIDES/ (practical, progressive, 2000-3000 words each)
│  ├─ unit-testing-guide.md (EN)
│  ├─ unit-testing-guide-FR.md
│  ├─ integration-testing-guide.md (EN)
│  ├─ integration-testing-guide-FR.md
│  ├─ bdd-testing-guide.md (EN)
│  ├─ bdd-testing-guide-FR.md
│  ├─ test-generation-guide.md (EN)
│  ├─ test-generation-guide-FR.md
│  ├─ framework-adapters-guide.md (EN)
│  └─ framework-adapters-guide-FR.md
│
├─ MCP/ (comprehensive, professional, examples-driven)
│  ├─ mcp-overview.md (EN)
│  ├─ mcp-overview-FR.md
│  ├─ mcp-integration-guide.md (EN)
│  ├─ mcp-integration-guide-FR.md
│  ├─ mcp-examples.md (EN)
│  ├─ mcp-examples-FR.md
│  ├─ mcp-best-practices.md (EN)
│  ├─ mcp-best-practices-FR.md
│  ├─ mcp-api-reference.md (EN)
│  └─ mcp-api-reference-FR.md
│
├─ REFERENCE/ (technical API reference, enhanced with examples)
│  ├─ core-package-reference.md (EN)
│  ├─ core-package-reference-FR.md
│  ├─ unit-package-reference.md (EN)
│  ├─ unit-package-reference-FR.md
│  ├─ integration-package-reference.md (EN)
│  ├─ integration-package-reference-FR.md
│  ├─ bdd-package-reference.md (EN)
│  ├─ bdd-package-reference-FR.md
│  ├─ moq-bogus-coverage-reference.md (EN)
│  ├─ moq-bogus-coverage-reference-FR.md
│  ├─ cli-reference.md (EN)
│  ├─ cli-reference-FR.md
│  ├─ test-assistant-reference.md (EN)
│  └─ test-assistant-reference-FR.md
│
├─ ADVANCED/ (deep topics, patterns, optimization)
│  ├─ custom-extensions.md (EN)
│  ├─ custom-extensions-FR.md
│  ├─ performance-tuning.md (EN)
│  ├─ performance-tuning-FR.md
│  ├─ ci-cd-integration.md (EN)
│  └─ ci-cd-integration-FR.md
│
├─ PACKAGING.md (keep as-is)
└─ superpowers/specs/ (spec files)
```

### Legacy Files
- Move existing `PeasyPilot-*.md` (EN) → `REFERENCE/` with updates
- Move existing `PeasyPilot-*-FR.md` → `REFERENCE/` with updates
- Deprecate original locations (but keep for backward compatibility)

---

## 3. Detailed Content Specifications

### 3.1 README.md (Updated Landing Page)

**Purpose:** 30-second first impression + navigation  
**Length:** ~500 words  
**Content:**
- Badge row (Build, Coverage, Release, NuGet, .NET)
- What is PeasyPilot? (1 sentence + elevator pitch)
- Feature highlights (bullet list, 10 items)
- Quick install (code block)
- 3 paths: "I want to [unit test / integration test / BDD test]"
- CTA buttons: "Get Started" → GETTING-STARTED.md

**Structure:**
```markdown
# PeasyPilot
[Badges]

## What is PeasyPilot?
[Elevator pitch, 2 sentences]

## Key Features
[10 bullets]

## Installation
[Copy-paste command]

## Choose Your Path
- Unit Testing → [link]
- Integration Testing → [link]
- BDD Testing → [link]

## Documentation
[Links to GETTING-STARTED, GUIDES, MCP]
```

---

### 3.2 GETTING-STARTED.md (Progressive Entry Point)

**Purpose:** Progressive learning from zero to first test  
**Length:** ~2000 words  
**Target Time:** 25 minutes start-to-finish  

**Sections:**
1. **What is PeasyPilot?** (2 min)
   - One clear sentence
   - Why it matters (unified testing experience)
   - No assumptions about reader knowledge

2. **Installation** (3 min)
   - .NET version requirements (8, 9, 10)
   - Copy-paste: `dotnet add package PeasyPilot.Unit`
   - Verify: "Run `dotnet test` to confirm"

3. **Your First Unit Test** (8 min)
   - Complete, runnable example (Calculator class)
   - Code with line-by-line comments
   - Run the test: `dotnet test`
   - Explain the three parts (Arrange, Act, Assert)

4. **Using Built-in Utilities** (5 min)
   - TestDataFactory for test data
   - MockFactory for mocking (1 simple example)
   - Real code example

5. **Your First Integration Test** (5 min)
   - IntegrationTestFixture setup (10 lines)
   - Database test example (simple)
   - Run and verify

6. **What's Next?** (2 min)
   - Link to unit-testing-guide.md (deeper)
   - Link to integration-testing-guide.md
   - Link to bdd-testing-guide.md
   - Link to MCP overview

---

### 3.3 GUIDES/ (Comprehensive, Progressive)

Each guide follows this pattern:

**Standard Guide Structure:**
```markdown
# [Topic] Guide

## Overview
- What you'll learn
- Prerequisites
- Time estimate

## Why This Matters
- Real-world use case
- Pain points it solves

## Core Concepts
- 2-3 key ideas
- Diagrams if helpful
- Plain English explanations

## Getting Started
- Prerequisites (libraries, setup)
- Step 1: Basic setup
- Example 1 (complete code)
- Example 2 (building on Example 1)

## Advanced Patterns
- Common patterns
- Code examples
- Trade-offs and when to use each

## Best Practices
- Do's and don'ts
- Performance considerations
- Testing patterns

## Troubleshooting
- FAQ
- Common errors and solutions

## Next Steps
- Related guides
- API reference links
```

**Individual Guides:**

#### unit-testing-guide.md
- Assertions and Assert.That() API
- Builder pattern for test objects
- Working with mocking (Moq integration)
- Fake data generation (Bogus)
- AAA pattern examples
- Multiple framework examples (xUnit, NUnit, TUnit)

#### integration-testing-guide.md
- IntegrationTestFixture setup
- In-memory vs real databases
- ITestDatabase abstraction
- IResettable for singleton reset
- Transaction patterns
- Multiple framework setup (xUnit, NUnit, TUnit)

#### bdd-testing-guide.md
- Gherkin feature file syntax
- Step definitions and attributes
- Pattern matching for parameters
- StepBindingResolver mechanism
- Running BDD tests
- Step organization patterns

#### test-generation-guide.md
- TestAssistant overview
- Generating test classes
- Code analysis (what TestAssistant detects)
- Customizing generated tests
- Integration with existing tests

#### framework-adapters-guide.md
- Differences between xUnit, NUnit, TUnit
- Lifecycle management (IAsyncLifetime, SetUp/TearDown, etc.)
- Choosing a framework
- Migration between frameworks

---

### 3.4 MCP/ (Professional, Complete)

#### mcp-overview.md
**Purpose:** Understanding what MCP is and why PeasyPilot uses it

**Content:**
- What is MCP? (Model Context Protocol)
- Why integrate MCP into testing?
- Architecture: PeasyPilot ↔ MCP Transport
- Key concepts:
  - Tools (what tests can do)
  - Resources (test data, context)
  - Transports (stdio, HTTP)
  - Prompts (test scenarios)
- Use cases (AI-assisted testing, test generation, analysis)
- Diagram: MCP protocol flow

**Length:** ~1500 words

#### mcp-integration-guide.md
**Purpose:** Step-by-step integration of MCP into a test project

**Content:**
1. Install packages: `PeasyPilot.Mcp`
2. Define test tools (implement IMcpTool)
3. Create transport (StdioTransport or HttpTransport)
4. Expose resources (test data, results)
5. Register server
6. Run with MCP support

**Each step:** Code example + Explanation

**Length:** ~2000 words

#### mcp-examples.md
**Purpose:** Real-world, working code examples

**Content:**
- Example 1: AI test generator integration
  - Define a "GenerateTests" MCP tool
  - Expose code as MCP resource
  - AI client generates test code
  - Full working code (100+ lines)

- Example 2: Test analysis tool
  - Analyze test suite via MCP
  - Expose test metrics as resource
  - Coverage analysis tool
  - Full working code

- Example 3: Custom tool for CI/CD
  - Run tests via MCP
  - Report results in structured format
  - Integration with GitHub Actions

**Length:** ~3000 words (3 detailed examples)

#### mcp-best-practices.md
**Purpose:** Patterns, error handling, performance

**Content:**
- Tool design patterns
- Error handling and reporting
- Resource management (memory, connections)
- Performance optimization
- Security considerations
- Logging and debugging
- Testing MCP integrations

**Length:** ~1500 words

#### mcp-api-reference.md
**Purpose:** Complete protocol reference

**Content:**
- Tool registration and discovery
- Resource definition (schema)
- Transport protocols (stdio, HTTP)
- Message formats (JSON schema)
- Error codes and messages
- Response structure
- Examples for each API call

**Length:** ~2000 words

---

### 3.5 REFERENCE/ (Enhanced Existing Docs)

Each reference file:
- Based on existing `PeasyPilot-*.md`
- Add 3-5 code examples per major feature
- Add "When to use" guidance
- Add links to related GUIDES/
- Keep technical detail (full API)

**Example structure:**
```markdown
# Core Package Reference

## Abstractions

### ITestContext
- Purpose: Manages test execution context
- When to use: ...
- Code example

### ITestDataFactory
- Purpose: ...
- Code example: Creating test data
- Code example: Using with DI

## API Reference
[Full API, parameters, return types]

## See Also
[Links to GUIDES, other packages]
```

---

### 3.6 ADVANCED/

#### custom-extensions.md
- Extending PeasyPilot (custom assertions, builders)
- Creating custom test fixtures
- Custom DI configuration
- Code examples for each pattern

#### performance-tuning.md
- Test execution performance
- Database optimization for integration tests
- Parallel test execution
- Memory management

#### ci-cd-integration.md
- GitHub Actions workflows
- Azure DevOps integration
- Coverage reporting setup
- MCP in CI/CD pipelines

---

## 4. Content Quality Standards

### Every File Must Have:
- ✅ Clear headline (h1)
- ✅ Brief overview (what you'll learn)
- ✅ Code examples (minimum 2 per major feature)
- ✅ Links to related docs
- ✅ Links to API reference
- ✅ "Next steps" section

### Every Code Example Must:
- ✅ Be complete and runnable (or clearly marked as pseudo-code)
- ✅ Have comments explaining key lines
- ✅ Use realistic class names (not Foo/Bar)
- ✅ Show expected output or assertions
- ✅ Link to source in samples/ directory

### Every Guide Must:
- ✅ Start with "Why this matters"
- ✅ Include 5-10 code examples
- ✅ Have "Troubleshooting" section
- ✅ Have "Best practices" section
- ✅ Have time estimate

---

## 5. Bilingual Strategy (EN + FR)

### English First, French Alongside
1. Write GETTING-STARTED.md (EN)
2. Write GETTING-STARTED-FR.md (FR translation)
3. Maintain both as file pairs

### File Naming Convention
```
my-guide.md           (English)
my-guide-FR.md        (French)
```

### Translation Approach
- Professional quality, not machine translation
- Maintain technical accuracy in French
- Keep examples in code blocks (no translation)

---

## 6. Migration Plan (High-Level)

### Phase 1: Create New Structure
- Create docs/GETTING-STARTED.md (EN)
- Create docs/GUIDES/ directory
- Create docs/MCP/ directory
- Create docs/REFERENCE/ directory
- Create docs/ADVANCED/ directory

### Phase 2: Write Core Content
- GETTING-STARTED (EN + FR)
- 5 GUIDES (EN + FR)
- 5 MCP docs (EN + FR)

### Phase 3: Enhance References
- Migrate & enhance existing PeasyPilot-*.md files
- Add examples to each
- Add cross-links

### Phase 4: Polish & Deploy
- Review all links (internal + external)
- Verify all code examples work
- Update README.md with new structure
- Deprecate old file locations (keep for backward compat)

---

## 7. Success Criteria

✅ **Structural**
- All files follow hyphen naming convention
- Clear hierarchy (Getting Started → Guides → Reference → Advanced)
- All EN files have FR equivalents

✅ **Content**
- Every guide has 5+ working code examples
- Every API doc has "When to use" section
- MCP section is professional and complete (5 files, 8000+ words total)

✅ **User Experience**
- New user can go from zero to first test in <25 minutes
- MCP integration explained at beginner + advanced level
- Every concept has working code

✅ **Maintainability**
- Clear template for future guides
- Cross-links are consistent
- Easy to add new content

---

## 8. Files to Modify/Create Summary

**New Files:** ~30 files (docs + spec)  
**Modified Files:** README.md, PACKAGING.md  
**Deprecated (kept for backward compat):** Existing PeasyPilot-*.md  

**Total Documentation:** ~25,000+ words  
**Code Examples:** 50+ complete examples  

---

## Approval Checklist

- ✅ Structure approved (hub-and-spoke, progressive learning)
- ✅ File naming convention (hyphens, no underscores)
- ✅ Bilingual strategy (EN + FR pairs)
- ✅ MCP documentation scope (professional, complete)
- ✅ Content quality standards defined
- ✅ Success criteria clear

**Ready for implementation planning.**

---

