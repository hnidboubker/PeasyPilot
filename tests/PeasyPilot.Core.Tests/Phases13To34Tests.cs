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
using PeasyPilot.XUnit;

using Xunit;


public class Phases13To34Tests
{
    [Fact]
    public async Task Phase13_RichConsoleReporter_GeneratesFormattedOutput()
    {
        var reporter = new RichConsoleReporter();
        var result = new TestRunResult { Passed = 10, Failed = 1, Status = TestRunStatus.Failed };
        var output = await reporter.ReportAsync(result);

        XAssert.Contains("PEASYPILOT SUMMARY", output);
        XAssert.Contains("Passed:", output);
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
        XAssert.Equal(2, results.Count);
        XAssert.All(results, r => Assert.Equal(TestRunStatus.Passed, r.Status));
    }

    [Fact]
    public async Task Phase15_RetryTestScheduler_RetriesFailedTests()
    {
        var mockScheduler = new DefaultTestScheduler();
        var retryScheduler = new RetryTestScheduler(mockScheduler, maxRetries: 2);
        var tests = new[] { new TestCase { Name = "Flaky", Category = "unit" } };

        var results = await retryScheduler.ExecuteAsync(tests);
        XAssert.Single(results);
    }

    [Fact]
    public async Task Phase16_HtmlFileReporter_GeneratesHtmlDashboard()
    {
        var reporter = new HtmlFileReporter();
        var result = new TestRunResult { Passed = 8, Failed = 2, Status = TestRunStatus.Failed };
        var html = await reporter.ReportAsync(result);

        XAssert.Contains("<!DOCTYPE html>", html);
        XAssert.Contains("PeasyPilot Test Execution Dashboard", html);
    }

    [Fact]
    public async Task Phase17_CiAnnotationReporter_ExecutesWithoutError()
    {
        var reporter = new CiAnnotationReporter();
        var result = new TestRunResult { Passed = 5, Status = TestRunStatus.Passed };
        var output = await reporter.ReportAsync(result);

        XAssert.IsEmpty(output);
    }

    [Fact]
    public void Phase18_MetadataTestFilter_MatchesKeyAndKind()
    {
        var tc = new TestCase { Name = "BddTest", Category = "spec", Kind = TestKind.Bdd };
        tc.Metadata["Env"] = "Staging";

        var filterMeta = new MetadataTestFilter("Env", "Staging");
        var filterKind = new MetadataTestFilter(TestKind.Bdd);

        XAssert.True(filterMeta.Matches(tc));
        XAssert.True(filterKind.Matches(tc));
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

        XAssert.Equal(HttpStatusCode.OK, response.StatusCode);
        XAssert.Single(server.RecordedRequests);
    }

    [Fact]
    public void Phase21_TestCorrelationContext_CreatesAndInjectsHeader()
    {
        var correlationId = TestCorrelationContext.CreateCorrelationId();
        var client = new HttpClient();

        TestCorrelationContext.InjectHeader(client, correlationId);

        XAssert.True(client.DefaultRequestHeaders.Contains(TestCorrelationContext.CorrelationHeaderName));
    }

    [Fact]
    public void Phase23_TestLogCapture_StoresAndRetrievesLogs()
    {
        var capture = new TestLogCapture();
        capture.WriteLog("Starting component setup");
        capture.WriteLog("Setup finished");

        var logs = capture.GetLogs();
        XAssert.Equal(2, logs.Count);
    }

    [Fact]
    public void Phase24_SnapshotAssert_ValidatesObjectGraph()
    {
        var actual = new { name = "John", age = 30 };
        var expected = "{\n  \"name\": \"John\",\n  \"age\": 30\n}";

        XAssert.True(SnapshotAssert.MatchSnapshot(actual, expected));
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
        XAssert.Equal("User Login", feature.Name);
        XAssert.Single(feature.Scenarios);
        XAssert.Equal("Valid Login", feature.Scenarios[0].Name);
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
        XAssert.NotNull(match);
    }

    [Fact]
    public void Phase27_ScenarioOutline_ExpandsExamples()
    {
        var outline = new ScenarioOutline("Login as <user>")
            .AddSteps("Given <user>", "When login", "Then success")
            .AddExample(new Dictionary<string, string> { ["user"] = "Alice" })
            .AddExample(new Dictionary<string, string> { ["user"] = "Bob" });

        var scenarios = outline.Expand();
        XAssert.Equal(2, scenarios.Count);
        // Todo cleui a beoin un Expression
        XAssert.Contains(scenarios, s => s.Name.Contains("Alice"));
    }

    [Fact]
    public void Phase28_LivingDocExporter_GeneratesMarkdownDocs()
    {
        var feature = new Feature("Billing");
        feature.AddScenario("Invoice Generation").Given("order paid").Then("invoice sent");

        var md = LivingDocExporter.ExportToMarkdown([feature]);
        XAssert.Contains("Living Documentation", md);
        XAssert.Contains("Billing", md);
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
        XAssert.Single(impacted);
        XAssert.Equal("CustomerServiceTests", impacted.First().Name);
    }

    [Fact]
    public void Phase30_RootCauseAnalyzer_CategorizesFailures()
    {
        var failure = new TestFailure { Message = "Assert failed", Expected = "1", Actual = "2" };
        var cause = RootCauseAnalyzer.AnalyzeRootCause(failure);

        XAssert.Contains("Assertion Mismatch", cause);
    }

    [Fact]
    public void Phase31_PerformanceTracker_DetectsRegressions()
    {
        var tracker = new PerformanceTracker();
        tracker.SetBaseline("FastTest", 50); // 50ms baseline

        var normalResult = new TestResult { Name = "FastTest", Duration = TimeSpan.FromMilliseconds(60) };
        var slowResult = new TestResult { Name = "FastTest", Duration = TimeSpan.FromMilliseconds(200) };

        XAssert.False(tracker.IsPerformanceRegressed(normalResult));
        XAssert.True(tracker.IsPerformanceRegressed(slowResult));
    }

    [Fact]
    public void Phase32_IdeProtocolAdapter_SerializesDiscoveryAndExecution()
    {
        var tests = new[] { new TestCase { Name = "UnitTest1", Category = "core" } };
        var discoveryJson = IdeProtocolAdapter.SerializeDiscovery(tests);

        XAssert.Contains("UnitTest1", discoveryJson);
    }

    [Fact]
    public void Phase33_MutationScoreEvaluator_CalculatesPercentage()
    {
        var score = MutationScoreEvaluator.CalculateMutationScore(totalMutants: 10, killedMutants: 8);
        XAssert.Equal(80.0, score);
    }

    [Fact]
    public async Task Phase34_PeasyPilotPlatform_ExecutesFullEnterprisePipeline()
    {
        var platform = PeasyPilotPlatform.Instance;
        XAssert.Equal("1.0.0-enterprise", platform.Version);

        var options = new TestPipelineOptions();
        var result = await platform.ExecuteAsync(options);

        XAssert.NotNull(result);
        XAssert.Equal(TestRunStatus.Passed, result.Status);

        var history = await platform.RunStore.GetRunHistoryAsync();
        XAssert.NotEmpty(history);
    }
}
