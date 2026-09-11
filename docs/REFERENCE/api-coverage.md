# PeasyPilot.Coverage API Reference

## Overview

`PeasyPilot.Coverage` provides code coverage reporting abstractions and analysis tools for tracking test coverage metrics. It offers a framework-agnostic interface for collecting, reporting, and analyzing code coverage data from test runs, enabling visibility into which parts of your codebase are exercised by your tests.

**Key Responsibilities:**
- Coverage reporting abstraction via ICoverageProvider
- Line coverage and branch coverage tracking
- Coverage percentage calculation
- Framework-agnostic coverage integration
- Seamless DI container integration
- Coverage metrics for orchestration and reporting
- Support for coverage-based test quality assessment

**Targets:** .NET 8.0, 9.0, 10.0

**Optional Dependencies:** OpenCover, CodeCoverage (external tools)

---

## Main Abstractions

### ICoverageProvider

Interface for collecting code coverage information from test runs.

```csharp
namespace PeasyPilot.Coverage;

/// <summary>
/// Provider for collecting code coverage information.
/// </summary>
public interface ICoverageProvider
{
    /// <summary>
    /// Gets the coverage report asynchronously.
    /// </summary>
    /// <returns>The coverage report.</returns>
    Task<CoverageReport> GetCoverageAsync();
}
```

**Purpose:** Provides a framework-agnostic abstraction for collecting coverage metrics, enabling pluggable implementations that work with different coverage tools (OpenCover, CodeCoverage, etc.).

---

## Core Models

### CoverageReport

Comprehensive code coverage metrics for a test run.

```csharp
namespace PeasyPilot.Coverage;

/// <summary>
/// Code coverage information for a test run.
/// </summary>
public class CoverageReport
{
    /// <summary>
    /// Gets or sets the total lines covered.
    /// </summary>
    public int LinesCovered { get; set; }

    /// <summary>
    /// Gets or sets the total lines in code.
    /// </summary>
    public int TotalLines { get; set; }

    /// <summary>
    /// Gets or sets the total branches covered.
    /// </summary>
    public int BranchesCovered { get; set; }

    /// <summary>
    /// Gets or sets the total branches.
    /// </summary>
    public int TotalBranches { get; set; }

    /// <summary>
    /// Gets the line coverage percentage (0-100).
    /// </summary>
    public double LineCoveragePercentage
    {
        get => TotalLines > 0 ? (LinesCovered * 100.0) / TotalLines : 0;
    }

    /// <summary>
    /// Gets the branch coverage percentage (0-100).
    /// </summary>
    public double BranchCoveragePercentage
    {
        get => TotalBranches > 0 ? (BranchesCovered * 100.0) / TotalBranches : 0;
    }

    /// <summary>
    /// Gets a string representation of the coverage report.
    /// </summary>
    public override string ToString()
    {
        return $"Coverage Report\n" +
               $"  Line Coverage: {LineCoveragePercentage:F2}% ({LinesCovered}/{TotalLines})\n" +
               $"  Branch Coverage: {BranchCoveragePercentage:F2}% ({BranchesCovered}/{TotalBranches})";
    }
}
```

**Purpose:** Encapsulates coverage metrics with calculated percentage properties for easy reporting and assertions.

---

## Coverage Metrics

### Line Coverage

Measures the percentage of executable lines that were executed during test runs:

```
Line Coverage % = (Lines Covered / Total Lines) × 100
```

**Interpretation:**
- **90-100%:** Excellent coverage (most code is exercised)
- **70-89%:** Good coverage (most paths are tested)
- **50-69%:** Acceptable coverage (basic paths tested)
- **<50%:** Low coverage (significant gaps)

### Branch Coverage

Measures the percentage of code branches (conditionals) that were executed:

```
Branch Coverage % = (Branches Covered / Total Branches) × 100
```

**Interpretation:**
- **90-100%:** All code paths are tested
- **70-89%:** Most conditional paths are covered
- **50-69%:** Some paths are not covered
- **<50%:** Significant path gaps exist

---

## Working Examples

### Example 1: Basic Coverage Report Creation

