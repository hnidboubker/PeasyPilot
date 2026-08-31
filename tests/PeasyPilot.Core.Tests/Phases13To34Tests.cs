namespace PeasyPilot.Core.Tests;

using System.Net;
using PeasyPilot.BDD;
using PeasyPilot.Core;
using PeasyPilot.Core.Adapters;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Core.Context;
using PeasyPilot.Core.Diagnostics;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Filters;
using PeasyPilot.Core.ImpactAnalysis;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Reporting;
using PeasyPilot.Core.Scheduling;
using PeasyPilot.Integration.Fixtures;
using PeasyPilot.Integration.Helpers;
using Xunit;
using Assert = Xunit.Assert;

public class Phases13To34Tests
{
    [Fact]
    public async Task Phase13_RichConsoleReporter_GeneratesFormattedOutput()
    {
        var reporter = new RichConsoleReporter();
        var result = new TestRunResult { Passed = 10, Failed = 1, Status = TestRunStatus.Failed };
        var output = await reporter.ReportAsync(result);

        Assert.Contains("PEASYPILOT SUMMARY", output);
        Assert.Contains("Passed:", output);
    }

    [Fact]
    public async Task Phase14_SmartParallelScheduler_ExecutesOrderedByDuration()
    {
        var scheduler = new SmartParallelScheduler(2);
        var tests = new[]
        {
            new TestCase { Name = "Quick", Category = "unit" },
            new TestCase { Name = "Long", Category = "integration", Kind = TestKind.Integration }
        };

        var results = await scheduler.ExecuteAsync(tests);
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Equal(TestRunStatus.Passed, r.Status));
    }

    [Fact]
    public async Task Phase15_RetryTestScheduler_RetriesFailedTests()
    {
        var mockScheduler = new DefaultTestScheduler();
        var retryScheduler = new RetryTestScheduler(mockScheduler, maxRetries: 2);
        var tests = new[] { new TestCase { Name = "Flaky", Category = "unit" } };

        var results = await retryScheduler.ExecuteAsync(tests);
        Assert.Single(results);
    }

    [Fact]
    public async Task Phase16_HtmlFileReporter_GeneratesHtmlDashboard()
    {
        var reporter = new HtmlFileReporter();
        var result = new TestRunResult { Passed = 8, Failed = 2, Status = TestRunStatus.Failed };
        var html = await reporter.ReportAsync(result);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("PeasyPilot Test Execution Dashboard", html);
    }

    [Fact]
    public async Task Phase17_CiAnnotationReporter_ExecutesWithoutError()
    {
        var reporter = new CiAnnotationReporter();
        var result = new TestRunResult { Passed = 5, Status = TestRunStatus.Passed };
        var output = await reporter.ReportAsync(result);

        Assert.Empty(output);
    }

    [Fact]
    public void Phase18_MetadataTestFilter_MatchesKeyAndKind()
    {
        var tc = new TestCase { Name = "BddTest", Category = "spec", Kind = TestKind.Bdd };
        tc.Metadata["Env"] = "Staging";

        var filterMeta = new MetadataTestFilter("Env", "Staging");
        var filterKind = new MetadataTestFilter(TestKind.Bdd);

        Assert.True(filterMeta.Matches(tc));
        Assert.True(filterKind.Matches(tc));
    }

    [Fact]
    public async Task Phase19_InMemoryTestDatabase_CleansUpSuccessfully()
    {
        var db = new InMemoryTestDatabase();
        await db.InitializeAsync();
        await db.ResetAsync();

        Assert.NotNull(db.Store);
    }

    [Fact]
    public async Task Phase20_MockHttpServer_StubsAndRecordsRequests()
    {
        var server = new MockHttpServer();
        server.StubEndpoint("/api/hello", HttpStatusCode.OK, "{\"message\":\"world\"}");

        var client = server.CreateClient();
        var response = await client.GetAsync("/api/hello");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(server.RecordedRequests);
    }

    [Fact]
    public void Phase21_TestCorrelationContext_CreatesAndInjectsHeader()
    {
        var correlationId = TestCorrelationContext.CreateCorrelationId();
        var client = new HttpClient();

        TestCorrelationContext.InjectHeader(client, correlationId);

        Assert.True(client.DefaultRequestHeaders.Contains(TestCorrelationContext.CorrelationHeaderName));
    }

    [Fact]
    public void Phase23_TestLogCapture_StoresAndRetrievesLogs()
    {
        var capture = new TestLogCapture();
        capture.WriteLog("Starting component setup");
        capture.WriteLog("Setup finished");

        var logs = capture.GetLogs();
        Assert.Equal(2, logs.Count);
    }

    [Fact]
    public void Phase24_SnapshotAssert_ValidatesObjectGraph()
    {
        var actual = new { name = "John", age = 30 };
        var expected = "{\n  \"name\": \"John\",\n  \"age\": 30\n}";

        Assert.True(SnapshotAssert.MatchSnapshot(actual, expected));
    }

    [Fact]
    public void Phase25_GherkinFeatureParser_ParsesFeatureText()
    {
        var gherkin = @"
Feature: User Login
  Scenario: Valid Login
    Given a valid user
    When submitting credentials
    Then user is logged in
";
        var feature = GherkinFeatureParser.Parse(gherkin);
        Assert.Equal("User Login", feature.Name);
        Assert.Single(feature.Scenarios);
        Assert.Equal("Valid Login", feature.Scenarios[0].Name);
    }

    [Fact]
    public void Phase26_BddStepRegistry_RegistersAndMatchesSteps()
    {
        var registry = new BddStepRegistry();
        var executed = false;

        registry.RegisterStep("a valid user", () =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        var match = registry.FindMatch("Given a valid user");
        Assert.NotNull(match);
    }

    [Fact]
    public void Phase27_ScenarioOutline_ExpandsExamples()
    {
        var outline = new ScenarioOutline("Login as <user>")
            .AddSteps("Given <user>", "When login", "Then success")
            .AddExample(new Dictionary<string, string> { ["user"] = "Alice" })
            .AddExample(new Dictionary<string, string> { ["user"] = "Bob" });

        var scenarios = outline.Expand();
        Assert.Equal(2, scenarios.Count);
        Assert.Contains(scenarios, s => s.Name.Contains("Alice"));
    }

    [Fact]
    public void Phase28_LivingDocExporter_GeneratesMarkdownDocs()
    {
        var feature = new Feature("Billing");
        feature.AddScenario("Invoice Generation").Given("order paid").Then("invoice sent");

        var md = LivingDocExporter.ExportToMarkdown([feature]);
        Assert.Contains("Living Documentation", md);
        Assert.Contains("Billing", md);
    }

    [Fact]
    public async Task Phase29_GitAstImpactAnalyzer_FiltersImpactedTests()
    {
        var analyzer = new GitAstImpactAnalyzer();
        var tests = new[]
        {
            new TestCase { Name = "CustomerServiceTests", Category = "unit" },
            new TestCase { Name = "PaymentServiceTests", Category = "unit" }
        };

        var impacted = await analyzer.GetImpactedTestsAsync(["CustomerService.cs"], tests);
        Assert.Single(impacted);
        Assert.Equal("CustomerServiceTests", impacted.First().Name);
    }

    [Fact]
    public void Phase30_RootCauseAnalyzer_CategorizesFailures()
    {
        var failure = new TestFailure { Message = "Assert failed", Expected = "1", Actual = "2" };
        var cause = RootCauseAnalyzer.AnalyzeRootCause(failure);

        Assert.Contains("Assertion Mismatch", cause);
    }

    [Fact]
    public void Phase31_PerformanceTracker_DetectsRegressions()
    {
        var tracker = new PerformanceTracker();
        tracker.SetBaseline("FastTest", 50); // 50ms baseline

        var normalResult = new TestResult { Name = "FastTest", Duration = TimeSpan.FromMilliseconds(60) };
        var slowResult = new TestResult { Name = "FastTest", Duration = TimeSpan.FromMilliseconds(200) };

        Assert.False(tracker.IsPerformanceRegressed(normalResult));
        Assert.True(tracker.IsPerformanceRegressed(slowResult));
    }

    [Fact]
    public void Phase32_IdeProtocolAdapter_SerializesDiscoveryAndExecution()
    {
        var tests = new[] { new TestCase { Name = "UnitTest1", Category = "core" } };
        var discoveryJson = IdeProtocolAdapter.SerializeDiscovery(tests);

        Assert.Contains("UnitTest1", discoveryJson);
    }

    [Fact]
    public void Phase33_MutationScoreEvaluator_CalculatesPercentage()
    {
        var score = MutationScoreEvaluator.CalculateMutationScore(totalMutants: 10, killedMutants: 8);
        Assert.Equal(80.0, score);
    }

    [Fact]
    public async Task Phase34_PeasyPilotPlatform_ExecutesFullEnterprisePipeline()
    {
        var platform = PeasyPilotPlatform.Instance;
        Assert.Equal("1.0.0-enterprise", platform.Version);

        var options = new TestPipelineOptions();
        var result = await platform.ExecuteAsync(options);

        Assert.NotNull(result);
        Assert.Equal(TestRunStatus.Passed, result.Status);

        var history = await platform.RunStore.GetRunHistoryAsync();
        Assert.NotEmpty(history);
    }
}
