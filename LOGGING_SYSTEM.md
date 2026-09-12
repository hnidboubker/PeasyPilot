# PeasyPilot Advanced Test Logging System

## Overview

TestLoggerBase provides a centralized, sophisticated logging infrastructure for all PeasyPilot tests. It automatically tracks test execution, errors, exceptions, and crashes, making it easy to identify and debug issues in CI/CD environments like GitHub Actions.

## Features

### ✅ Automatic Logging
- **Test Lifecycle**: Logs test initialization with timestamp and method name
- **Duration Tracking**: Measures test execution time in milliseconds
- **Console Output**: All logs are visible in GitHub Actions and CI/CD
- **Structured Format**: `[HH:mm:ss.fff] [LEVEL] [Category] Message`

### 🔍 Error Tracking
- **Error Capture**: Automatically tracks all errors during test execution
- **Exception Handling**: Logs full stack traces for exceptions
- **Assertion Failures**: Records assertion failures with expected vs. actual values
- **Error Report**: Generates comprehensive error summary at test completion

### 📊 Test Metrics
- **Execution Time**: Measures how long each test takes
- **Error Count**: Tracks number of errors per test
- **Timestamp Recording**: Records exact time each error occurred

### 🎯 Usage Methods

#### Basic Logging
```csharp
// Log information
Logger.LogInformation("Starting data processing");

// Log warnings
Logger.LogWarning("Performance threshold exceeded");

// Log errors (custom)
Logger.LogError("Critical issue detected");
```

#### Log Assertion Failures
```csharp
// Log with expected vs actual comparison
LogAssertionFailure("Values do not match", expected: 10, actual: 5);

// Simple assertion logging
LogAssertionFailure("Check failed");
```

#### Log Exceptions
```csharp
try
{
    // risky operation
    throw new InvalidOperationException("Something went wrong");
}
catch (Exception ex)
{
    LogException(ex, "Operation failed");
    // Error is automatically tracked with full stack trace
}
```

#### Log Warnings (Non-Critical Issues)
```csharp
LogWarning("Database query took longer than expected");
LogWarning("Memory usage is high");
```

#### Get Error Report
```csharp
// Get formatted error report as string
var errorReport = GetErrorReport();
Logger.LogInformation(errorReport);

// Or check error collection
if (Errors.Count > 0)
{
    foreach (var error in Errors)
    {
        Logger.LogError($"{error.ErrorType}: {error.Message}");
    }
}
```

## Example Test

```csharp
public class AdvancedLoggingTests : PeasyPilotTestBase
{
    [Fact]
    public void ProcessData_WithValidInput_Succeeds()
    {
        // Arrange
        Logger.LogInformation("Loading test data");
        var data = GetTestData("important-file", () => new { value = 42 });

        // Act
        Logger.LogInformation("Processing data");
        var result = ProcessData(data);
        Logger.LogInformation($"Result: {result}");

        // Assert - with assertion logging
        if (result != 42)
        {
            LogAssertionFailure("Result validation", expected: 42, actual: result);
        }
        Assert.Equal(42, result);
        Logger.LogInformation("✅ Test passed");
    }

    [Fact]
    public void ComplexOperation_WithErrorHandling_TracksIssues()
    {
        Logger.LogInformation("Starting complex operation");

        try
        {
            Logger.LogInformation("Step 1: Initialize");
            var step1 = InitializeStep();

            Logger.LogInformation("Step 2: Process");
            var step2 = ProcessStep(step1);

            Logger.LogInformation("Step 3: Validate");
            if (step2 == null)
            {
                LogWarning("Step 2 returned null");
            }

            Assert.NotNull(step2);
        }
        catch (Exception ex)
        {
            LogException(ex, "Complex operation failed");
            throw; // Re-throw after logging
        }
    }
}
```

## GitHub Actions Output Example

```
═══════════════════════════════════════════════════════════════
09:15:42.123 [info] Test initialized: ProcessDataTests
09:15:42.124 [info] Test method: ProcessData_WithValidInput_Succeeds
09:15:42.124 [info] Time: 2024-01-15 09:15:42.123
═══════════════════════════════════════════════════════════════
09:15:42.125 [info] Loading test data
09:15:42.126 [debug] Getting or creating test data: user-fixture
09:15:42.127 [info] Processing data
09:15:42.128 [info] Result: 42
09:15:42.129 [info] ✅ Test passed
─────────────────────────────────────────────────────────────────
09:15:42.130 [info] Test completed: ProcessDataTests
09:15:42.130 [info] Duration: 7ms
✅ TEST PASSED: No errors detected
═══════════════════════════════════════════════════════════════
```

## Error Report Output Example

```
─────────────────────────────────────────────────────────────────
❌ TEST FAILED: 2 error(s) found

📋 ERROR REPORT:
─────────────────────────────────────────────────────────────────
1. [AssertionFailure] Assertion failed: Values do not match (Expected: 10, Actual: 5)
   Stack: at PeasyPilot.Tests.ComplexTests.TestMethod() in ComplexTests.cs:line 45
   Time: 09:15:43.234

2. [InvalidOperationException] Operation failed
   Stack: System.InvalidOperationException: Test exception...
   Time: 09:15:43.456

═══════════════════════════════════════════════════════════════
```

## Integration with All Frameworks

The logging system works seamlessly with:
- ✅ **xUnit**: Via `IAsyncLifetime` interface
- ✅ **NUnit**: Via `Setup`/`TearDown` attributes
- ✅ **TUnit**: Via `BeforeEachAsync`/`AfterEachAsync`

## Error Types

The system tracks multiple error types:

| Error Type | When Logged | Severity |
|-----------|-----------|----------|
| **AssertionFailure** | Via `LogAssertionFailure()` | Medium |
| **Exception Name** | Via `LogException()` | High |
| **Warning** | Via `LogWarning()` | Low |

## Best Practices

1. **Log every significant step** in your test for traceability
2. **Use LogAssertionFailure()** for validation point failures
3. **Catch and log exceptions** with `LogException()` for debugging
4. **Use LogWarning()** for potential issues that don't fail the test
5. **Check Errors collection** before assertions for detailed error info

## CI/CD Integration

All logs are automatically output to:
- ✅ Console (visible in GitHub Actions output)
- ✅ All test frameworks' logging systems
- ✅ CI/CD pipeline logs

No additional configuration needed—just use `Logger` in your tests!

## Performance

- **Minimal Overhead**: Stopwatch tracking adds <1ms per test
- **Efficient Logging**: Console output is buffered
- **No External Dependencies**: Uses only Microsoft.Extensions.Logging

## Troubleshooting

### "Logger not initialized" Exception
**Solution**: Call `await base.InitializeAsync()` in your test's `InitializeAsync()` method

### Logs not appearing in CI
**Solution**: Logs are written to console; check CI output verbosity level

### Missing stack traces
**Solution**: Use `LogException()` which automatically captures full stack trace

---

For examples, see `samples/PeasyPilot.XUnit.Samples/Tests/LoggingExampleTests.cs`
