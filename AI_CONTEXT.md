# AI_CONTEXT.md

**Project**: PeasyPilot | **Type**: Modular .NET testing framework
**Date Updated**: 2026-09-04 | **Status**: In Development

## What is this project?

A modular .NET testing framework for building, orchestrating, and running unit,
integration, and BDD-style testing workflows through one consistent API, with
adapters for xUnit, NUnit, and TUnit.

## Tech stack

| Layer | Choice |
|---|---|
| Language | C# |
| Targets | .NET 8, 9, 10 |
| Package versioning | Central (`Directory.Packages.props`) |
| Solution format | `.slnx` (`easy-peasy.slnx`) |
| Fake data | Bogus (`PeasyPilot.Bogus`) |
| Mocking | Moq (`PeasyPilot.Moq`) |
| CI | GitHub Actions (`.github/workflows/`: build, coverage, release) |
| Distribution | NuGet, one package per `src/PeasyPilot.*` project |

## High-level architecture

```
PeasyPilot.Core (abstractions, context, discovery, orchestration, reporting)
        ^
        |
        +-- PeasyPilot.CLI (filtering, scheduling, impact analysis, CI reports)
        +-- PeasyPilot.Unit (builder-oriented test helpers)
        +-- PeasyPilot.Integration (integration fixtures)
        +-- PeasyPilot.BDD (feature/scenario model)
        +-- PeasyPilot.Coverage (coverage reporting)
        +-- PeasyPilot.Bogus / PeasyPilot.Moq (optional add-ons)
        +-- PeasyPilot.XUnit / .NUnit / .TUnit (per-framework adapters)
```

Full detail: `.agents/05_ARCHITECTURE.md`.

## Key components

| Component | Role |
|---|---|
| `PeasyPilot.Core/Discovery` | Finds tests, respecting filters and impact analysis |
| `PeasyPilot.Core/Orchestration`, `Engines` | Drive test execution |
| `PeasyPilot.Core/Assertions` | Fluent `Assert.That(...)` API, framework-agnostic |
| `PeasyPilot.Core/Reporting` | CI-friendly JSON / JUnit output |
| `PeasyPilot.CLI` | Command-line entry point for running/filtering tests |

## How it works

1. **Discovery** locates tests via `Discovery/`, applying `Filters/` and
   `ImpactAnalysis/` when a scoped run is requested.
2. **Orchestration/Engines** execute tests, using `Context/` to carry run state and
   `Configuration/` for run settings.
3. **Assertions** provide the fluent API test code calls, independent of the
   underlying test framework.
4. **Reporting** turns results into CI-consumable output.
5. Framework adapters (`XUnit`/`NUnit`/`TUnit`) wire their framework's lifecycle
   into steps 1–4 without duplicating `Core` logic.

## Current state

- Core packages and framework adapters exist under `src/`.
- Tests for `Core` exist under `tests/PeasyPilot.Core.Tests`, mirroring `Core`'s
  folder structure.
- Worked examples exist under `samples/` for NUnit, XUnit, and TUnit.
- Governance scaffolding (`PROJECT_MEMORY.md`, `.agents/`, this file,
  `PROJECT_INDEX.md`) was added 2026-09-04 — no prior project-level governance
  existed before that.
- MCP GitHub auto-issue integration (`.agents/09_MCP_GITHUB_CONFIG.md`) is
  documented but **not yet configured** for this repo.

## Important constraints

- `PeasyPilot.Core` must not take a hard dependency on `Bogus` or `Moq` — those
  stay optional add-ons.
- Framework adapters (`XUnit`/`NUnit`/`TUnit`) depend on `Core`, never the reverse.
- Public types under `src/PeasyPilot.*` ship to NuGet — treat their public API as a
  compatibility contract.
- Human-controlled git: only Houssine runs `git commit` / `git push` / `git merge`.

## Essential commands

```bash
dotnet build easy-peasy.slnx
dotnet test
dotnet add package PeasyPilot.Core
```

## Files to read next

1. `CLAUDE.md` (global, `C:\Users\DevOps\.claude\CLAUDE.md`) — governance rules
2. `PROJECT_INDEX.md` — what to read for which task
3. `.agents/05_ARCHITECTURE.md` — detailed architecture
4. `PROJECT_MEMORY.md` — history and decisions

---

**Maintained by**: Houssine + assistant (see `.agents/11_MAINTENANCE_AI_CONTEXT_INDEX.md`)
