# PeasyPilot CLI Reference

Complete reference for all PeasyPilot command-line interface (CLI) commands, options, and examples. Target frameworks: .NET 8, 9, and 10.

---

## Quick Start

The PeasyPilot CLI is invoked via the `peasypilot` command:

```bash
# Show help
peasypilot --help

# Run all tests
peasypilot

# Run tests with filtering
peasypilot --filter "UserService"

# Run with impact analysis (changed files)
peasypilot --changed-files "src/User.cs,src/Order.cs"

# Generate test suggestions
peasypilot suggest-tests --assembly ./bin/Release/net9.0/MyApp.dll --type MyApp.UserService

# Show test execution history
peasypilot history
```

---

## Commands Overview

PeasyPilot provides four primary commands:

1. **Test Execution** — Run tests with filtering and impact analysis
2. **History** — View previous test run records
3. **Test Generation** — Suggest and generate test suites
4. **Help** — Display usage information

---

## Command: Test Execution

Run tests from the current solution with filtering, scheduling, and reporting options.

### Syntax

```bash
peasypilot [options]
```

### Options

#### `--filter <name>` | `-f <name>`

Filter tests by name (case-insensitive substring match).

| Property | Value |
|----------|-------|
| **Type** | `string` |
| **Required** | No |
| **Default** | `null` (no filtering) |
| **Example** | `--filter "UserService"` |

Matches any test containing the substring "UserService" in its fully qualified name.

```bash
# Run only tests with "UserService" in the name
peasypilot --filter "UserService"

# Run tests matching multiple keywords (case-insensitive)
peasypilot -f "Repository"
```

#### `--changed-files <files>` | `-c <files>`

Comma-separated list of changed file paths for test impact analysis. When provided, PeasyPilot performs impact analysis to identify only tests affected by the changed files.

| Property | Value |
|----------|-------|
| **Type** | `string` (comma-separated paths) |
| **Required** | No |
| **Default** | `null` (all tests scheduled) |
| **Example** | `--changed-files "src/User.cs,src/Order.cs"` |

File paths can be:
- Relative paths: `src/User.cs`
- Absolute paths: `/home/user/project/src/User.cs`
- Wildcards: `src/*.cs` (processed as literal paths)

```bash
# Run only tests affected by changes to User.cs and Order.cs
peasypilot --changed-files "src/User.cs,src/Order.cs"

# Impact analysis on Windows
peasypilot -c "src\User.cs,src\Order.cs"

# Multiple file changes
peasypilot -c "src/User.cs,src/UserRepository.cs,tests/UserTests.cs"
```

#### `--format <format>` | `-fmt <format>`

Output format for test results reporting.

| Property | Value |
|----------|-------|
| **Type** | `string` (enum: `console`, `json`, `junit`) |
| **Required** | No |
| **Default** | `console` |
| **Valid values** | `console`, `json`, `junit` |

Supported formats:

- **`console`** — Human-readable console output with colors and summary
- **`json`** — Machine-readable JSON format for programmatic consumption
- **`junit`** — JUnit XML format for CI/CD integration (Jenkins, Azure DevOps, GitHub Actions)

```bash
# Console output (default)
peasypilot --format console

# JSON output for parsing
peasypilot --format json

# JUnit XML for CI/CD
peasypilot --format junit
```

#### `--output <path>` | `-o <path>`

File path to save test report. Format is auto-detected from file extension or `--format` option.

| Property | Value |
|----------|-------|
| **Type** | `string` (file path) |
| **Required** | No |
| **Default** | `null` (console output only) |
| **Example** | `--output "results.json"` |

The output path can include:
- Relative paths: `./results/report.json`
- Absolute paths: `/var/log/test-results.xml`
- Directories are created if they do not exist

If both `--output` and `--format` are specified, the format must be compatible with the file extension. If the file extension conflicts with the format, the file extension takes precedence.

```bash
# Save JSON report
peasypilot --format json --output "./reports/test-results.json"

# Save JUnit XML (auto-detected from extension)
peasypilot --output "./results/junit.xml"

# Multiple reporters (console + file)
peasypilot --format json -o "./results/results.json"
```

### Global Options

#### `--help` | `-h` | `help`

Display help information for the CLI.

```bash
peasypilot --help
peasypilot -h
peasypilot help
```

### Exit Codes

| Code | Meaning |
|------|---------|
| **0** | All tests passed successfully |
| **1** | One or more tests failed, or an error occurred |

### Examples

#### Example 1: Basic Test Run

```bash
peasypilot
```

Discovers and runs all tests in the solution. Output is printed to console.

**Expected output:**
```
[PeasyPilot CLI] Executing Test Pipeline...
[PeasyPilot CLI] Status: Passed | Discovered: 42 | Scheduled: 42 | Passed: 42 | Failed: 0
```

#### Example 2: Filtered Test Run

```bash
peasypilot --filter "UserService"
```

Runs only tests with "UserService" in their name.

