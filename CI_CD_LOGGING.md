# CI/CD Test Logging Integration

## Overview

The PeasyPilot test logging system is fully integrated with GitHub Actions CI/CD. Even when tests fail, comprehensive error reports are automatically generated and visible in the CI/CD pipeline.

## How It Works

### 1. During Test Execution
```
Test runs → Errors logged → TestErrorReporter captures failures
```

### 2. After Test Completion
```
GenerateReport() called → Markdown report generated → Saved to .test-errors/
```

### 3. GitHub Actions Processing
```
Artifacts uploaded → Workflows parse → Summary in Job Logs
```

## CI/CD Integration Points

### GitHub Actions Workflows

#### `build-and-test.yml` (Main workflow)
- **Triggers**: Push to main/develop, Pull requests
- **Tests on**: .NET 8.0, 9.0, 10.0
- **On Failure**:
  - Captures test output
  - Generates failure report
  - Uploads as artifact
  - Displays in job logs

#### `test-failures-report.yml` (Failure reporter)
- **Triggers**: After build-and-test workflow
- **On Failure**:
  - Downloads test failures artifact
  - Parses markdown report
  - Adds to job summary
  - Optionally creates GitHub issue

## Output Artifacts

### `.test-errors/test-failures.md`
Comprehensive markdown report with:
- All failed tests
- Error details and messages
- Stack traces
- Execution timestamps
- Test duration metrics

### `.test-errors/failures-summary.json`
Machine-readable JSON for CI parsing:
```json
{
  "timestamp": "2024-01-15T09:15:42Z",
  "totalFailures": 2,
  "totalErrors": 3,
  "failures": [
    {
      "testClass": "CalculatorTests",
      "testName": "Add_WithNegatives_ReturnsCorrect",
      "timestamp": "2024-01-15T09:15:42Z",
      "duration": 145,
      "errorCount": 1
    }
  ]
}
```

## GitHub Actions Output Examples

### Job Summary
```
## ❌ Test Failures Detected

## CalculatorTests.Add_WithNegatives_ReturnsCorrect
- Time: 09:15:42.123
- Duration: 145ms
- Errors: 1

### Error Details

**Error 1**: `AssertionFailure`

Assertion failed: Values do not match (Expected: 5, Actual: 3)

**Stack Trace**:
at PeasyPilot.Tests.CalculatorTests.Add_WithNegatives_ReturnsCorrect()
  in CalculatorTests.cs:line 45

---

## Summary

- Total test failures: **2**
- Total errors: **3**
- Total time: **245ms**
```

### Console Output
```
╔════════════════════════════════════════════════════════════════╗
║                    TEST FAILURE REPORT                          ║
╚════════════════════════════════════════════════════════════════╝

## CalculatorTests.Add_WithNegatives_ReturnsCorrect
...
[Error details]
...

╔════════════════════════════════════════════════════════════════╗
║  ❌ 2 test(s) failed with 3 total error(s)
╚════════════════════════════════════════════════════════════════╝
```

## Automatic Issue Creation

When tests fail in CI/CD, the workflow can automatically create a GitHub issue:

```
Title: Test Failures in main
Label: test-failure, automated
Body: [Complete markdown failure report]
```

This helps track failures and ensures visibility for the team.

## Viewing Failed Tests

### Option 1: GitHub Actions Job Logs
1. Go to Actions → Failed workflow
2. Scroll to "Parse and Display Failures" step
3. Failure report visible in step output

### Option 2: Job Summary
1. Go to workflow run summary page
2. Failure report appears in "Test Failure Report" section
3. Click download artifacts for full details

### Option 3: GitHub Issues
1. Go to Issues tab
2. Filter by "test-failure" label
3. View automatically created issue with full report

## Manual Report Generation

You can also generate reports manually in your tests:

```csharp
// At end of test suite or after critical test batch
[OneTimeTearDown]
public void FinalReport()
{
    TestErrorReporter.GenerateReport();
}
```

Or programmatically:

```csharp
public class ReportGenerator
{
    public static void Main()
    {
        // ... run tests ...
        TestErrorReporter.GenerateReport();
    }
}
```

## Report Files

Reports are saved to `.test-errors/` directory:

```
.test-errors/
├── test-failures.md          # Markdown report for humans
└── failures-summary.json     # JSON summary for automation
```

## Filtering and Analysis

The JSON format enables automated analysis:

```bash
# Count total failures
jq '.totalFailures' .test-errors/failures-summary.json

# List failed test names
jq -r '.failures[] | "\(.testClass).\(.testName)"' \
  .test-errors/failures-summary.json

# Find slowest test
jq -r '.failures | sort_by(-.duration) | .[0]' \
  .test-errors/failures-summary.json
```

## Environment Variables

Pass to workflows for custom reporting:

```yaml
env:
  REPORT_GITHUB_ISSUE: true      # Auto-create issue
  REPORT_RETENTION_DAYS: 30      # Keep reports for 30 days
  REPORT_NOTIFICATION: slack     # Send to Slack (future)
```

## Best Practices

1. **Always check failures**: Don't ignore test failures in CI/CD
2. **Review error reports**: Check the markdown report for full context
3. **Fix and commit**: Address failures and commit fixes
4. **Track metrics**: Monitor failure trends over time
5. **Clean up issues**: Close resolved failure issues

## Troubleshooting

### No Failure Report Generated
- Check that tests are actually failing
- Verify TestErrorReporter is called in DisposeLoggerAsync()
- Check `.test-errors/` directory permissions

### Workflow Not Running
- Verify workflow file in `.github/workflows/`
- Check branch protection rules allow workflows
- Ensure YAML syntax is valid

### Reports Not Uploading
- Check artifacts upload permissions
- Verify workflow has write access to repository
- Check artifact retention settings

## Integration with Other Tools

The JSON report format enables integration with:
- **Slack**: Send failure notifications
- **Jira**: Auto-create tickets for failures
- **DataDog**: Track test failure metrics
- **Grafana**: Visualize failure trends
- **PagerDuty**: Alert on critical test failures

## Security Notes

- Failure reports may contain sensitive data
- Artifacts retention period: 30 days (configurable)
- Reports are private to the repository
- Stack traces may reveal implementation details

---

For more details, see LOGGING_SYSTEM.md
