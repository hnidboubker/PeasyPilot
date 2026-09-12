# Visual Studio Test Discovery Troubleshooting

## Problem

Visual Studio Test Explorer doesn't show PeasyPilot.Core.Tests, but:
- ✅ Tests compile successfully
- ✅ `dotnet test` runs all tests
- ✅ Tests pass on all frameworks (.NET 8, 9, 10)

**Why?** This is likely a Visual Studio cache or adapter issue, not a project configuration problem.

---

## Solutions

### Solution 1: Refresh Visual Studio Test Explorer (Quick Fix)

1. Open **Test Explorer** (Menu → Test → Test Explorer)
2. Click **Run All Tests** or use **Ctrl+R, A**
3. Wait for tests to discover and run
4. Close and reopen Visual Studio if tests still don't show

**⏱️ Time:** 30 seconds

---

### Solution 2: Clear Visual Studio Cache (Effective)

This often resolves adapter detection issues:

#### Option A: Delete Cache Folder
```cmd
REM Close Visual Studio first
rmdir "%USERPROFILE%\.vs" /s /q
```

#### Option B: Clean Solution
In Visual Studio:
1. **Build** → **Clean Solution**
2. Delete `bin/` and `obj/` folders manually:
   ```cmd
   cd G:\MCS\Github\apps\PeasyPilot
   rmdir tests\PeasyPilot.Core.Tests\bin /s /q
   rmdir tests\PeasyPilot.Core.Tests\obj /s /q
   ```
3. **Build** → **Rebuild Solution**
4. Re-open Test Explorer

**⏱️ Time:** 1-2 minutes

---

### Solution 3: Verify xUnit Adapter Installation (Config Check)

Test Explorer relies on `xunit.runner.visualstudio` package:

1. Check `Directory.Packages.props`:
   ```xml
   <PackageVersion Include="xunit.runner.visualstudio" Version="3.0.0" />
   ```

2. Verify in `.csproj`:
   ```xml
   <PackageReference Include="xunit.runner.visualstudio">
     <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
     <PrivateAssets>all</PrivateAssets>
   </PackageReference>
   ```

3. **Check installation:**
   ```cmd
   cd G:\MCS\Github\apps\PeasyPilot
   dotnet restore tests/PeasyPilot.Core.Tests/PeasyPilot.Core.Tests.csproj
   ```

**Status in this project:** ✅ Already configured correctly

---

### Solution 4: Run Tests from Command Line (Alternative)

Visual Studio Test Explorer is nice but not required. Use CLI instead:

```bash
# List tests (verbose discovery)
dotnet test tests/PeasyPilot.Core.Tests --list-tests

# Run specific test class
dotnet test tests/PeasyPilot.Core.Tests --filter "FullyQualifiedName~FrameworkAdapterTests"

# Run with detailed output
dotnet test tests/PeasyPilot.Core.Tests -v d

# Run single framework
dotnet test tests/PeasyPilot.Core.Tests -f net10.0

# Export JUnit results
dotnet test tests/PeasyPilot.Core.Tests --logger "trx;LogFileName=test-results.trx"
```

**⏱️ Time:** Instant | **Reliability:** 100%

---

### Solution 5: Rebuild Visual Studio Project Files

Sometimes project file metadata becomes stale:

```cmd
cd G:\MCS\Github\apps\PeasyPilot

# Generate new project cache
dotnet build tests/PeasyPilot.Core.Tests/PeasyPilot.Core.Tests.csproj /p:ContinuousIntegrationBuild=false

# Force complete regeneration
dotnet build --no-incremental
```

Then:
1. Close Visual Studio
2. Reopen the solution
3. Open Test Explorer
4. Click "Run All"

---

## Current Project Status

✅ **Configuration is correct:**
- `xunit.runner.visualstudio` v3.0.0 installed
- Tests have `[Fact]` attributes
- Multi-targeting: net8.0, net9.0, net10.0
- `IsPackable=false` set correctly

✅ **Tests execute successfully:**
```
Tests réussis : « G:\MCS\Github\apps\PeasyPilot\tests\PeasyPilot.Core.Tests\bin\Debug\net8.0\... »
Tests réussis : « G:\MCS\Github\apps\PeasyPilot\tests\PeasyPilot.Core.Tests\bin\Debug\net9.0\... »
Tests réussis : « G:\MCS\Github\apps\PeasyPilot\tests\PeasyPilot.Core.Tests\bin\Debug\net10.0\... »
```

**Issue:** Visual Studio Test Explorer display only (adapter registration issue)

---

## Why This Happens

### Root Causes:
1. **Visual Studio cache corruption** → Solution #2 fixes this
2. **xunit.runner.visualstudio not registered** → Solution #3 verifies config
3. **Build not in Debug mode** → Solution #5 rebuilds
4. **Missing project recompilation** → Solution #2 (Clean Solution)

### Why CLI Works But VS Doesn't:
- **`dotnet test`** uses xunit.console runner (doesn't need Visual Studio adapter)
- **Visual Studio Test Explorer** relies on VS-specific adapter registration
- Adapters sometimes fail to load without full cache clear

---

## Recommended Approach

For this project, use the **CLI approach (Solution #4)**:

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FrameworkAdapterTests"

# Watch mode (rebuild on file change)
dotnet watch test
```

**Advantages:**
- ✅ No Visual Studio cache issues
- ✅ Works on CI/CD pipelines
- ✅ Consistent across Windows/Linux/Mac
- ✅ Faster than VS Test Explorer
- ✅ Better for scripting

---

## If VS Test Explorer is Required

Use **Test Explorer > Configure Run Settings**:

1. Test Explorer → **Settings** (⚙️ icon)
2. Select **Run Settings** file (or create one):
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <RunSettings>
     <RunConfiguration>
       <MaxCpuCount>4</MaxCpuCount>
       <TargetFrameworkVersion>net10.0</TargetFrameworkVersion>
     </RunConfiguration>
   </RunSettings>
   ```
3. Save and refresh

---

## Commands Summary

### Quick Diagnostics
```bash
# Check test discovery
dotnet test --list-tests

# Run tests with xunit adapter
dotnet test -v n

# Verify adapter installed
dotnet nuget locals all --clear
dotnet restore
```

### Full Cache Clear (Nuclear Option)
```cmd
REM Close Visual Studio
del /s /q "%USERPROFILE%\.vs"
del /s /q "%USERPROFILE%\.nuget\packages\xunit*"
dotnet nuget locals all --clear
cd G:\MCS\Github\apps\PeasyPilot
dotnet restore
REM Reopen Visual Studio
```

---

## When to Use Each Solution

| Solution | Use When | Time |
|----------|----------|------|
| #1 (Refresh) | First attempt | 30s |
| #2 (Clear Cache) | Refresh doesn't work | 1-2m |
| #3 (Verify Config) | Adapter issues persist | 2m |
| #4 (CLI) | You don't need VS UI | instant |
| #5 (Rebuild) | Last resort | 5m |

---

## See Also

- [CI/CD Workflows](./CI-CD-WORKFLOWS.md) — How CI tests are discovered
- [CLI Reference](./REFERENCE/cli-reference.md) — `dotnet test` options
- [Setup Guide](./GETTING-STARTED.md) — Initial configuration
