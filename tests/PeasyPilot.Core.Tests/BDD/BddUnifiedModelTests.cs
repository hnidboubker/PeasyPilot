namespace PeasyPilot.Core.Tests.BDD;

using PeasyPilot.BDD;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

using Xunit;


public class BddUnifiedModelTests
{
    [Fact]
    public void Scenario_ToTestCase_MapsToBddKindAndMetadata()
    {
        // Arrange
        var scenario = new Scenario("User Login")
            .Given("a valid user credentials")
            .When("the user submits the login form")
            .Then("the dashboard should be displayed", () => true);

        // Act
        var testCase = scenario.ToTestCase("Authentication");

        // XAssert
        PeasyAssert.Equal("Authentication - User Login", testCase.Name);
        PeasyAssert.Equal("Authentication", testCase.Category);
        PeasyAssert.Equal(TestKind.Bdd, testCase.Kind);
        PeasyAssert.Contains("Gherkin", testCase.Metadata.Keys);
        PeasyAssert.Equal("3", testCase.Metadata["StepCount"]);
    }

    [Fact]
    public async Task Scenario_ExecuteAndAsTestResultAsync_ReturnsUnifiedTestResult()
    {
        // Arrange
        var executedSteps = new List<string>();
        var scenario = new Scenario("Password Reset")
            .Given("a registered user", async () =>
            {
                executedSteps.Add("Given");
                await Task.CompletedTask;
            })
            .When("requesting password reset", async () =>
            {
                executedSteps.Add("When");
                await Task.CompletedTask;
            })
            .Then("reset email should be sent", () => true);

        // Act
        var result = await scenario.ExecuteAndAsTestResultAsync("Authentication");

        // XAssert
        PeasyAssert.Equal("Authentication - Password Reset", result.Name);
        PeasyAssert.Equal(TestRunStatus.Passed, result.Status);
        PeasyAssert.Null(result.Failure);
        PeasyAssert.Equal(2, executedSteps.Count);
    }

    [Fact]
    public async Task Feature_ExecuteAndAsTestRunResultAsync_AggregatesResults()
    {
        // Arrange
        var feature = new Feature("Order Processing");
        feature.AddScenario("Create Order")
            .Given("valid cart")
            .When("checkout is pressed")
            .Then("order is created", () => true);

        feature.AddScenario("Cancel Order")
            .Given("an existing order")
            .When("cancel button is clicked")
            .Then("order is cancelled", () => true);

        // Act
        var cases = feature.ToTestCases();
        var runResult = await feature.ExecuteAndAsTestRunResultAsync();

        // XAssert
        PeasyAssert.Equal(2, cases.Count);
        PeasyAssert.All(cases, c => PeasyAssert.Equal(TestKind.Bdd, c.Kind));
        PeasyAssert.Equal(2, runResult.Passed);
        PeasyAssert.Equal(0, runResult.Failed);
        PeasyAssert.Equal(TestRunStatus.Passed, runResult.Status);
    }
}
