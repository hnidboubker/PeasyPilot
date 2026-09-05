# 06 — Skills Available

Skills registered for this project, and when to reach for each.

## Priority skills

- **`human-controlled-git`** — absolute priority for anything touching git
  publication (`commit`, `push`, `merge`). Ensures agents stop at
  `READY_FOR_COMMIT` and never publish on Houssine's behalf.
- **`auto-issue-on-bug-detection`** — fires automatically the moment a bug, error,
  failing test, or CI failure is detected, creating a trackable GitHub issue before
  diagnosis starts.
- **`issue-resolution`** — diagnoses and fixes the problem tracked by the issue
  above: root-cause analysis, fix, tests, then stop at `READY_FOR_COMMIT`.

## Domain skills

- **`dotent-dev-webapi`** — .NET Web API & multi-tenancy specialist; use for any
  ASP.NET Core Web API or multi-tenant design work in this repo or sibling
  projects.
- **`dotnet-dev-maui`** — .NET MAUI specialist; use for MAUI/mobile-adjacent work.
- **`peasypilot-test-generator`** — generates PeasyPilot-flavored tests; auto-detects
  framework (xUnit/NUnit/TUnit/etc.). **Generic, framework-agnostic** option. Use when
  user doesn't specify a framework or wants automatic detection.
- **`peasypilot-test-generator-xunit`** — (NEW) specialized test generation **for xUnit**
  projects. Use when user specifically wants xUnit-optimized test scaffolding with
  `[Fact]`, `[Theory]` patterns and PeasyPilot.XUnit base classes.
- **`peasypilot-test-generator-nunit`** — (NEW) specialized test generation **for NUnit**
  projects. Use when user specifically wants NUnit-optimized test scaffolding with
  `[TestFixture]`, `[SetUp]` patterns and PeasyPilot.NUnit base classes.
- **`peasypilot-test-generator-tunit`** — (NEW) specialized test generation **for TUnit**
  projects. Use when user specifically wants TUnit-optimized test scaffolding with
  modern TUnit patterns and PeasyPilot.TUnit base classes.
- **`power-tools-vs-vsix-creator`** — Visual Studio VSIX extension architecture and
  packaging; use only if a VSIX/VS-extension task arises.
- **`repository-builder-archi`** — MSBuild repository architecture expert; use for
  `Directory.Build.props`/`Directory.Packages.props`/solution-layout changes.

## Test/workflow validation

- **`test-bug-detection-workflow`** — exercises the full bug-detection → auto-issue
  pipeline end to end; use to validate the automation itself, not for regular
  feature work.

## Notes

- This list must be kept in sync with the actual skills registered for the project
  (surfaced to the assistant via its skills listing at session start). If a skill
  appears there but not here, add it here; if one is removed, remove it here too —
  don't let this file drift from reality.