```csharp
using PeasyPilot.Coverage;

public class BasicCoverageReportTest
{
    [Fact]
    public void TestCreateCoverageReport()
    {
        var report = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        Assert.Equal(450, report.LinesCovered);
        Assert.Equal(500, report.TotalLines);
        Assert.Equal(90.0, report.LineCoveragePercentage);
        Assert.Equal(87.5, report.BranchCoveragePercentage);
    }

    [Fact]
    public void TestCoveragePercentageCalculation()
    {
        var report = new CoverageReport
        {
            LinesCovered = 75,
            TotalLines = 100,
            BranchesCovered = 15,
            TotalBranches = 20
        };

        Assert.Equal(75.0, report.LineCoveragePercentage);
        Assert.Equal(75.0, report.BranchCoveragePercentage);
    }

    [Fact]
    public void TestZeroCoverageEdgeCase()
    {
        var report = new CoverageReport
        {
            LinesCovered = 0,
            TotalLines = 0,
            BranchesCovered = 0,
            TotalBranches = 0
        };

        Assert.Equal(0.0, report.LineCoveragePercentage);
        Assert.Equal(0.0, report.BranchCoveragePercentage);
    }
}
```

### Example 2: Coverage Report String Representation

```csharp
using PeasyPilot.Coverage;

public class CoverageReportFormattingTest
{
    [Fact]
    public void TestCoverageReportToString()
    {
        var report = new CoverageReport
        {
            LinesCovered = 420,
            TotalLines = 500,
            BranchesCovered = 32,
            TotalBranches = 40
        };

        var reportString = report.ToString();

        Assert.Contains("Coverage Report", reportString);
        Assert.Contains("Line Coverage: 84.00%", reportString);
        Assert.Contains("420/500", reportString);
        Assert.Contains("Branch Coverage: 80.00%", reportString);
        Assert.Contains("32/40", reportString);
    }

    [Fact]
    public void TestMultipleCoverageReports()
    {
        var reports = new[]
        {
            new CoverageReport
            {
                LinesCovered = 450,
                TotalLines = 500,
                BranchesCovered = 35,
                TotalBranches = 40
            },
            new CoverageReport
            {
                LinesCovered = 300,
                TotalLines = 400,
                BranchesCovered = 28,
                TotalBranches = 32
            }
        };

        foreach (var report in reports)
        {
            Console.WriteLine(report.ToString());
            Assert.True(report.LineCoveragePercentage >= 0);
            Assert.True(report.LineCoveragePercentage <= 100);
        }
    }
}
```

### Example 3: Coverage Threshold Validation

```csharp
using PeasyPilot.Coverage;

public class CoverageThresholdTest
{
    [Fact]
    public void TestValidateCoverageThreshold()
    {
        var minimumLinesCoverageThreshold = 80.0;
        var minimumBranchCoverageThreshold = 75.0;

        var report = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        Assert.True(report.LineCoveragePercentage >= minimumLinesCoverageThreshold,
            $"Line coverage {report.LineCoveragePercentage}% is below threshold {minimumLinesCoverageThreshold}%");

        Assert.True(report.BranchCoveragePercentage >= minimumBranchCoverageThreshold,
            $"Branch coverage {report.BranchCoveragePercentage}% is below threshold {minimumBranchCoverageThreshold}%");
    }

    [Fact]
    public void TestFailsCoverageThreshold()
    {
        var minimumCoverageThreshold = 90.0;

        var report = new CoverageReport
        {
            LinesCovered = 75,
            TotalLines = 100,
            BranchesCovered = 10,
            TotalBranches = 20
        };

        Assert.False(report.LineCoveragePercentage >= minimumCoverageThreshold,
            "Coverage should fail the threshold");
    }
}
```

### Example 4: Coverage Progress Tracking

```csharp
using PeasyPilot.Coverage;

public class CoverageProgressTrackingTest
{
    [Fact]
    public void TestTrackCoverageImprovement()
    {
        var reports = new[]
        {
            new CoverageReport
            {
                LinesCovered = 50,
                TotalLines = 200,
                BranchesCovered = 10,
                TotalBranches = 40
            },
            new CoverageReport
            {
                LinesCovered = 120,
                TotalLines = 200,
                BranchesCovered = 28,
                TotalBranches = 40
            },
            new CoverageReport
            {
                LinesCovered = 180,
                TotalLines = 200,
                BranchesCovered = 38,
                TotalBranches = 40
            }
        };

        var lineCoverageProgress = reports
            .Select(r => r.LineCoveragePercentage)
            .ToList();

        Assert.Equal(25.0, lineCoverageProgress[0]);
        Assert.Equal(60.0, lineCoverageProgress[1]);
        Assert.Equal(90.0, lineCoverageProgress[2]);

        // Verify improvement trend
        for (int i = 1; i < lineCoverageProgress.Count; i++)
        {
            Assert.True(lineCoverageProgress[i] >= lineCoverageProgress[i - 1],
                "Coverage should not decrease");
        }
    }
}
```

