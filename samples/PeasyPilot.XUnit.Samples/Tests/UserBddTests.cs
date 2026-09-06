namespace PeasyPilot.XUnit.Samples.Tests;

using PeasyPilot.BDD;
using PeasyPilot.BDD.Execution;
using PeasyPilot.BDD.FileLoading;
using Xunit;

/// <summary>
/// BDD tests for user management using feature files.
/// </summary>
public class UserBddTests
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
        return Path.Combine(solutionRoot, "samples/PeasyPilot.XUnit.Samples/features/users.feature");
    }

    [Fact]
    public async Task UserFeatures_LoadSuccessfully()
    {
        // Arrange
        var featurePath = GetFeaturePath();

        // Act
        var feature = await _loader.LoadFromFileAsync(featurePath);

        // Assert
        Assert.NotNull(feature);
        Assert.Equal("User Management", feature.Name);
        Assert.Equal(4, feature.Scenarios.Count);
    }

    [Fact]
    public async Task UserFeatures_ScenariosExist()
    {
        // Arrange
        var featurePath = GetFeaturePath();
        var feature = await _loader.LoadFromFileAsync(featurePath);

        // Assert
        var scenarioNames = feature.Scenarios.Select(s => s.Name).ToList();
        Assert.Contains("Create a new user", scenarioNames);
        Assert.Contains("Retrieve user by ID", scenarioNames);
        Assert.Contains("Multiple users isolation", scenarioNames);
        Assert.Contains("Delete user", scenarioNames);
    }

    [Fact]
    public async Task CreateUserScenario_HasCorrectSteps()
    {
        // Arrange
        var featurePath = GetFeaturePath();
        var feature = await _loader.LoadFromFileAsync(featurePath);
        var scenario = feature.Scenarios.First(s => s.Name == "Create a new user");

        // Assert
        Assert.True(scenario.Steps.Count >= 3, "Scenario should have at least 3 steps");

        var stepTypes = scenario.Steps.Select(s => s.Type).ToList();
        Assert.Contains(StepType.Given, stepTypes);
        Assert.Contains(StepType.When, stepTypes);
        Assert.Contains(StepType.Then, stepTypes);
    }
}
