namespace PeasyPilot.BDD.StepDefinitions;

/// <summary>
/// Resolves step text to step definition methods via reflection and pattern matching.
/// </summary>
public interface IStepBindingResolver
{
    /// <summary>
    /// Resolves a step text to an execution action by matching against registered step definitions.
    /// </summary>
    /// <param name="stepType">The type of step (Given, When, Then, And, But).</param>
    /// <param name="stepText">The step text to match.</param>
    /// <param name="serviceProvider">The DI container for resolving step definition instances.</param>
    /// <returns>A task-based action that executes the matched step, or null if no match found.</returns>
    Func<Task>? ResolveStep(StepType stepType, string stepText, IServiceProvider serviceProvider);

    /// <summary>
    /// Registers a step definition class to discover step bindings.
    /// </summary>
    /// <param name="stepDefinitionType">The type that implements BddStepDefinition.</param>
    void RegisterStepDefinition(Type stepDefinitionType);
}