### Example 5: Integration with DI Container

```csharp
using PeasyPilot.Coverage;
using Microsoft.Extensions.DependencyInjection;

public class CoverageProviderDiTest
{
    [Fact]
    public async Task TestCoverageProviderWithDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICoverageProvider, MockCoverageProvider>();
        var serviceProvider = services.BuildServiceProvider();

        var coverageProvider = serviceProvider.GetRequiredService<ICoverageProvider>();
        var report = await coverageProvider.GetCoverageAsync();

        Assert.NotNull(report);
        Assert.True(report.TotalLines > 0);
    }
}

public class MockCoverageProvider : ICoverageProvider
{
    public Task<CoverageReport> GetCoverageAsync()
    {
        var report = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        return Task.FromResult(report);
    }
}
```

### Example 6: Coverage Report Comparison

```csharp
using PeasyPilot.Coverage;

public class CoverageComparisonTest
{
    [Fact]
    public void TestCompareCoverageReports()
    {
        var reportBefore = new CoverageReport
        {
            LinesCovered = 300,
            TotalLines = 500,
            BranchesCovered = 20,
            TotalBranches = 40
        };

        var reportAfter = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        var lineCoverageImprovement = 
            reportAfter.LineCoveragePercentage - reportBefore.LineCoveragePercentage;
        var branchCoverageImprovement = 
            reportAfter.BranchCoveragePercentage - reportBefore.BranchCoveragePercentage;

        Assert.Equal(30.0, lineCoverageImprovement);
        Assert.Equal(37.5, branchCoverageImprovement);
        Assert.True(lineCoverageImprovement > 0, "Coverage should improve");
    }
}
```

### Example 7: Coverage Report Aggregation

```csharp
using PeasyPilot.Coverage;
using System.Collections.Generic;

public class CoverageAggregationTest
{
    [Fact]
    public void TestAggregateCoverageReports()
    {
        var reports = new[]
        {
            new CoverageReport
            {
                LinesCovered = 100,
                TotalLines = 200,
                BranchesCovered = 10,
                TotalBranches = 20
            },
            new CoverageReport
            {
                LinesCovered = 150,
                TotalLines = 200,
                BranchesCovered = 15,
                TotalBranches = 20
            },
            new CoverageReport
            {
                LinesCovered = 200,
                TotalLines = 200,
                BranchesCovered = 20,
                TotalBranches = 20
            }
        };

        var aggregatedReport = new CoverageReport
        {
            LinesCovered = reports.Sum(r => r.LinesCovered),
            TotalLines = reports.Sum(r => r.TotalLines),
            BranchesCovered = reports.Sum(r => r.BranchesCovered),
            TotalBranches = reports.Sum(r => r.TotalBranches)
        };

        Assert.Equal(450, aggregatedReport.LinesCovered);
        Assert.Equal(600, aggregatedReport.TotalLines);
        Assert.Equal(75.0, aggregatedReport.LineCoveragePercentage);
    }
}
```

### Example 8: Coverage-Based Test Quality Scoring

```csharp
using PeasyPilot.Coverage;

public class CoverageQualityScoringTest
{
    private double CalculateQualityScore(CoverageReport report)
    {
        // Score based on both line and branch coverage
        const double lineWeightage = 0.6;
        const double branchWeightage = 0.4;

        var score = (report.LineCoveragePercentage * lineWeightage) +
                    (report.BranchCoveragePercentage * branchWeightage);

        return Math.Round(score, 2);
    }

    [Fact]
    public void TestQualityScoring()
    {
        var excellentReport = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        var goodReport = new CoverageReport
        {
            LinesCovered = 350,
            TotalLines = 500,
            BranchesCovered = 28,
            TotalBranches = 40
        };

        var poorReport = new CoverageReport
        {
            LinesCovered = 150,
            TotalLines = 500,
            BranchesCovered = 10,
            TotalBranches = 40
        };

        Assert.True(CalculateQualityScore(excellentReport) > 
                   CalculateQualityScore(goodReport));
        Assert.True(CalculateQualityScore(goodReport) > 
                   CalculateQualityScore(poorReport));
    }
}
```

