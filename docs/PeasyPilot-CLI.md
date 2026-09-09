# PeasyPilot.CLI

Command-line test runner with filtering and scheduling.

```bash
dotnet pesypilot run --filter "*.Tests" --schedule daily
dotnet pesypilot run --impact-analysis
```

## Features
- ✅ Test filtering by name/category
- ✅ Scheduling and recurrence
- ✅ Impact analysis
- ✅ JSON/JUnit reporting
- ✅ CI/CD integration

## Usage

```bash
# Run all tests
dotnet pesypilot run

# Filter tests
dotnet pesypilot run --filter "*UserTests"

# Schedule tests
dotnet pesypilot run --schedule "0 2 * * *"

# Impact analysis
dotnet pesypilot run --impact-analysis
```

## Install
```bash
dotnet tool install PeasyPilot.CLI
```
