# Phase 7 Tier 4 & 5 — Documentation REFERENCE + ADVANCED

> **For agentic workers:** Use `superpowers:subagent-driven-development` or `superpowers:executing-plans` to implement task-by-task.

**Goal:** Complete Phase 7 documentation with comprehensive REFERENCE guides (API, CLI, configuration, errors) and ADVANCED guides (patterns, troubleshooting, performance, extensibility).

**Architecture:** 
- **Tier 4 (REFERENCE):** Detailed API reference for each package + CLI + config + error codes. 14 guides (EN+FR = 28 files).
- **Tier 5 (ADVANCED):** Performance, patterns, troubleshooting, migration, extensibility. 5 guides (EN+FR = 10 files).
- **Total Phase 7 delivery:** 49 existing + 38 new = 87 documentation files, ~80,000 words.

**Tech Stack:** Markdown, English + French, with code examples.

---

## Global Constraints

- Language: Bilingual (EN + FR)
- Location: `docs/REFERENCE/` and `docs/ADVANCED/`
- Each guide: 1,500-3,000 words minimum, with 5-10 working examples
- All code examples: Valid C# (runnable, not pseudocode)
- Version: Reference .NET 8, 9, 10

---

## Tier 4: REFERENCE Guides (Tasks 1-5, 28 files)

### Task 1: API References — Core + Unit + Integration
- Create: `docs/REFERENCE/api-core.md` + FR (2,500 words, 8 examples)
- Create: `docs/REFERENCE/api-unit.md` + FR (2,000 words, 6 examples)
- Create: `docs/REFERENCE/api-integration.md` + FR (2,200 words, 7 examples)

### Task 2: API References — BDD + Moq + Bogus + Coverage
- Create: `docs/REFERENCE/api-bdd.md` + FR (2,000 words)
- Create: `docs/REFERENCE/api-moq.md` + FR (1,500 words)
- Create: `docs/REFERENCE/api-bogus.md` + FR (1,500 words)
- Create: `docs/REFERENCE/api-coverage.md` + FR (1,500 words)

### Task 3: API References — XUnit + NUnit + TUnit
- Create: `docs/REFERENCE/api-xunit.md` + FR (1,500 words)
- Create: `docs/REFERENCE/api-nunit.md` + FR (1,500 words)
- Create: `docs/REFERENCE/api-tunit.md` + FR (1,500 words)

### Task 4: CLI + Configuration References
- Create: `docs/REFERENCE/cli-reference.md` + FR (2,000 words, all CLI commands)
- Create: `docs/REFERENCE/configuration-reference.md` + FR (1,800 words, all config options)

### Task 5: Error Codes + Troubleshooting Quick Ref
- Create: `docs/REFERENCE/error-codes.md` + FR (1,800 words, 20+ error codes)
- Create: `docs/REFERENCE/troubleshooting-quick-ref.md` + FR (1,500 words, common issues)

---

## Tier 5: ADVANCED Guides (Tasks 6-10, 10 files)

### Task 6: Performance & Optimization
- Create: `docs/ADVANCED/performance-optimization.md` + FR (2,500 words, 10+ examples)

### Task 7: Advanced Testing Patterns
- Create: `docs/ADVANCED/testing-patterns.md` + FR (2,500 words, AAA, GWT, patterns)

### Task 8: Troubleshooting & Debugging
- Create: `docs/ADVANCED/troubleshooting-debugging.md` + FR (2,200 words, debugging techniques)

### Task 9: Migration & Upgrade Guide
- Create: `docs/ADVANCED/migration-upgrade.md` + FR (2,000 words, version history, breaking changes)

### Task 10: Extensibility & Contributing
- Create: `docs/ADVANCED/extensibility-contributing.md` + FR (2,200 words, plugins, custom reporters)

---

## Task 11-12: Navigation + Final Validation

- [ ] **Task 11:** Create `docs/REFERENCE/README.md`, `docs/ADVANCED/README.md`, update cross-links
- [ ] **Task 12:** Verify all files created, sizes, cross-links, build succeeds, tests pass

---

## Execution Path

**Ready to implement 12 tasks**

**Option 1: Subagent-Driven** → Fresh subagent per task, auto-review between tasks  
**Option 2: Inline Execution** → All tasks in this session with checkpoints

Which approach?
