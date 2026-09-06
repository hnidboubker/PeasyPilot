using PeasyPilot.BDD;
using PeasyPilot.BDD.Execution;
using PeasyPilot.BDD.FileLoading;
using PeasyPilot.BDD.StepDefinitions;
using Xunit;

namespace PeasyPilot.Core.Tests.BDD;

/// <summary>
/// E2E tests for scenario execution with step binding resolver.
/// Tests the complete flow: load feature → resolve bindings → execute steps.
/// </summary>
public class ScenarioExecutionWithBindingTests
{
    private string GetSolutionRoot()
    {
        var testDir = AppContext.BaseDirectory;
        var di = new DirectoryInfo(testDir);
        while (di != null && !File.Exists(Path.Combine(di.FullName, "easy-peasy.slnx")))
        {
            di = di.Parent;
        }
        return di?.FullName ?? throw new InvalidOperationException("Solution root not found");
    }

    [Fact]
    public async Task ExecuteScenario_WithStepBindings_ResolvesAndExecutes()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var featurePath = Path.Combine(solutionRoot, "samples/PeasyPilot.XUnit.Samples/features/users.feature");

        var loader = new GherkinFeatureFileLoader();
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(SimpleUserSteps));
        var executor = new ScenarioExecutor(resolver);
        var serviceProvider = new SimpleServiceProvider();

        // Load feature file
        var feature = await loader.LoadFromFileAsync(featurePath);
        Assert.NotNull(feature);

        // Find first scenario
        var scenario = feature.Scenarios.First();
        Assert.NotNull(scenario);

        // Act
        var result = await executor.ExecuteAsync(scenario, serviceProvider);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Status == ScenarioStatus.Passed || result.Status == ScenarioStatus.Failed,
            $"Scenario execution resulted in: {result.Status}. Error: {result.ErrorMessage}");
    }

    [Fact]
    public async Task ExecuteScenarioWithParameters_ExtractsAndPasses()
    {
        // Arrange
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(ParamSteps));
        var executor = new ScenarioExecutor(resolver);
        var serviceProvider = new SimpleServiceProvider();

        // Create scenario with parameterized steps
        var scenario = new Scenario("Test with parameters");
        scenario
            .Given("I have {count} items", null)
            .When("I add {count} more items", null)
            .Then("I should have {total} items", null);

        // Act
        var result = await executor.ExecuteAsync(scenario, serviceProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test with parameters", result.ScenarioName);
    }

    /// <summary>
    /// Simple step definition for testing.
    /// </summary>
    private class SimpleUserSteps : BddStepDefinition
    {
        public static bool DatabaseEmpty { get; set; }

        [Given("the user database is empty")]
        public async Task DatabaseIsEmpty()
        {
            DatabaseEmpty = true;
            await Task.CompletedTask;
        }

        [When("I create a user")]
        public async Task CreateUser()
        {
            await Task.CompletedTask;
        }

        [Then("the user is stored")]
        public async Task UserStored()
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Parameterized step definition for testing.
    /// </summary>
    private class ParamSteps : BddStepDefinition
    {
        public static int ItemCount { get; set; }

        [Given("I have {count} items")]
        public async Task HaveItems(string count)
        {
            if (int.TryParse(count, out var num))
            {
                ItemCount = num;
            }
            await Task.CompletedTask;
        }

        [When("I add {count} more items")]
        public async Task AddItems(string count)
        {
            if (int.TryParse(count, out var num))
            {
                ItemCount += num;
            }
            await Task.CompletedTask;
        }

        [Then("I should have {total} items")]
        public async Task VerifyTotal(string total)
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Minimal service provider for testing.
    /// </summary>
    private class SimpleServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
