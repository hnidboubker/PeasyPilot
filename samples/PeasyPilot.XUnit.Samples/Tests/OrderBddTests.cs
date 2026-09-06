namespace PeasyPilot.XUnit.Samples.Tests;

using PeasyPilot.BDD;
using PeasyPilot.BDD.Execution;
using PeasyPilot.BDD.FileLoading;
using Xunit;

/// <summary>
/// BDD tests for order processing using feature files.
/// </summary>
public class OrderBddTests
{
    private readonly IFeatureFileLoader _loader = new GherkinFeatureFileLoader();
    private readonly IScenarioExecutor _executor = new ScenarioExecutor();

    private string GetFeaturePath()
    {
        var testDir = AppContext.BaseDirectory;
        var di = new DirectoryInfo(testDir);
        while (di != null && !File.Exists(Path.Combine(di.FullName, "easy-peasy.slnx")))
        {
            di = di.Parent;
        }
        var solutionRoot = di?.FullName ?? throw new InvalidOperationException("Could not find solution root");
        return Path.Combine(solutionRoot, "samples/PeasyPilot.XUnit.Samples/features/orders.feature");
    }

    [Fact]
    public async Task OrderFeatures_LoadSuccessfully()
    {
        // Arrange
        var featurePath = GetFeaturePath();

        // Act
        var feature = await _loader.LoadFromFileAsync(featurePath);

        // Assert
        Assert.NotNull(feature);
        Assert.Equal("Order Processing", feature.Name);
        Assert.Equal(5, feature.Scenarios.Count);
    }

    [Fact]
    public async Task OrderFeatures_ScenariosExist()
    {
        // Arrange
        var featurePath = GetFeaturePath();
        var feature = await _loader.LoadFromFileAsync(featurePath);

        // Assert
        var scenarioNames = feature.Scenarios.Select(s => s.Name).ToList();
        Assert.Contains("Create an order", scenarioNames);
        Assert.Contains("Add items to order", scenarioNames);
        Assert.Contains("Multiple items in order", scenarioNames);
        Assert.Contains("Update order status", scenarioNames);
        Assert.Contains("Cancel order", scenarioNames);
    }

    [Fact]
    public async Task CreateOrderScenario_HasCorrectSteps()
    {
        // Arrange
        var featurePath = GetFeaturePath();
        var feature = await _loader.LoadFromFileAsync(featurePath);
        var scenario = feature.Scenarios.First(s => s.Name == "Create an order");

        // Assert
        Assert.True(scenario.Steps.Count >= 3, "Scenario should have at least 3 steps");

        var stepTypes = scenario.Steps.Select(s => s.Type).ToList();
        Assert.Contains(StepType.Given, stepTypes);
        Assert.Contains(StepType.When, stepTypes);
        Assert.Contains(StepType.Then, stepTypes);
    }
}