**Expected output:**
```
[PeasyPilot CLI] Executing Test Pipeline...
[PeasyPilot CLI] Status: Passed | Discovered: 42 | Scheduled: 8 | Passed: 8 | Failed: 0
```

#### Example 3: Impact Analysis

```bash
peasypilot --changed-files "src/UserService.cs,src/UserRepository.cs"
```

Analyzes which tests depend on the changed files and runs only those tests.

**Expected output:**
```
[PeasyPilot CLI] Executing Test Pipeline...
Impact Analysis: 15 of 42 tests affected by changes
[PeasyPilot CLI] Status: Passed | Discovered: 42 | Scheduled: 15 | Passed: 15 | Failed: 0
```

#### Example 4: JSON Report

```bash
peasypilot --format json --output "./results/test-report.json"
```

Runs all tests and saves results to `./results/test-report.json` in JSON format.

**JSON schema:**
```json
{
  "status": "Passed",
  "discoveredCount": 42,
  "scheduledCount": 42,
  "aggregateRunResult": {
    "passed": 42,
    "failed": 0,
    "skipped": 0,
    "duration": "00:00:05.123"
  }
}
```

#### Example 5: JUnit XML for CI/CD

```bash
peasypilot --output "./build/test-results.xml"
```

Runs tests and generates JUnit XML format (auto-detected from `.xml` extension) for GitHub Actions, Azure Pipelines, or Jenkins.

#### Example 6: Combined Filtering and Reporting

```bash
peasypilot --filter "Repository" --format json -o "./reports/repo-tests.json"
```

Runs tests matching "Repository", generates JSON report, and saves to `./reports/repo-tests.json`.

---

## Command: Test History

View previously recorded test execution runs.

### Syntax

```bash
peasypilot history
```

### Options

None. The `history` command shows the 10 most recent test runs by default.

### Output Format

Console output displays:
- **Execution timestamp** — Date and time of test run
- **Run ID** — Unique identifier for the run
- **Status** — Overall result (Passed, Failed, Skipped)
- **Discovered** — Number of tests discovered
- **Passed/Failed** — Test results

### Examples

```bash
peasypilot history
```

**Output:**
```
[PeasyPilot CLI] Test Execution History:
[2026-09-11 14:23:45] Run ID: 550e8400-e29b-41d4-a716-446655440000 | Status: Passed | Discovered: 42 | Passed: 42 | Failed: 0
[2026-09-11 10:15:32] Run ID: 550e8400-e29b-41d4-a716-446655440001 | Status: Failed | Discovered: 42 | Passed: 40 | Failed: 2
[2026-09-11 08:45:12] Run ID: 550e8400-e29b-41d4-a716-446655440002 | Status: Passed | Discovered: 42 | Passed: 42 | Failed: 0
```

---

## Command: Test Generation (suggest-tests)

Generate test suite proposals for a given type using AI-assisted analysis and code generation.

### Syntax

```bash
peasypilot suggest-tests --assembly <path> --type <name> [options]
```

### Required Options

#### `--assembly <path>` | `-a <path>`

Path to the compiled assembly (.dll) to analyze.

| Property | Value |
|----------|-------|
| **Type** | `string` (file path) |
| **Required** | **Yes** |
| **Example** | `./bin/Release/net9.0/MyApp.dll` |

Must be a valid .NET assembly file. Path can be:
- Relative: `./bin/Release/net9.0/MyApp.dll`
- Absolute: `/home/user/project/bin/Release/net9.0/MyApp.dll`

#### `--type <name>` | `-t <name>`

Target type name to generate tests for.

| Property | Value |
|----------|-------|
| **Type** | `string` (fully qualified type name or simple name) |
| **Required** | **Yes** |
| **Example** | `MyApp.Services.UserService` or `UserService` |

Can be either:
- **Fully qualified name** (preferred): `MyApp.Services.UserService`
- **Simple name**: `UserService` (matched against first occurrence)

### Optional Options

#### `--framework <fw>` | `-fw <fw>`

Target test framework for code generation.

| Property | Value |
|----------|-------|
| **Type** | `string` (enum: `xunit`, `nunit`, `tunit`) |
| **Required** | No |
| **Default** | `xunit` |
| **Valid values** | `xunit`, `nunit`, `tunit` |

```bash
# Generate xUnit tests (default)
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService

# Generate NUnit tests
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -fw nunit

# Generate TUnit tests
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -fw tunit
```

#### `--output-dir <dir>` | `-o <dir>`

Output directory for generated test files.

| Property | Value |
|----------|-------|
| **Type** | `string` (directory path) |
| **Required** | No |
| **Default** | `./generated-tests` |
| **Example** | `./tests/generated` |

Directory is created if it does not exist.

```bash
# Save to custom directory
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -o ./tests/generated
```

#### `--format <fmt>` | `-fmt <fmt>`

Output file format for test generation.

| Property | Value |
|----------|-------|
| **Type** | `string` (enum: `json`, `cs`, `both`) |
| **Required** | No |
| **Default** | `json` |
| **Valid values** | `json`, `cs`, `both` |

