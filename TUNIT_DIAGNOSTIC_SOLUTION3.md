# TUnit Solution 3 Analysis: MSBuild Configuration Alternatives

## Context
**Problem**: TUnit tests fail on .NET 10 SDK (10.0.400) due to `Microsoft.Testing.Platform 2.4.0` blocking VSTest runner at the MSBuild level, even when project targets `net8.0`.

**Error Location**: `C:\Users\DevOps\.nuget\packages\microsoft.testing.platform.msbuild\2.4.0\buildMultiTargeting\Microsoft.Testing.Platform.MSBuild.targets(355,5)`

**Error Condition** (line 355):
```xml
<Error Text="Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later..."
       Condition="'$(IsTestingPlatformApplication)'=='true' AND '$(TargetFramework)'!='' AND '$(_SupportsGlobalJsonTestRunner)'=='true'" />
```

---

## Solution 3 Exploration

### Approach 1: Environment Variable Override
**Tested**: `$env:DOTNET_TEST_RUNNER_VSTEST=0`
**Result**: ❌ **FAILED** — No effect on MSBuild blocking

### Approach 2: MSBuild Property Investigation
**Discoverable Properties** (from .targets file):
- `TestingPlatformDisableCustomTestTarget` (default: false) — Controls whether custom `-t:Test` target is disabled
- `IsTestingPlatformApplication` (auto-detected) — Whether this is a Testing Platform application
- `UseNewTestingPlatform` (already set to `true` in TUnit.Samples) — Opt-in to new testing platform
- `_SupportsGlobalJsonTestRunner` (auto-calculated) — Set to `true` when .NET SDK version >= 10

**Finding**: Even with `UseNewTestingPlatform=true`, the blocage occurs because:
1. The error condition evaluates **at MSBuild parse time**, not at runtime
2. The check is in the `_MTPBeforeVSTest` target which runs **before** VSTest target
3. No documented MSBuild property exists to suppress this blocking condition

### Approach 3: Version Investigation
**Current Dependency Chain**:
- `TUnit 1.66.10` (latest stable)
  └─ `Microsoft.Testing.Platform 2.4.0` (includes .NET 10 blocking logic)

**Key Observation**: The blocking logic was *intentionally added* to Microsoft.Testing.Platform 2.4.0 to force migration from VSTest to the new Testing Platform. This is a **deliberate breaking change**, not a bug or oversight.

---

## Why Solution 3 is NOT Viable

### Root Cause Analysis
1. **SDK-Level Enforcement**: The error is generated at MSBuild targets level, before `dotnet test` runner selection
2. **Design Decision**: Microsoft deliberately blocks VSTest on .NET 10 SDK to force adoption of the new Testing Platform
3. **No Bypass Available**: No documented MSBuild property or environment variable bypasses this check
4. **Version Dependency**: TUnit 1.66.10 (latest) bundles Microsoft.Testing.Platform 2.4.0, which includes this check

### Why Each Alternative Fails
- ❌ **Environment Variables**: Ignored by MSBuild .targets file
- ❌ **MSBuild Properties**: `UseNewTestingPlatform=true` is ignored by the blocking condition
- ❌ **CustomTestTarget Disable**: Only controls the custom `-t:Test` target, not the VSTest blocker
- ❌ **Conditional Targets**: Cannot conditionally suppress MSBuild Error elements based on runtime conditions

---

## Viable Solution Paths

### Solution 1 (PARTIAL WORKAROUND) ✅
**Status**: Already applied in this project
- Limit TUnit.Samples.csproj to `net8.0` only
- Keeps TUnit functional for .NET 8.0
- CI workflow filters out .NET 10 runs via test traits
- **Limitation**: Does not enable TUnit on .NET 10

### Solution 2 (AWAIT UPSTREAM FIX) ⏳
**Status**: Pending TUnit project response
- TUnit team must update to use new Testing Platform 3.0+ or compatible version
- Microsoft.Testing.Platform 3.0+ may provide .NET 10 SDK support
- **Timeline**: Unknown; depends on TUnit maintainers
- **Tracking**: GitHub issue created to monitor

### Solution 3 (DEPRECATE TUNIT) 🗑️
**Status**: Not recommended (last resort)
- NUnit ✅ and XUnit ✅ both work on .NET 10
- Could remove TUnit adapter from this project
- TUnit would still fail on .NET 10 SDK externally
- **Cost**: Loss of TUnit capability for framework users

---

## Technical Details

### Microsoft.Testing.Platform Architecture
- **Pre-2.4.0**: VSTest supported on all SDK versions
- **2.4.0+**: VSTest explicitly blocked on SDK 10+ via MSBuild Error
- **New Platform**: Requires projects to explicitly opt-in and adapt code

### Why ".NET 10 SDK" Matters
- Check: `'$(_SdkMajorVersion)' >= '10'` (line 352 of .targets)
- This is the **build-time SDK version**, not the project's `TargetFramework`
- Even if your project targets net8.0, if you **build with SDK 10.0.400**, the check triggers
- This is the core incompatibility: presence of .NET 10 SDK → VSTest blocked globally

---

## Recommendation

**For This Project**:
- ✅ **Immediate**: Maintain current workaround (Solution 1 partial + CI filtering)
- ✅ **Short-term**: Monitor TUnit releases for .NET 10 compatibility
- ✅ **Long-term**: Decide on TUnit's future once Solution 2 status is known

**For Users**:
- If using .NET 10 SDK with TUnit: Must either
  - Use NUnit or XUnit (alternative frameworks)
  - Downgrade to .NET 8/9 SDK for building
  - Await TUnit fix for Microsoft.Testing.Platform 3.0+ compatibility

---

## References
- Error Link: https://aka.ms/dotnet-test-mtp-error
- Microsoft.Testing.Platform: `.NET 10 SDK blocking introduced in version 2.4.0`
- TUnit GitHub: Awaiting issue response on .NET 10 compatibility

---

## Conclusion
Solution 3 (MSBuild alternatives) is **not viable** because the blocking is an intentional design decision in Microsoft.Testing.Platform, not a configuration issue. The solution must come from either:
1. **Upstream** (TUnit/Microsoft updates), or
2. **Alternative frameworks** (NUnit, XUnit), or
3. **Deprecation** (remove TUnit support)
