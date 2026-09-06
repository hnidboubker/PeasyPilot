# GitHub Issue - TUnit MSBuild VSTest Incompatibility

**Title:** [AUTO] TUnit MSBuild VSTest incompatibility - .NET 10 SDK

**Labels:** bug, tunit, .net-10

---

## Problem Description

When running TUnit tests on .NET 10 SDK (10.0.400), the following error occurs:

```
Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later.
```

This happens **even when targeting net8.0 project framework** because the .NET 10 SDK applies VSTest by default.

## Environment

- **SDK:** .NET 10.0.400
- **TUnit:** 1.66.10 (latest stable)
- **Microsoft.NET.Test.Sdk:** 17.0.0
- **Microsoft.Testing.Platform:** 2.4.0
- **Project Target:** net8.0
- **Property:** `<UseNewTestingPlatform>true</UseNewTestingPlatform>` already configured

## Root Cause

The .NET 10 SDK automatically applies the old VSTest runner (via --target:VSTest) even when:
1. `UseNewTestingPlatform=true` is set
2. Project targets net8.0
3. TUnit is correctly configured

The MSBuild targets file at `Microsoft.Testing.Platform.MSBuild.targets(355,5)` blocks execution with this error.

## Attempted Solutions (Failed)

- ✗ Setting `<UseNewTestingPlatform>true</UseNewTestingPlatform>` → Still fails with VSTest error
- ✗ Updating TUnit to latest 1.66.10 → No change, error persists
- ✗ Targeting net8.0 only → Does NOT fix (SDK still applies VSTest)

## Current Workaround

- CI workflow filters with: `Category!=net10-skip&Trait!=net10-skip`
- This masks the problem but does not resolve it

## Status

- **Non-blocking for now** (NUnit ✅, XUnit ✅ work fine with net10.0)
- **TUnit effectively limited to net8.0 only**
- **Awaiting TUnit/Microsoft.Testing.Platform patch**

## Files Affected

- `samples/PeasyPilot.TUnit.Samples/PeasyPilot.TUnit.Samples.csproj`
- `.github/workflows/build.yml` (CI filter as workaround)
- `passation.md` (documented in handoff notes)

## Next Steps

1. Monitor TUnit releases for .NET 10 SDK compatibility fix
2. Consider pinning to older Microsoft.Testing.Platform version if available
3. Explore MSBuild property workarounds

---

**Created by:** Auto-issue-on-bug-detection skill  
**Date:** 2026-09-06  
**Related:** passation.md, PROJECT_MEMORY.md
