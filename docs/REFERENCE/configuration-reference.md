# PeasyPilot Configuration Reference

Complete reference for all PeasyPilot configuration options, settings, and precedence rules. Target frameworks: .NET 8, 9, and 10.

---

## Quick Start

PeasyPilot configuration is managed through multiple sources with a clear precedence order:

```csharp
// Example: Configure test options programmatically
var testOptions = new TestOptions
{
    Environment = "Integration",
    EnableLogging = true
};

// Example: Configure pipeline options
var pipelineOptions = new TestPipelineOptions
{
    Filter = new NameTestFilter("UserService"),
    RunDiagnosticsOnFailure = true,
    Reporters = new[] { new ConsoleReporter(), new JsonFileReporter("./results.json") }
};
```

---

## Configuration Sources

PeasyPilot recognizes configuration from the following sources (in order of precedence):

1. **Programmatic Configuration** (highest precedence) — Direct code settings
2. **Environment Variables** — System environment variables
3. **appsettings.json** — Application settings file
4. **appsettings.{Environment}.json** — Environment-specific settings
5. **global.json** — Global SDK settings
6. **Built-in Defaults** (lowest precedence) — Hardcoded defaults

---

## Configuration Precedence

Settings are applied in this order (highest to lowest):

```
Programmatic API
    ↓
Environment Variables
    ↓
appsettings.{Environment}.json (e.g., appsettings.Test.json)
    ↓
appsettings.json
    ↓
global.json
    ↓
Built-in Defaults
```

The first defined value wins. For example, if `Environment` is set in `appsettings.json` and via a code configuration, the code configuration takes precedence.

---

## Core Settings

### TestOptions

Configuration for general test environment behavior.

#### Environment

The test environment name.

| Property | Value |
|----------|-------|
| **Type** | `string` |
| **Default** | `"Development"` |
| **Valid values** | `"Development"`, `"Testing"`, `"Integration"`, `"Staging"`, `"Production"` |
| **Configuration sources** | Code, Environment variable, appsettings.json |

Controls which settings file is loaded: if set to `"Testing"`, PeasyPilot loads `appsettings.Testing.json`.

**Programmatic configuration:**
```csharp
var options = new TestOptions { Environment = "Integration" };
```

**Environment variable:**
```bash
# PowerShell
$env:PeasyPilot__Environment = "Testing"

# Bash
export PeasyPilot__Environment="Testing"
```

**appsettings.json:**
```json
{
  "PeasyPilot": {
    "Environment": "Testing"
  }
}
```

**Recommended environments:**
- **Development** — Local development, debugging enabled, full output
- **Testing** — CI/CD pipelines, minimal logging, structured output
- **Integration** — Integration test suites, database fixture support
- **Staging** — Pre-production validation, performance tracking

#### EnableLogging

Enable or disable test logging output.

| Property | Value |
|----------|-------|
| **Type** | `bool` |
| **Default** | `true` |
| **Configuration sources** | Code, Environment variable, appsettings.json |

When enabled, test execution details are logged to console and log file. Useful for debugging test failures.

**Programmatic configuration:**
```csharp
var options = new TestOptions { EnableLogging = true };
```

**Environment variable:**
```bash
# PowerShell
$env:PeasyPilot__EnableLogging = "true"

# Bash
export PeasyPilot__EnableLogging="true"
```

**appsettings.json:**
```json
{
  "PeasyPilot": {
    "EnableLogging": true
  }
}
```

### TestPipelineOptions

Configuration for test pipeline execution behavior.

#### ChangedFiles

Comma-separated list of changed files for test impact analysis.

| Property | Value |
|----------|-------|
| **Type** | `IReadOnlyCollection<string>` |
| **Default** | `null` |
| **Configuration sources** | Code, CLI argument |

When set, PeasyPilot performs impact analysis to determine which tests are affected by the changed files, running only those tests.

**Programmatic configuration:**
```csharp
var options = new TestPipelineOptions
{
    ChangedFiles = new[] { "src/User.cs", "src/UserRepository.cs" }
};
```

