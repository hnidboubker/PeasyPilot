# Migration & Upgrade Guide

## Overview

This guide helps you navigate upgrading PeasyPilot to the latest version while managing breaking changes, deprecated APIs, and ensuring your tests continue to run smoothly.

**Time estimate:** 30-60 minutes depending on project complexity  
**Frameworks:** xUnit, NUnit, TUnit  
**Prerequisites:** [Getting Started Guide](../GETTING-STARTED.md)

---

## Table of Contents

1. [Version Compatibility Matrix](#version-compatibility-matrix)
2. [Before You Upgrade](#before-you-upgrade)
3. [Upgrade Paths](#upgrade-paths)
4. [Breaking Changes by Version](#breaking-changes-by-version)
5. [Data & Configuration Migration](#data--configuration-migration)
6. [Testing After Upgrade](#testing-after-upgrade)
7. [Deprecation Policy](#deprecation-policy)
8. [Rollback Procedures](#rollback-procedures)
9. [FAQ](#faq)
10. [Getting Help](#getting-help)

---

## Version Compatibility Matrix

### .NET Framework Support

| PeasyPilot Version | .NET 8.0 | .NET 9.0 | .NET 10.0 | Status |
|---|---|---|---|---|
| 0.1.x (current) | ✅ | ✅ | ✅ | Active |
| 0.2.x (upcoming) | ✅ | ✅ | ✅ | Planned |
| 1.0.x (future) | ✅ | ✅ | ✅ | Planned |

### Package Version Compatibility

All PeasyPilot packages ship in lockstep. When upgrading, ensure all packages use the same version:

```xml
<!-- ❌ DON'T MIX VERSIONS -->
<PackageReference Include="PeasyPilot.Core" Version="0.1.5" />
<PackageReference Include="PeasyPilot.Unit" Version="0.1.3" />

<!-- ✅ DO ALIGN VERSIONS -->
<PackageReference Include="PeasyPilot.Core" Version="0.1.5" />
<PackageReference Include="PeasyPilot.Unit" Version="0.1.5" />
```

### Dependency Requirements

| PeasyPilot Version | Min C# | Min MSBuild | Tested Frameworks |
|---|---|---|---|
| 0.1.x | 10.0 | 17.0 | xUnit 2.4+, NUnit 3.13+, TUnit 1.0+ |
| 0.2.x | 11.0 | 17.5 | xUnit 2.6+, NUnit 4.0+, TUnit 1.1+ |

---

## Before You Upgrade

### 1. Check Your Current Version

```bash
# List installed PeasyPilot packages
dotnet package list PeasyPilot
```

### 2. Review Release Notes

Visit the [GitHub Releases](https://github.com/hnidboubker/PeasyPilot/releases) page for:
- New features
- Breaking changes
- Deprecations
- Migration guides

### 3. Backup Your Code

```bash
# Create a safe backup branch
git checkout -b backup/before-upgrade-0.1.5
git push origin backup/before-upgrade-0.1.5
```

### 4. Run Current Tests

Ensure all tests pass before upgrading:

```bash
dotnet build
dotnet test
```

**Status:** All tests must pass before proceeding.

---

## Upgrade Paths

### Path 1: Simple Patch Upgrade (0.1.4 → 0.1.5)

**Time:** 5 minutes | **Risk:** Low | **Breaking changes:** None

#### Step 1: Update NuGet packages

```bash
dotnet package update --upgrade-dependency "PeasyPilot*"
```

Or in Visual Studio Package Manager:
```
Update-Package PeasyPilot* -IncludePrerelease
```

#### Step 2: Rebuild

```bash
dotnet clean
dotnet build
```

#### Step 3: Run tests

```bash
dotnet test
```

**Expected:** All tests pass without code changes.

---

### Path 2: Minor Version Upgrade (0.1.x → 0.2.x)

**Time:** 15-30 minutes | **Risk:** Low-Medium | **Breaking changes:** Yes, with guidance

#### Step 1: Update packages

```bash
dotnet package update --upgrade-dependency "PeasyPilot*" --version-range "0.2"
```

#### Step 2: Review breaking changes

See [Breaking Changes by Version](#breaking-changes-by-version) for detailed guidance.

#### Step 3: Update your code

Use the migration guide below to update your code to use new APIs.

#### Step 4: Verify compilation

```bash
dotnet build
```

If compilation fails, see [FAQ](#faq) for common issues.

#### Step 5: Run tests

```bash
dotnet test
```

---

### Path 3: Major Version Upgrade (0.x → 1.0)

**Time:** 1-2 hours | **Risk:** High | **Breaking changes:** Major

Major version upgrades require significant code changes. Plan accordingly:

1. **Schedule:** Dedicate focused time (not during urgent sprints)
2. **Branch:** Create a dedicated upgrade branch
3. **Review:** Read all [Breaking Changes](#breaking-changes-by-version) sections
4. **Migrate:** Follow tier-by-tier migration guides
5. **Test:** Extensive testing required
6. **Review:** Code review before merging to main

#### Example: Major Upgrade Process

```bash
# Create upgrade branch
git checkout -b feat/upgrade-to-1.0
git push origin feat/upgrade-to-1.0

# Update packages
dotnet package update --upgrade-dependency "PeasyPilot*" --version-range "1.0"

# Make code changes (follow guides below)
# ...

# Test thoroughly
dotnet test

# Create pull request for review
# ... (create PR in GitHub)
```

---

## Breaking Changes by Version

### 0.1.5 → 0.2.0

#### Change 1: ITestFixture Renamed to ITestContext

**Impact:** Medium  
**Affected packages:** PeasyPilot.Core, all framework packages

**Old API:**
```csharp
using PeasyPilot.Core;

public class UserRepositoryTests : XUnitTestFixture
{
    public void Test_Method()
    {
        // ITestFixture methods
        var context = Fixture.GetContext();
    }
}
```

**New API:**
```csharp
using PeasyPilot.Core;

public class UserRepositoryTests : XUnitTestContext
{
    public void Test_Method()
    {
        // ITestContext methods
        var context = Context.GetContext();
    }
}
```

**Migration steps:**
1. Replace `XUnitTestFixture` with `XUnitTestContext`
2. Replace `Fixture` property with `Context` property
3. Update method names: `Fixture.GetContext()` → `Context.GetContext()`

**Automated fix (search & replace):**
```regex
Search:  Fixture\.
Replace: Context.
```

---

#### Change 2: Deprecated Methods Removed

**Impact:** Low  
**Affected packages:** PeasyPilot.Unit

The following deprecated methods from 0.1.x are removed:

- `TestBuilder.WithTimeout()` → Use `[Timeout(ms)]` attribute instead
- `TestBuilder.WithIgnore()` → Use `[Skip("reason")]` attribute instead
- `AssertThat.IsEqual()` → Use `Assert.Equal()` from xUnit directly

**Migration example:**
```csharp
// ❌ OLD (0.1.x)
[Fact]
public void Test_Method()
{
    var builder = new TestBuilder()
        .WithTimeout(5000)
        .WithIgnore("Not ready yet");
}

// ✅ NEW (0.2.x)
[Fact(Timeout = 5000)]
[Skip("Not ready yet")]
public void Test_Method()
{
    // Your test
}
```

---

#### Change 3: BDD Step Binding Pattern Changes

**Impact:** High for BDD users, Low for others  
**Affected packages:** PeasyPilot.BDD

Step binding patterns are now stricter with better validation:

**Old pattern:**
```csharp
[Given("I have {count} users")]
public void CreateUsers(string count)
{
    var num = int.Parse(count); // Manual conversion
}
```

**New pattern:**
```csharp
[Given("I have {count:int} users")]
public void CreateUsers(int count)
{
    // Automatic type conversion
}
```

**Supported type parameters:**
- `{name}` - string (default)
- `{count:int}` - integer
- `{amount:decimal}` - decimal
- `{enabled:bool}` - boolean
- `{date:date}` - DateTime

**Migration checklist:**
- [ ] Update step patterns with explicit type hints
- [ ] Remove manual type conversions from step methods
- [ ] Test with `dotnet test` to ensure patterns still match

---

### 0.2.0 → 1.0.0 (Future)

To be documented when 1.0.0 is released. Subscribe to [release notifications](https://github.com/hnidboubker/PeasyPilot/releases) for updates.

---

## Data & Configuration Migration

### Test Results Migration

If you store test results (via PeasyPilot.Coverage), migration is automatic for patch upgrades.

For minor/major upgrades, test result schemas may change:

```csharp
// 0.1.x format
{
  "testName": "UserRepository_CreateUser_Success",
  "duration": 45,
  "passed": true
}

// 0.2.x format (backward compatible)
{
  "id": "unique-id",
  "testName": "UserRepository_CreateUser_Success",
  "duration": 45,
  "passed": true,
  "tags": ["unit", "repository"],
  "coveredTypes": ["User", "UserRepository"]
}
```

**No action needed:** Old format is automatically upgraded on first read.

---

### Configuration Files

If using `PeasyPilot.CLI` with a config file (`peasy.config.json`):

**0.1.x format:**
```json
{
  "testFilter": "Category=Unit",
  "parallel": true,
  "timeout": 30000
}
```

**0.2.x format:**
```json
{
  "discovery": {
    "filter": "Category=Unit",
    "includeSkipped": false
  },
  "execution": {
    "parallel": true,
    "timeout": 30000,
    "retries": 0
  }
}
```

**Migration:** Your old config will work but trigger deprecation warnings. Update at your convenience.

---

### Custom Extensions

If you've built custom extensions (ITestDiscovery, ITestOrchestrator):

**Before upgrading:**
1. Review the extension interfaces in [Architecture](../.agents/05_ARCHITECTURE.md)
2. Check the [API Reference](../REFERENCE/core-package-reference.md)
3. Test thoroughly after upgrading

**After upgrading:**
```csharp
// Your custom implementation
public class CustomTestOrchestrator : ITestOrchestrator
{
    // Implementation details may have changed
    // Review interface definition for updates
}
```

---

## Testing After Upgrade

### Validation Checklist

Use this checklist after every upgrade:

- [ ] All packages updated to same version
- [ ] Project builds without errors (`dotnet build`)
- [ ] All tests pass (`dotnet test`)
- [ ] No compiler warnings about deprecated APIs
- [ ] Framework-specific tests pass (xUnit/NUnit/TUnit)
- [ ] Integration tests pass (database, HTTP fixtures)
- [ ] BDD tests execute scenarios correctly
- [ ] Performance benchmarks within 5% of baseline
- [ ] CI/CD pipeline passes

### Regression Testing

Run focused test suites to detect regressions:

```bash
# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"

# BDD tests only
dotnet test --filter "Category=BDD"

# Run with detailed output
dotnet test --verbosity detailed
```

### Performance Testing

Compare test execution times before/after upgrade:

```bash
# Before upgrade (on backup branch)
git checkout backup/before-upgrade-0.1.5
dotnet test --logger "console;verbosity=minimal" > results-before.txt

# After upgrade
git checkout feat/upgrade-to-0.2.x
dotnet test --logger "console;verbosity=minimal" > results-after.txt

# Compare (manual review)
```

**Acceptable variance:** Tests should not be 5%+ slower due to framework changes.

---

## Deprecation Policy

### How Deprecation Works

1. **Announce:** Feature marked as `[Obsolete]` in code
2. **Grace period:** Minimum 2 minor versions before removal
3. **Documentation:** Deprecation guide published with alternatives
4. **Removal:** Feature deleted after grace period

### Deprecation Timeline

**0.1.x:**
- `TestBuilder.WithTimeout()` - Deprecated, use `[Timeout]` attribute

**0.2.x:**
- Marked for removal in 0.4.x

**0.4.x:**
- `TestBuilder.WithTimeout()` removed

### Finding Deprecated APIs

When you upgrade, your IDE will flag deprecated code:

```csharp
// ⚠️ Compiler warning CS0618 in Visual Studio
public void OldMethod()
{
    builder.WithTimeout(5000); // 'WithTimeout' is obsolete
}
```

**Action:** Use the suggested alternative (shown in tooltip).

---

## Rollback Procedures

### Quick Rollback (Same Day)

If upgrade causes critical issues:

```bash
# 1. Revert to previous branch
git checkout main
git reset --hard <previous-commit-hash>

# 2. Reinstall packages
dotnet clean
dotnet package restore

# 3. Verify
dotnet build
dotnet test
```

### Documented Rollback (Same Week)

If you discovered issues after committing:

```bash
# 1. Create rollback branch
git checkout -b fix/rollback-from-0.2.0

# 2. Revert upgrade commit
git revert <upgrade-commit-hash>

# 3. Test thoroughly
dotnet build
dotnet test

# 4. Merge & document
# ... create PR with explanation
```

### Manual Package Downgrade

To downgrade a specific package:

```bash
# Downgrade to specific version
dotnet package update PeasyPilot.Core --version 0.1.5

# Verify all packages match
dotnet package list PeasyPilot
```

---

## FAQ

### Q: Can I skip minor versions? (0.1.x → 0.3.x directly?)

**A:** Generally yes, but not recommended. Each minor version documents breaking changes. If you skip 0.2.x:
1. You'll miss 0.2.x documentation
2. You may skip recommended migration steps
3. Troubleshooting becomes harder

**Recommendation:** Upgrade incrementally (0.1.5 → 0.2.0 → 0.3.0).

---

### Q: Will my existing tests break?

**A:** Depends on which APIs you use:

| Scenario | Risk | Action |
|---|---|---|
| Only unit tests, no BDD | Low | Quick patch upgrade likely fine |
| Heavy BDD use | Medium | Review [Breaking Changes](#breaking-changes-by-version) |
| Custom extensions | High | Review interface docs thoroughly |

**Safe approach:** Always run full test suite after upgrading.

---

### Q: How do I know if I'm using deprecated APIs?

**A:** Three ways:

1. **Build warnings:** Run `dotnet build` and look for CS0618 warnings
2. **IDE tooltips:** Deprecated code appears with strikethrough in VS/VS Code
3. **Documentation:** Check this guide's [Deprecation](#deprecation-policy) section

**Command to find all:**
```bash
dotnet build --no-incremental 2>&1 | grep "CS0618"
```

---

### Q: What if the tests pass but behavior changed?

**A:** This is rare but possible. To detect behavioral changes:

1. **Integration tests:** These catch most behavior changes
2. **E2E tests:** Run against real databases/services
3. **Manual testing:** Spot-check critical paths
4. **Monitoring:** Watch application logs after deployment

**If behavior changes detected:**
- Report issue on [GitHub](https://github.com/hnidboubker/PeasyPilot/issues)
- Use rollback procedure above
- Provide test case reproducing the issue

---

### Q: How long is a version supported?

**A:** Support timeline:

| Version | Released | Latest | End of Support |
|---|---|---|---|
| 0.1.x | 2026-09 | 0.1.5 | 2027-03 (6 months) |
| 0.2.x | TBD | - | - |
| 1.0.x | TBD | - | LTS (3 years) |

**LTS (Long Term Support) versions** receive security fixes beyond release.

---

### Q: Can I use PeasyPilot with .NET 7 or earlier?

**A:** No. PeasyPilot targets .NET 8+, which enables modern C# features. Upgrading your project to .NET 8 is required.

---

### Q: What about NuGet.org versions?

**A:** All versions are published to [NuGet.org](https://www.nuget.org/packages/PeasyPilot.Core/):

```bash
# See all available versions
dotnet package search PeasyPilot.Core --exact-match

# Install specific version
dotnet add package PeasyPilot.Core --version 0.1.5
```

---

## Getting Help

### Resources

- **Documentation:** [docs/](../) folder
- **Examples:** [samples/](../../samples/) folder
- **Issues:** [GitHub Issues](https://github.com/hnidboubker/PeasyPilot/issues)
- **Discussions:** [GitHub Discussions](https://github.com/hnidboubker/PeasyPilot/discussions)

### Report Issues

If you encounter problems during upgrade:

1. **Reproduce:** Create minimal test case showing issue
2. **Document:** Note your version, .NET version, and steps to reproduce
3. **Report:** [Create GitHub issue](https://github.com/hnidboubker/PeasyPilot/issues/new) with:
   - PeasyPilot version (before and after)
   - .NET version
   - Error message/stacktrace
   - Minimal code example

### Ask Questions

- [GitHub Discussions](https://github.com/hnidboubker/PeasyPilot/discussions) for general questions
- [GitHub Issues](https://github.com/hnidboubker/PeasyPilot/issues) for bugs only

---

## Summary

Upgrading PeasyPilot is straightforward:

1. **Backup** your code (`git branch`)
2. **Update** packages (`dotnet package update`)
3. **Review** breaking changes (if minor/major)
4. **Test** thoroughly (`dotnet test`)
5. **Deploy** with confidence

**Total time for typical upgrade:** 15-30 minutes

For help, refer to this guide or [GitHub discussions](https://github.com/hnidboubker/PeasyPilot/discussions).

Happy upgrading! 🚀
