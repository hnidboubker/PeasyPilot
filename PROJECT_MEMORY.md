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
- Files `.agents/09_MCP_GITHUB_CONFIG.md`, `10_COMPLETE_WORKFLOW.md`,
  `11_MAINTENANCE_AI_CONTEXT_INDEX.md`, plus root `AI_CONTEXT.md` and
  `PROJECT_INDEX.md`, were added 2026-09-04, modeled on
  `G:\MCS\Github\apps\test-vs-extensions-vsix\.agents\09-11` after the owner
  pointed to that project as a reference and confirmed both additions (see
  `.agents/09_MCP_GITHUB_CONFIG.md` and `.agents/11_MAINTENANCE_AI_CONTEXT_INDEX.md`).
- The GitHub MCP auto-issue integration (`.agents/09_MCP_GITHUB_CONFIG.md`) is
  documented but explicitly **not configured** for PeasyPilot as of 2026-09-04 —
  don't assume `auto-issue-on-bug-detection` can actually reach GitHub until the
  owner confirms the token/config exists.
- On 2026-09-05, the full governance set (PeasyPilot's own, as the cleanest existing
  example) was propagated to every other project under `G:\MCS\Github\apps` via a
  new reusable script, `G:\MCS\Github\apps\scaffold-governance.ps1` (companion to
  the existing `sync-auto-issue-skill.ps1`). The script only creates missing files
  — it never overwrites anything that already existed. Stack-dependent files
  (`03_CHECKLIST_BEFORE_COMMIT.md`, `04_LANGUAGE_SPECIFIC.md`, `05_ARCHITECTURE.md`,
  `07_AUDIT_REQUIREMENTS.md`) were written as TODO skeletons in projects that
  lacked them, since the script has no real knowledge of each project's stack —
  those need filling in with real content the next time work happens in that repo.
  Some existing projects were found to have extra, non-canonical files
  (`00_START_MEMORY.md`, `01_START_HERE.md`) that were left untouched (not part of
  the canonical 00-11 set, not referenced by any project's `CLAUDE.md` reading
  order, and not deleted without explicit agreement).
- Project-level `CLAUDE.md` and `AGENTS.md` were added 2026-09-04 (previously only
  the global `C:\Users\DevOps\.claude\CLAUDE.md` existed). `CLAUDE.md` holds the
  mandatory reading order and core rules only (no tech description — that's
  `AI_CONTEXT.md` / `.agents/05_ARCHITECTURE.md`, per owner's choice). `AGENTS.md`
  is a short pointer to `CLAUDE.md`, not a duplicate — kept deliberately thin so it
  never drifts out of sync.

- On branch `feature/test-generator`: added `PeasyPilot.Generator` (reflection-based
  test scaffolding, `peasypilot generate` CLI verb) — see issue #4 for the done/
  remaining checklist. While validating it, found and filed a real, pre-existing
  bug in `PeasyPilot.Bogus.TestDataFactory.Create<T>()` (always throws, reflection
  signature mismatch on `Faker<T>.Generate()`) — see issue #3. Root cause and fix
  are known but not applied yet (owner asked to file the issue only, no code
  change for now).
- GitHub issues are created via a direct HTTPS call to the GitHub REST API using
  the token already present in `~/.claude/claude_desktop_config.json`
  (`mcpServers.github.env.GITHUB_PERSONAL_ACCESS_TOKEN`) — never via `gh auth
  login`, which stays off-limits per the owner's explicit instruction. `gh auth
  status` can report the CLI's own keyring token as invalid while this
  independently-configured token still works fine for direct API calls.

- Issue #5 tracks the 9-item architecture improvement plan (test-base dedup,
  CLI reporter registry, filter combinators, AssertThat extensibility, adapter/
  engine stub question, IMockFactory ergonomics, ITestEnvironment, pipeline DI
  registration model) as one checklist, grouped by priority tier. No code written
  for any item yet.
- Found and fixed a third real bug: `Nuget.config` had `nuget.org` commented out,
  leaving only a private GitHub Packages feed — this blocked restoring NUnit/TUnit
  and their dependencies solution-wide (not related to the generator feature).
  Fixed directly on branch `feature/test-generator` (not a separate branch —
  needed immediately to unblock testing there; flag this for Houssine to split
  into its own commit if he wants it separate from the generator feature PR).
  Filed as issue #6, already resolved and verified: solution builds with 0 errors,
  all 63 pre-existing Core tests still pass.
- `tests/PeasyPilot.Generator.Tests` added (13 tests, all passing) — covers
  per-framework output shape, constructor mocking, numeric/nullable/enum/
  collection variant generation, async unwrapping, cross-namespace `using`
  generation, and the non-nullable-result TODO placeholder. Progress posted to
  issue #4.
- NUnit and TUnit generator output verified the same way XUnit was: generated
  scaffold compiles and runs against `PeasyPilot.NUnit.Samples`/`PeasyPilot.TUnit.Samples`.
  NUnit: 9/13 pass (4 expected failures, same empty-service pattern as XUnit).
  TUnit: all generated tests pass.

**[2026-09-05] Issue #3 resolved:**
  - Fixed `PeasyPilot.Bogus.TestDataFactory.Create<T>()` and `CreateMany<T>(int)` by
    removing reflection and calling `Faker<T>().Generate()` directly. Root cause was
    reflection signature mismatch: code looked for `Generate(Type.EmptyTypes)` but
    Bogus has `Generate(string? ruleSets = null)`.
  - Created `tests/PeasyPilot.Bogus.Tests` with 5 regression tests covering happy path,
    property population, collection generation, and complex types.
  - All existing tests pass; new Bogus tests pass; solution builds cleanly (0 errors).
  - Prepared for commit: `Fixes #3` will auto-close the issue on push.

**[2026-09-05] Test Generation Skills Created (feature/test-generation-skills):**
  - Created 3 new specialized skills for test generation:
    - `peasypilot-test-generator-xunit.md` — Framework-specific for xUnit
    - `peasypilot-test-generator-nunit.md` — Framework-specific for NUnit
    - `peasypilot-test-generator-tunit.md` — Framework-specific for TUnit
  - Each skill is optional, helping users generate framework-specific test scaffolds
  - Generic `peasypilot-test-generator` skill remains default (auto-detects framework)
  - All skills follow PeasyPilot patterns and integrate with Bogus/Moq
  - Updated `.agents/06_SKILLS_AVAILABLE.md` with new skill documentation
  - Prepared for PR: `feature/test-generation-skills` → `main`

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
  `.agents/07_AUDIT_REQUIREMENTS.md`, `.agents/08_AUTO_ISSUE_SKILL.md`,
  `.agents/09_MCP_GITHUB_CONFIG.md`, `.agents/10_COMPLETE_WORKFLOW.md`,
  `.agents/11_MAINTENANCE_AI_CONTEXT_INDEX.md`.
- `AI_CONTEXT.md` and `PROJECT_INDEX.md` (repo root) exist to satisfy
  `.agents/11_MAINTENANCE_AI_CONTEXT_INDEX.md` and give a fast orientation/GPS on
  top of this file — keep them current per that file's guidance.