**CLI argument:**
```bash
peasypilot --changed-files "src/User.cs,src/Order.cs"
```

#### Filter

Test filter to select which tests to run.

| Property | Value |
|----------|-------|
| **Type** | `ITestFilter` |
| **Default** | `null` (no filtering) |
| **Implementation** | `NameTestFilter` (substring match) |
| **Configuration sources** | Code, CLI argument |

Filters tests by name using case-insensitive substring matching.

**Programmatic configuration:**
```csharp
var filter = new NameTestFilter("UserService");
var options = new TestPipelineOptions { Filter = filter };
```

**CLI argument:**
```bash
peasypilot --filter "UserService"
```

#### RunDiagnosticsOnFailure

Enable automatic diagnostics when tests fail.

| Property | Value |
|----------|-------|
| **Type** | `bool` |
| **Default** | `true` |
| **Configuration sources** | Code |

When true, detailed diagnostic information (stack traces, log output, performance metrics) is collected when tests fail.

**Programmatic configuration:**
```csharp
var options = new TestPipelineOptions { RunDiagnosticsOnFailure = true };
```

#### Reporters

Collection of test reporters to generate output.

| Property | Value |
|----------|-------|
| **Type** | `IReadOnlyCollection<ITestReporter>` |
| **Default** | `[]` (empty) |
| **Configuration sources** | Code |

**Available reporters:**
- `ConsoleReporter` — Print results to console
- `JsonFileReporter` — JSON file output
- `JUnitXmlReporter` — JUnit XML format (CI/CD integration)
- `HtmlFileReporter` — Interactive HTML report
- `CiAnnotationReporter` — GitHub Actions/Azure Pipelines annotations

**Programmatic configuration:**
```csharp
var reporters = new List<ITestReporter>
{
    new ConsoleReporter(),
    new JsonFileReporter("./results.json"),
    new JUnitXmlReporter("./junit.xml")
};

var options = new TestPipelineOptions { Reporters = reporters };
```

#### Diagnostics

Collection of diagnostic providers for test failure analysis.

| Property | Value |
|----------|-------|
| **Type** | `IReadOnlyCollection<ITestDiagnostic>` |
| **Default** | `[]` (empty) |
| **Configuration sources** | Code |

Diagnostic providers analyze test failures and suggest root causes.

**Programmatic configuration:**
```csharp
var diagnostics = new List<ITestDiagnostic>
{
    new DefaultDiagnostic(),
    new PerformanceTracker()
};

var options = new TestPipelineOptions { Diagnostics = diagnostics };
```

---

## Configuration Files

### appsettings.json

Main application configuration file for test settings.

**Location:** `./appsettings.json` (root of project)

**Format:** JSON

**Example appsettings.json:**
```json
{
  "PeasyPilot": {
    "Environment": "Development",
    "EnableLogging": true,
    "TestDatabase": {
      "Engine": "InMemory",
      "ResetBetweenTests": true
    },
    "Discovery": {
      "AssemblyPattern": "**.Tests.dll",
      "IncludeFrameworks": [ "xunit", "nunit", "tunit" ]
    },
    "Reporting": {
      "DefaultFormat": "console",
      "ConsoleVerbosity": "detailed"
    }
  }
}
```

### appsettings.{Environment}.json

Environment-specific configuration, loaded after `appsettings.json`. Settings here override `appsettings.json`.

**Examples:**
- `appsettings.Development.json` — Local development settings
- `appsettings.Testing.json` — CI/CD pipeline settings
- `appsettings.Production.json` — Production/staging settings

**Example appsettings.Testing.json:**
```json
{
  "PeasyPilot": {
    "Environment": "Testing",
    "EnableLogging": false,
    "TestDatabase": {
      "Engine": "InMemory",
      "ResetBetweenTests": true
    },
    "Reporting": {
      "DefaultFormat": "json",
      "OutputPath": "./build/test-results.json"
    }
  }
}
```

When running in Testing environment:
```bash
# Set environment before running
$env:ASPNETCORE_ENVIRONMENT = "Testing"
dotnet test

# Or specify in appsettings.json
```

