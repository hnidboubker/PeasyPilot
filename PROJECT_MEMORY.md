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

## Open Questions

- Is there an existing `.agents/` template from another Houssine project that should
  have been copied verbatim instead of drafted fresh? (Asked; owner chose to have it
  drafted here and reviewed instead of pointing to a reference project.)
- Should `PROJECT_MEMORY.md` eventually record CI/release specifics (build.yml,
  coverage.yml, release.yml under `.github/`) in more detail, or stay high-level?

## Important Constraints

- Rotate this file at 300 lines: create `PROJECT_MEMORY_01.md`, have this file point
  to it via `Next`, and have the new file set `Prev` back to this one. Keep `Root`
  pointing at `PROJECT_MEMORY.md` in every file in the chain.
- Read order before any action in this repo: this file, then
  `.agents/00_START_HERE.md`, `.agents/02_QUESTION_PROTOCOL.md`, `.agents/01_RULES.md`,
  `.agents/03_CHECKLIST_BEFORE_COMMIT.md`, `.agents/04_LANGUAGE_SPECIFIC.md`,
  `.agents/05_ARCHITECTURE.md`, `.agents/06_SKILLS_AVAILABLE.md`,
  `.agents/07_AUDIT_REQUIREMENTS.md`, `.agents/08_AUTO_ISSUE_SKILL.md`.
