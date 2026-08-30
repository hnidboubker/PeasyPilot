namespace PeasyPilot.Core.Tests.Orchestration;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Discovery;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Filters;
using PeasyPilot.Core.ImpactAnalysis;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Orchestration;
using PeasyPilot.Core.Reporting;
using PeasyPilot.Core.Scheduling;
using Xunit;

public class TestPipelineOrchestratorTests
{
    [Fact]
    public async Task ExecutePipelineAsync_FullPipelineSuccess_ReturnsAggregatedResults()
    {
        // Arrange
        var testCases = new[]
        {
            new TestCase { Name = "CustomerService.Create_Valid", Category = "unit" },
            new TestCase { Name = "CustomerService.Delete_Valid", Category = "unit" },
            new TestCase { Name = "OrderService.Process_Valid", Category = "integration" }
        };

        var discovery = new DefaultTestDiscovery(testCases);
        var scheduler = new DefaultTestScheduler();
        var impactAnalyzer = new DefaultTestImpactAnalyzer();

        var orchestrator = new TestPipelineOrchestrator(discovery, scheduler, impactAnalyzer);

        var consoleReporter = new ConsoleReporter();
        var jsonReporter = new JsonFileReporter();

        var options = new TestPipelineOptions
        {
            ChangedFiles = ["CustomerService"],
            Filter = new NameTestFilter("Create"),
            Reporters = [consoleReporter, jsonReporter],
            RunDiagnosticsOnFailure = true
        };

        // Act
        var result = await orchestrator.ExecutePipelineAsync(options);

        // Assert
        Assert.Equal(3, result.DiscoveredCount);
        Assert.Equal(2, result.ImpactedCount); // 2 CustomerService tests
        Assert.Equal(1, result.ScheduledCount); // 1 CustomerService.Create_Valid test after filter
        Assert.Single(result.TestResults);
        Assert.Equal("CustomerService.Create_Valid", result.TestResults.First().Name);
        Assert.Equal(TestRunStatus.Passed, result.Status);
        Assert.Empty(result.Diagnostics); // No diagnostics since zero failures
    }

    [Fact]
    public async Task JsonFileReporter_GeneratesValidJson()
    {
        var reporter = new JsonFileReporter();
        var runResult = new TestRunResult
        {
            Passed = 5,
            Failed = 0,
            Skipped = 1,
            Duration = TimeSpan.FromSeconds(2),
            Status = TestRunStatus.Passed
        };

        var json = await reporter.ReportAsync(runResult);

        Assert.NotNull(json);
        Assert.Contains("\"Passed\": 5", json);
        Assert.Contains("\"Skipped\": 1", json);
    }

    [Fact]
    public async Task JUnitXmlReporter_GeneratesValidXml()
    {
        var reporter = new JUnitXmlReporter();
        var runResult = new TestRunResult
        {
            Passed = 4,
            Failed = 1,
            Skipped = 0,
            Duration = TimeSpan.FromSeconds(1.5),
            Status = TestRunStatus.Failed
        };

        var xml = await reporter.ReportAsync(runResult);

        Assert.NotNull(xml);
        Assert.Contains("<testsuites>", xml);
        Assert.Contains("failures=\"1\"", xml);
        Assert.Contains("tests=\"5\"", xml);
    }
}