### global.json

.NET SDK configuration file (shared with entire solution).

**Location:** `./ global.json` (repository root)

**Format:** JSON

**Example global.json:**
```json
{
  "sdk": {
    "version": "10.0"
  }
}
```

This file is automatically detected by .NET tooling and does not contain PeasyPilot-specific settings, but determines which .NET SDK version is used for compilation and test execution.

---

## Environment Variables

PeasyPilot recognizes environment variables with the prefix `PeasyPilot__` (double underscore).

### Setting Environment Variables

**PowerShell:**
```powershell
$env:PeasyPilot__Environment = "Testing"
$env:PeasyPilot__EnableLogging = "true"
```

**Bash:**
```bash
export PeasyPilot__Environment="Testing"
export PeasyPilot__EnableLogging="true"
```

**Windows Command Prompt:**
```cmd
set PeasyPilot__Environment=Testing
set PeasyPilot__EnableLogging=true
```

**In CI/CD pipelines (GitHub Actions):**
```yaml
env:
  PeasyPilot__Environment: Testing
  PeasyPilot__EnableLogging: "false"
```

### Supported Environment Variables

| Variable | Type | Default | Example |
|----------|------|---------|---------|
| `PeasyPilot__Environment` | string | `"Development"` | `Testing` |
| `PeasyPilot__EnableLogging` | bool | `true` | `false` |
| `PeasyPilot__LogLevel` | string | `"Information"` | `"Debug"` |
| `PeasyPilot__TestTimeout` | int | `30000` | `60000` |

---

## Configuration Examples

### Development Environment

Local development with full logging and console output:

**appsettings.Development.json:**
```json
{
  "PeasyPilot": {
    "Environment": "Development",
    "EnableLogging": true,
    "Logging": {
      "LogLevel": "Debug"
    },
    "Reporting": {
      "ConsoleVerbosity": "detailed"
    }
  }
}
```

**Run:**
```bash
dotnet test
```

### Testing Environment (CI/CD)

Minimal logging, structured output for CI/CD systems:

**appsettings.Testing.json:**
```json
{
  "PeasyPilot": {
    "Environment": "Testing",
    "EnableLogging": false,
    "TestDatabase": {
      "Engine": "InMemory"
    },
    "Reporting": {
      "DefaultFormat": "json",
      "OutputPath": "./build/test-results.json"
    }
  }
}
```

**Run:**
```bash
$env:ASPNETCORE_ENVIRONMENT = "Testing"
dotnet test
```

### Integration Testing

Database fixture support and integration-specific settings:

**appsettings.Integration.json:**
```json
{
  "PeasyPilot": {
    "Environment": "Integration",
    "EnableLogging": true,
    "TestDatabase": {
      "Engine": "SqlServer",
      "ConnectionString": "Server=(local);Database=PeasyPilot_Tests;Integrated Security=true;",
      "ResetBetweenTests": true
    },
    "Discovery": {
      "IncludeIntegrationTests": true
    }
  }
}
```

**Run:**
```bash
$env:ASPNETCORE_ENVIRONMENT = "Integration"
dotnet test
```

### Production/Staging Validation

Performance tracking and comprehensive reporting:

**appsettings.Staging.json:**
```json
{
  "PeasyPilot": {
    "Environment": "Staging",
    "EnableLogging": true,
    "Reporting": {
      "DefaultFormat": "html",
      "OutputPath": "./reports/test-report.html"
    },
    "Performance": {
      "EnableTracking": true,
      "ThresholdMs": 5000
    }
  }
}
```

**Run:**
```bash
$env:ASPNETCORE_ENVIRONMENT = "Staging"
dotnet test
```

---

## Configuration Priority Examples

### Example 1: Environment Override

**Scenario:** `appsettings.json` sets `EnableLogging: true`, but you want to disable it for this run.

```bash
# Set environment variable (takes precedence over appsettings.json)
$env:PeasyPilot__EnableLogging = "false"
dotnet test

# Logging is now disabled, even though appsettings.json says true
```

