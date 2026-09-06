# Packaging Guide — PeasyPilot

Guide to building, packing, and distributing PeasyPilot NuGet packages.

---

## Overview

PeasyPilot provides automated scripts for building and packaging all projects at once.

### Supported frameworks
- **.NET 8.0**
- **.NET 9.0**
- **.NET 10.0**

### Build outputs
```
artifacts/
├── PeasyPilot.Core.0.1.2.nupkg
├── PeasyPilot.BDD.0.1.2.nupkg
├── PeasyPilot.Integration.0.1.2.nupkg
├── PeasyPilot.XUnit.0.1.2.nupkg
├── PeasyPilot.NUnit.0.1.2.nupkg
├── PeasyPilot.TUnit.0.1.2.nupkg
├── PeasyPilot.Unit.0.1.2.nupkg
├── PeasyPilot.Bogus.0.1.2.nupkg
├── PeasyPilot.Moq.0.1.2.nupkg
├── PeasyPilot.Coverage.0.1.2.nupkg
└── ... (symbol packages .snupkg)
```

---

## Quick Start

### Option 1: One-Time Build & Pack

```powershell
# Build and pack all projects
dotnet build -c Release
dotnet pack -c Release -o artifacts

# Or in one command
dotnet pack -c Release -o artifacts
```

### Option 2: Watch Mode (Automatic)

```powershell
# Auto-rebuild & auto-pack on file changes (Ctrl+C to stop)
.\scripts\version-watch.ps1
```

---

## Manual Build Steps

### Step 1: Build

```bash
# Debug build (default)
dotnet build

# Release build (optimized)
dotnet build -c Release
```

### Step 2: Create artifacts directory

```bash
# Clean previous artifacts
rm -r artifacts -Force

# Create new directory
mkdir artifacts
```

### Step 3: Pack

```bash
# Pack all projects (uses Release config)
dotnet pack -c Release -o artifacts --no-build

# Or with verbose output
dotnet pack -c Release -o artifacts --no-build --verbosity detailed
```

### Step 4: Verify packages

```bash
# List generated packages
ls artifacts/*.nupkg
```

---

## Automated Watch Script

### What it does

The `version-watch.ps1` script:

