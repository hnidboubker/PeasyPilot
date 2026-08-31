namespace PeasyPilot.Core.Tests.Reporting;

using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Reporting;
using Xunit;
using Assert = Xunit.Assert;

public class SmartDiagnosticProviderTests
{
    [Fact]
    public async Task DiagnoseAsync_PassedResult_ReturnsCleanDiagnostic()
    {
        // Arrange
        var provider = new SmartDiagnosticProvider();
        var result = new TestRunResult
        {
            Passed = 5,
            Failed = 0,
            Status = TestRunStatus.Passed
        };

        // Act
        var diagnostic = await provider.DiagnoseAsync(result);

        // Assert
        Assert.NotNull(diagnostic);
        Assert.Contains("cleanly", diagnostic.Summary);
        Assert.Empty(diagnostic.Suggestions);
    }

    [Fact]
    public async Task DiagnoseAsync_FailedResult_GeneratesActionableSuggestions()
    {
        // Arrange
        var provider = new SmartDiagnosticProvider();
        var result = new TestRunResult
        {
            Passed = 4,
            Failed = 2,
            Status = TestRunStatus.Failed
        };

        // Act
        var diagnostic = await provider.DiagnoseAsync(result);

        // Assert
        Assert.NotNull(diagnostic);
        Assert.Contains("2 test failure", diagnostic.Summary);
        Assert.NotEmpty(diagnostic.Suggestions);
    }
}