### Example 2: Programmatic Configuration

**Scenario:** Code configuration overrides all file-based settings.

```csharp
// Programmatic configuration (highest precedence)
var options = new TestOptions { Environment = "Integration" };

// This takes precedence over:
// - appsettings.Integration.json
// - Environment variable
// - appsettings.json
// - global.json
// - defaults
```

### Example 3: Environment-Specific Files

**Scenario:** Development vs. Testing environments use different settings.

**appsettings.json:**
```json
{
  "PeasyPilot": {
    "Environment": "Development",
    "EnableLogging": true
  }
}
```

**appsettings.Testing.json:**
```json
{
  "PeasyPilot": {
    "Environment": "Testing",
    "EnableLogging": false
  }
}
```

**Run locally:**
```bash
# Uses appsettings.json (Development)
dotnet test
```

**Run in CI/CD:**
```bash
# Set environment to Testing, which loads appsettings.Testing.json
$env:ASPNETCORE_ENVIRONMENT = "Testing"
dotnet test
```

---

## Validation Rules

### Environment

- **Allowed values:** `"Development"`, `"Testing"`, `"Integration"`, `"Staging"`, `"Production"`
- **Case-insensitive:** `"development"` = `"Development"`
- **Invalid values:** Default to `"Development"` with warning

### EnableLogging

- **Allowed values:** `true`, `false`
- **Case-insensitive strings:** `"true"`, `"false"`, `"True"`, `"False"`
- **Invalid values:** Default to `true` with warning

### TestPipelineOptions

- **Filter:** Must be a valid `ITestFilter` implementation
- **Reporters:** Must implement `ITestReporter` interface
- **Diagnostics:** Must implement `ITestDiagnostic` interface
- **ChangedFiles:** Must be valid file paths (relative or absolute)

---

## Troubleshooting

### Configuration Not Being Applied

**Symptom:** Set a value in `appsettings.json` but tests still use the default.

**Diagnostic:**
1. Verify the correct environment is set
2. Check file format (valid JSON)
3. Check precedence — programmatic code overrides files

**Solution:**
```bash
# Check current environment
echo $env:ASPNETCORE_ENVIRONMENT

# Verify appsettings file exists and is valid JSON
cat ./appsettings.json
```

### "Unable to Load appsettings.json"

**Symptom:** CLI reports configuration file not found.

**Solution:**
1. Verify `appsettings.json` exists in project root
2. Ensure the file path is correct
3. Check file permissions (must be readable)

```bash
# Verify file exists
ls ./appsettings.json

# Or on Windows
dir appsettings.json
```

### Environment-Specific File Not Loaded

**Symptom:** Set `ASPNETCORE_ENVIRONMENT=Testing` but `appsettings.Testing.json` is not loaded.

**Solution:**
1. Verify file exists: `appsettings.Testing.json`
2. Ensure environment variable is set correctly before running tests
3. Restart your terminal/IDE after setting environment variables

```bash
# Verify the environment variable is set
echo $env:ASPNETCORE_ENVIRONMENT

# Try with explicit environment
dotnet test --configuration Release
```

---

## Best Practices

1. **Use appsettings.json for defaults** — Set sensible defaults in `appsettings.json`
2. **Use appsettings.{Environment}.json for overrides** — Override defaults per environment
3. **Use environment variables for CI/CD** — Set variables in CI/CD pipeline configuration
4. **Use code configuration sparingly** — Only for dynamic, runtime-determined settings
5. **Keep logging on in Development** — Helps debug test failures locally
6. **Keep logging off in Testing (CI/CD)** — Reduces noise and speeds up pipelines
7. **Use structured formats (JSON)** — Easier to parse and analyze in CI/CD

---

## Related Documentation

- [CLI Reference](./cli-reference.md) — Command-line interface reference
- [Getting Started Guide](../GETTING-STARTED.md) — Configuration in practice
- [Troubleshooting Quick Reference](./troubleshooting-quick-ref.md) — Common configuration issues

---

**Last updated:** 2026-09-11  
**Framework support:** .NET 8, 9, 10
