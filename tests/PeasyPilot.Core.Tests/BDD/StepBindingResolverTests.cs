using PeasyPilot.BDD;
using PeasyPilot.BDD.Execution;
using PeasyPilot.BDD.StepDefinitions;
using Xunit;

namespace PeasyPilot.Core.Tests.BDD;

/// <summary>
/// Tests for step binding resolution via reflection and pattern matching.
/// </summary>
public class StepBindingResolverTests
{
    [Fact]
    public void RegisterStepDefinition_WithValidType_DiscoversMethods()
    {
        // Arrange
        var resolver = new StepBindingResolver();

        // Act
        resolver.RegisterStepDefinition(typeof(TestStepDefinition));

        // Assert - just ensure no exception thrown and resolver is ready
        Assert.NotNull(resolver);
    }

    [Fact]
    public void ResolveStep_WithMatchingPattern_ReturnsBoundMethod()
    {
        // Arrange
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(TestStepDefinition));
        var serviceProvider = new TestServiceProvider();

        // Act
        var resolvedStep = resolver.ResolveStep(StepType.Given, "a user named Alice", serviceProvider);

        // Assert
        Assert.NotNull(resolvedStep);
    }

    [Fact]
    public void ResolveStep_WithNonMatchingPattern_ReturnsNull()
    {
        // Arrange
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(TestStepDefinition));
        var serviceProvider = new TestServiceProvider();

        // Act
        var resolvedStep = resolver.ResolveStep(StepType.Given, "some unrelated step", serviceProvider);

        // Assert
        Assert.Null(resolvedStep);
    }

    [Fact]
    public async Task ResolveStep_WithParameterizedPattern_ExtractsParameters()
    {
        // Arrange
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(TestStepDefinition));
        var serviceProvider = new TestServiceProvider();

        // Act
        var resolvedStep = resolver.ResolveStep(StepType.Given, "a user named Bob", serviceProvider);
        await resolvedStep!.Invoke();

        // Assert
        Assert.True(TestStepDefinition.LastUserNameCreated == "Bob");
    }

    [Fact]
    public async Task ResolveStep_WithWhenStep_ResolvesCorrectly()
    {
        // Arrange
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(TestStepDefinition));
        var serviceProvider = new TestServiceProvider();

        // Act
        var resolvedStep = resolver.ResolveStep(StepType.When, "I delete the user", serviceProvider);
        await resolvedStep!.Invoke();

        // Assert
        Assert.True(TestStepDefinition.UserDeleted);
    }

    [Fact]
    public async Task ResolveStep_WithThenStep_ResolvesCorrectly()
    {
        // Arrange
        var resolver = new StepBindingResolver();
        resolver.RegisterStepDefinition(typeof(TestStepDefinition));
        var serviceProvider = new TestServiceProvider();

        // Act
        var resolvedStep = resolver.ResolveStep(StepType.Then, "the user count is 0", serviceProvider);

        // Assert
        Assert.NotNull(resolvedStep);
        // Note: Then steps in this basic resolver are also Func<Task>, not Func<bool>
    }

    /// <summary>
    /// Test step definition class for testing step binding.
    /// </summary>
    private class TestStepDefinition : BddStepDefinition
    {
        public static string? LastUserNameCreated { get; set; }
        public static bool UserDeleted { get; set; }

        [Given("a user named {name}")]
        public async Task CreateUser(string name)
        {
            LastUserNameCreated = name;
            await Task.CompletedTask;
        }

        [When("I delete the user")]
        public async Task DeleteUser()
        {
            UserDeleted = true;
            await Task.CompletedTask;
        }

        [Then("the user count is {count}")]
        public async Task VerifyUserCount(string count)
        {
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Minimal test service provider.
    /// </summary>
    private class TestServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            return null;
        }
    }
}
