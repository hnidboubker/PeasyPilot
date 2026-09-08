File ID: PM-00
Prev: none
Next: none
Root: PROJECT_MEMORY.md

## Summary

PeasyPilot is a modular .NET testing framework (targets .NET 8/9/10) for building,
orchestrating, and running unit, integration, and BDD-style testing workflows through
a consistent API. It is split into focused packages under `src/`:

- **PeasyPilot.Core** — abstractions, test context, discovery, orchestration, reporting, DI integration
- **PeasyPilot.CLI** — command-line test runner (filtering, scheduling, impact analysis)
- **PeasyPilot.Unit** — builder-oriented utilities and shared unit-test helpers
- **PeasyPilot.Integration** — integration testing support and fixtures
- **PeasyPilot.Bogus** — fake data generation (via Bogus)
- **PeasyPilot.Moq** — mock factory abstractions (via Moq)
- **PeasyPilot.BDD** — BDD-style feature/scenario model
- **PeasyPilot.Coverage** — coverage reporting support
- **PeasyPilot.XUnit / .NUnit / .TUnit** — per-framework base-class integrations

Tests live under `tests/` (currently `PeasyPilot.Core.Tests`), and `samples/` holds
worked examples per test framework (NUnit, XUnit, TUnit).

This file and the `.agents/` folder were scaffolded on 2026-09-04 because they were
missing from the repo, per the user's global governance rule (identical `.agents/`
structure and project memory across all projects). Content was drafted by the
assistant from the actual repo layout and the user's CLAUDE.md rules, then presented
for validation — it has not been independently reviewed line-by-line by the owner.

## Decisions

- Project owner: Houssine (autodidact developer). Conversation in French, all code/
  docs/commits in English.
- All Git publish operations (commit, push, merge) are human-controlled only. Agents
  stop at `READY_FOR_COMMIT` and never run `git commit` / `git push` / `git merge`.
- Bugs/errors/failing tests detected during development trigger automatic GitHub
  issue creation (`auto-issue-on-bug-detection`) before diagnosis/resolution work
  begins (`issue-resolution`).
- No assuming, no modifying without validation, no deleting without explicit
  agreement — this applies repo-wide, not just to governance files.

## Recent Updates (2026-09-08)

**Phase 4 (BDD) Final Status:**
- T1-T5: ✅ COMPLETE — All tiers finished
- Step Binding Resolver: ✅ Implemented + 8 tests passing
- Documentation: ✅ BDD_GUIDE.md (400+ lines)
- Build: ✅ 0 errors (all frameworks)
- Tests: ✅ 126+ passing | 0 failed | 3 skipped
- Branch: phase/04-bdd (ready for merge to main)
- Status: READY_FOR_COMMIT (awaiting manual git commit/push)

**Phase 5 (AI Test Engineer) Planning Complete:**
- Vision: Transform `PeasyPilot.TestAssistant` into a full AI Test Engineer
- 4 Capabilities: Analyze → Plan → Generate → Challenge
- 5 Tiers planned with deliverables, files, and tests per tier
- Issue #35 created with full scope, checklist, and documentation list
- Status: Ready to start after Phase 4 merged to main

## Open Questions

**Phase 5 Design Decisions:**
- Test generation: Should generated tests be placed in same project or separate `*.Generated.cs` files?
- Mutation simulation: How deep should mutation patterns go? Boundary values only, or logic inversions?
- Challenge mode: Should it auto-fix weak tests or only report gaps?
- CLI integration: Should test generation support interactive mode or only batch CLI?

## Important Constraints

- Rotate this file at 300 lines: create `PROJECT_MEMORY_01.md`, have this file point
  to it via `Next`, and have the new file set `Prev` back to this one. Keep `Root`
  pointing at `PROJECT_MEMORY.md` in every file in the chain.
- Read order before any action in this repo: this file, then
  `.agents/00_START_HERE.md`, `.agents/02_QUESTION_PROTOCOL.md`, `.agents/01_RULES.md`,
  `.agents/03_CHECKLIST_BEFORE_COMMIT.md`, `.agents/04_LANGUAGE_SPECIFIC.md`,
  `.agents/05_ARCHITECTURE.md`, `.agents/06_SKILLS_AVAILABLE.md`,
  `.agents/07_AUDIT_REQUIREMENTS.md`, `.agents/08_AUTO_ISSUE_SKILL.md`.