1. **Monitors** all source files for changes
2. **Detects** changes in:
   - `.cs` files (C# source)
   - `.csproj` files (project files)
   - `.props`, `.targets` (configuration)
   - `.slnx`, `.json` (solution/config)
   - `.md`, `.png` (documentation)
   - `.ps1`, `.sh` (scripts)

3. **Ignores** changes in:
   - `.git/` (version control)
   - `bin/`, `obj/` (build output)
   - `artifacts/` (packages)

4. **On change detected**:
   - Runs `dotnet build -c Release`
   - Clears `artifacts/` directory
   - Runs `dotnet pack -c Release`
   - Displays package names

### Usage

**PowerShell (Windows):**
```powershell
.\scripts\version-watch.ps1
```

**Bash (Linux/Mac):**
```bash
./scripts/version-watch.sh
```

### Example Output

```
Surveillance de G:\MCS\Github\apps\PeasyPilot (Ctrl+C pour arrêter)
Build de la solution...
  Microsoft.Extensions.DependencyInjection -> ...
  PeasyPilot.Core -> ...
  PeasyPilot.BDD -> ...
  ... (all projects build)
Package de la solution...
  PeasyPilot.Core -> G:\MCS\Github\apps\PeasyPilot\artifacts\PeasyPilot.Core.0.1.2.nupkg
  PeasyPilot.BDD -> G:\MCS\Github\apps\PeasyPilot\artifacts\PeasyPilot.BDD.0.1.2.nupkg
  ... (all packages)
Modification détectée. Relance du build et du package...
```

---

## Build Variants

### Debug Build (Fast, not optimized)

```bash
dotnet build
dotnet pack -c Debug -o artifacts
```

### Release Build (Optimized, recommended)

```bash
dotnet build -c Release
dotnet pack -c Release -o artifacts
```

### With Code Analysis

```bash
dotnet build -c Release /p:EnableAnalyzers=true
dotnet pack -c Release -o artifacts
```

### Multi-targeting Verification

```bash
# Verify all frameworks compile
dotnet build -c Release

# Check which frameworks are included in nupkg
# (Look at .nupkg contents)
```

---

## Package Contents

### Each NuGet package includes

```
PeasyPilot.BDD.0.1.2.nupkg/
├── lib/
│   ├── net8.0/
│   │   ├── PeasyPilot.BDD.dll
│   │   ├── PeasyPilot.BDD.xml (docs)
│   │   └── PeasyPilot.Core.dll (dependencies)
│   ├── net9.0/
│   │   └── ... (same structure)
│   └── net10.0/
│       └── ... (same structure)
├── PeasyPilot.BDD.nuspec (metadata)
└── package.json (NuGet metadata)
```

### Version in package

Version comes from `.csproj`:
```xml
<PropertyGroup>
    <Version>0.1.2</Version>
    <PackageId>PeasyPilot.BDD</PackageId>
</PropertyGroup>
```

---

## Publishing Packages

### Publish to NuGet.org

```bash
# Set your NuGet API key (get from nuget.org)
nuget setApiKey xxxxxxxxxxxxxxxx

# Push package
dotnet nuget push artifacts/PeasyPilot.BDD.0.1.2.nupkg -s https://api.nuget.org/v3/index.json

# Or use nuget CLI
nuget push artifacts/*.nupkg -Source https://api.nuget.org/v3/index.json
```

### Publish to private feed

```bash
# MyGet example
dotnet nuget push artifacts/*.nupkg -s https://www.myget.org/F/yourfeed/api/v2/package
```

### Verify published package

```bash
# Search on NuGet.org
https://www.nuget.org/packages/PeasyPilot.BDD/

# Or via CLI
dotnet package search PeasyPilot.BDD
```

---

## Troubleshooting

### Build fails

```bash
# Clean all build artifacts
dotnet clean

# Restore dependencies
dotnet restore

# Try build again
dotnet build -c Release
```

### Pack fails with version error

```bash
# Check version in all .csproj files
grep -r "<Version>" src/

# Ensure all versions match
# Update in affected .csproj files
# Then rebuild
```

### Artifacts directory not created

```bash
# Create manually
mkdir artifacts

# Ensure path is writable
# Re-run pack
dotnet pack -c Release -o artifacts
```

### NuGet package missing dependencies

```bash
# Verify .csproj has correct dependencies
cat src/PeasyPilot.BDD/PeasyPilot.BDD.csproj | grep -A5 "ItemGroup"

# Ensure ProjectReference entries are correct
# Rebuild and repack
```

---

## CI/CD Integration

### GitHub Actions

```yaml
name: build

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version-file: global.json
      
      - run: dotnet build -c Release
      
      - run: dotnet pack -c Release -o artifacts
      
      - uses: actions/upload-artifact@v4
        with:
          name: packages
          path: artifacts/*.nupkg
```

### Local CI Test

```bash
# Simulate CI environment
dotnet clean
dotnet restore
dotnet build -c Release
dotnet test -c Release --no-build
dotnet pack -c Release -o artifacts
```

---

## Best Practices

### ✅ DO

- ✅ Use Release build for packages
- ✅ Test locally before publishing
- ✅ Use semantic versioning (MAJOR.MINOR.PATCH)
- ✅ Update version in all .csproj files consistently
- ✅ Include XML documentation in packages
- ✅ Run tests before packing
- ✅ Verify package contents after creation

### ❌ DON'T

- ❌ Pack Debug builds for distribution
- ❌ Publish without testing
- ❌ Publish same version twice
- ❌ Leave out dependencies
- ❌ Include test projects in packages
- ❌ Ship without documentation

---

## Scripting

### PowerShell (Windows)

**File:** `scripts/version-watch.ps1`

```powershell
# Watches entire solution for changes
# Auto-rebuilds and auto-packs on detection
# Press Ctrl+C to stop

.\scripts\version-watch.ps1
```

### Bash (Linux/Mac)

**File:** `scripts/version-watch.sh`

```bash
# Same functionality as PowerShell version
# Auto-watch and auto-pack on changes

./scripts/version-watch.sh
```

---

## Versioning Strategy

Current version: **0.1.2**

```
0 = Major (breaking changes)
1 = Minor (new features, backwards compatible)
2 = Patch (bug fixes)
```

To update version:
1. Edit all `.csproj` files
2. Update `Version` tag
3. Rebuild and repack
4. Tag release in git

Example:
```xml
<!-- src/PeasyPilot.BDD/PeasyPilot.BDD.csproj -->
<PropertyGroup>
    <Version>0.2.0</Version>  <!-- Updated from 0.1.2 -->
    <AssemblyVersion>0.2.0</AssemblyVersion>
</PropertyGroup>
```

---

## Summary

### Quick reference

```bash
# Build everything
dotnet build -c Release

# Pack everything
dotnet pack -c Release -o artifacts

# Watch mode (auto-build + pack)
.\scripts\version-watch.ps1

# Publish to NuGet
dotnet nuget push artifacts/*.nupkg -s https://api.nuget.org/v3/index.json
```

### Important files

- Solution: `easy-peasy.slnx`
- Projects: `src/**/*.csproj`
- Watcher: `scripts/version-watch.ps1` (Windows) / `version-watch.sh` (Unix)
- Artifacts: `artifacts/` (generated)

---

## Getting Help

- Review project files: `src/**/*.csproj`
- Check NuGet docs: https://docs.microsoft.com/nuget/
- Test locally before publishing
- Open an issue if problems occur