- **`json`** — JSON test plan file (analysis + recommendations)
- **`cs`** — C# source code file (ready to integrate)
- **`both`** — Both JSON and C# files

```bash
# Generate JSON analysis
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -fmt json

# Generate C# code
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -fmt cs

# Generate both
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -fmt both
```

#### `--max-enum-cases <count>` | `-m <count>`

Maximum number of enum values to test.

| Property | Value |
|----------|-------|
| **Type** | `int` |
| **Required** | No |
| **Default** | `8` |
| **Valid range** | `1–100` |
| **Example** | `-m 16` |

When analyzing enum parameters, this limits the number of test cases generated. Useful for large enums to avoid test explosion.

```bash
# Test up to 16 enum values
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService -m 16
```

#### `--force`

Overwrite existing proposal files without prompting.

| Property | Value |
|----------|-------|
| **Type** | `bool` (flag) |
| **Required** | No |
| **Default** | `false` |

By default, if output files already exist, the command fails. Use `--force` to overwrite.

```bash
# Overwrite existing files
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService --force
```

### Exit Codes

| Code | Meaning |
|------|---------|
| **0** | Test proposal generated successfully |
| **1** | Error: assembly not found, type not found, or permission denied |

### Examples

#### Example 1: Generate xUnit Tests (Default)

```bash
peasypilot suggest-tests --assembly ./bin/Release/net9.0/MyApp.dll --type UserService
```

Generates test proposal for `UserService` as xUnit tests in `./generated-tests/UserServiceTests.Proposed.cs`.

#### Example 2: Generate NUnit Tests

```bash
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t OrderService -fw nunit
```

Generates NUnit test suite for `OrderService` with `[TestFixture]` and `[Test]` attributes.

#### Example 3: Generate Both JSON and C#

```bash
peasypilot suggest-tests \
  --assembly ./bin/Release/net9.0/MyApp.dll \
  --type PaymentProcessor \
  --framework tunit \
  --output-dir ./tests/generated \
  --format both
```

Outputs:
- `./tests/generated/PaymentProcessor.testbattery.json` — Test plan analysis
- `./tests/generated/PaymentProcessorTests.Proposed.cs` — TUnit-compatible C# code

#### Example 4: Large Enum Analysis

```bash
peasypilot suggest-tests \
  -a ./bin/Release/net9.0/MyApp.dll \
  -t ReportGenerator \
  -m 20
```

Tests up to 20 values of enum parameters (instead of the default 8).

---

## Configuration Precedence

CLI options override configuration file settings and environment variables.

**Precedence (highest to lowest):**
1. Command-line arguments (`--filter`, `--format`, etc.)
2. Environment variables (if supported in future versions)
3. Configuration files (global.json, appsettings.json)
4. Built-in defaults

For example:

```bash
# CLI overrides default format
peasypilot --format json
```

---

## Troubleshooting

### "Assembly not found"

```bash
peasypilot suggest-tests -a ./bin/Release/MyApp.dll -t UserService
Error: Assembly not found: ./bin/Release/MyApp.dll
```

**Solution:**
1. Verify the path is correct
2. Ensure the project is built in Release mode
3. Use absolute paths if relative paths fail

```bash
# Rebuild the project first
dotnet build --configuration Release

# Then run with correct path
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService
```

### "Type not found"

```bash
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t NonExistentClass
Error: Type 'NonExistentClass' not found in assembly.
```

**Solution:**
1. Use the fully qualified type name: `MyApp.Services.UserService`
2. Verify the type is public and exported from the assembly
3. Check the assembly contains the expected types

```bash
# Use fully qualified name
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t "MyApp.Services.UserService"
```

### "Output files already exist"

```bash
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService
Error: Output files already exist. Use --force to overwrite.
```

**Solution:** Use `--force` to overwrite:

```bash
peasypilot suggest-tests -a ./bin/Release/net9.0/MyApp.dll -t UserService --force
```

### Test Filter Not Matching

```bash
peasypilot --filter "User"
# No tests run, but expected some to match
```

**Solution:**
1. Verify the filter string matches test names (case-insensitive substring)
2. Use shorter filter strings
3. Check test discovery is working

```bash
# Run without filter to verify tests exist
peasypilot

# Then use a known substring
peasypilot --filter "UserService"
```

---

## Environment

- **Supported .NET versions:** .NET 8, 9, 10
- **Supported platforms:** Windows, Linux, macOS
- **Shell:** PowerShell, Bash, Command Prompt

---

## Related Documentation

- [Configuration Reference](./configuration-reference.md) — Configuration options and precedence
- [Getting Started Guide](../GETTING-STARTED.md) — First steps with PeasyPilot
- [Troubleshooting Quick Reference](./troubleshooting-quick-ref.md) — Common issues and solutions

---

**Last updated:** 2026-09-11  
**Framework support:** .NET 8, 9, 10