### Example 9: Coverage Report Validation

```csharp
using PeasyPilot.Coverage;

public class CoverageValidationTest
{
    private bool IsValidCoverageReport(CoverageReport report)
    {
        // Validate logical constraints
        if (report.LinesCovered < 0 || report.TotalLines < 0)
            return false;

        if (report.BranchesCovered < 0 || report.TotalBranches < 0)
            return false;

        if (report.LinesCovered > report.TotalLines)
            return false;

        if (report.BranchesCovered > report.TotalBranches)
            return false;

        return true;
    }

    [Fact]
    public void TestValidCoverageReport()
    {
        var validReport = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        Assert.True(IsValidCoverageReport(validReport));
    }

    [Fact]
    public void TestInvalidCoverageReport()
    {
        var invalidReport = new CoverageReport
        {
            LinesCovered = 600,  // More than total
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        Assert.False(IsValidCoverageReport(invalidReport));
    }
}
```

### Example 10: Coverage Report JSON Serialization

```csharp
using PeasyPilot.Coverage;
using System.Text.Json;

public class CoverageJsonSerializationTest
{
    [Fact]
    public void TestSerializeCoverageReport()
    {
        var report = new CoverageReport
        {
            LinesCovered = 450,
            TotalLines = 500,
            BranchesCovered = 35,
            TotalBranches = 40
        };

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Assert.Contains("\"linesCovered\"", json);
        Assert.Contains("\"totalLines\"", json);
        Assert.Contains("450", json);
        Assert.Contains("500", json);
    }

    [Fact]
    public void TestDeserializeCoverageReport()
    {
        var json = @"{
            ""linesCovered"": 450,
            ""totalLines"": 500,
            ""branchesCovered"": 35,
            ""totalBranches"": 40
        }";

        var report = JsonSerializer.Deserialize<CoverageReport>(json);

        Assert.NotNull(report);
        Assert.Equal(450, report.LinesCovered);
        Assert.Equal(500, report.TotalLines);
        Assert.Equal(90.0, report.LineCoveragePercentage);
    }
}
```

---

## Coverage Metrics Interpretation

### Coverage Ranges and Recommendations

| Coverage % | Assessment | Recommendation |
|-----------|------------|-----------------|
| 90-100% | Excellent | Production ready, maintain quality |
| 80-89% | Very Good | Minor gaps, acceptable for release |
| 70-79% | Good | Noticeable gaps, improve before release |
| 60-69% | Acceptable | Significant gaps, testing needed |
| <60% | Poor | Major gaps, substantial work needed |

### Coverage Gap Analysis

When coverage is below target:
1. Identify uncovered lines and branches
2. Determine if they're critical paths or edge cases
3. Add tests for critical paths
4. Document why edge cases are not covered
5. Track coverage trends over time

---

## Integration Patterns

### Coverage-Gated Builds

Fail builds if coverage falls below threshold:

```csharp
public async Task ValidateCoverageTresholdAsync(
    ICoverageProvider provider,
    double minimumCoverage)
{
    var report = await provider.GetCoverageAsync();
    
    if (report.LineCoveragePercentage < minimumCoverage)
    {
        throw new Exception(
            $"Coverage {report.LineCoveragePercentage}% is below " +
            $"minimum {minimumCoverage}%");
    }
}
```

### Coverage Reporting

Export coverage metrics for dashboards:

```csharp
public string GenerateCoverageReport(CoverageReport report)
{
    return $@"
    # Coverage Report
    
    - **Line Coverage:** {report.LineCoveragePercentage:F2}% ({report.LinesCovered}/{report.TotalLines})
    - **Branch Coverage:** {report.BranchCoveragePercentage:F2}% ({report.BranchesCovered}/{report.TotalBranches})
    - **Status:** {GetCoverageStatus(report)}
    ";
}
```

---

## Performance Considerations

- **Metric Calculation:** O(1) - instant percentage calculations
- **Report Creation:** Minimal overhead for aggregation
- **Memory:** Lightweight data structure (4 integers only)
- **Scalability:** Handles projects of any size without performance degradation

---

## See Also

- [PeasyPilot.Core API](api-core.md)
- [Test Generation Guide](../GUIDES/test-generation-guide.md)
- [Unit Testing Guide](../GUIDES/unit-testing-guide.md)
- [OpenCover Documentation](https://github.com/OpenCover/opencover)

