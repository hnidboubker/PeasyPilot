namespace PeasyPilot.BDD.Execution;

/// <summary>
/// Executes BDD scenarios with dependency injection support.
/// </summary>
public interface IScenarioExecutor
{
    /// <summary>
    /// Executes a scenario using the provided service provider for step resolution.
    /// </summary>
    /// <param name="scenario">Scenario to execute.</param>
    /// <param name="serviceProvider">Service provider for step definitions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Execution result with step details.</returns>
    Task<ScenarioExecutionResult> ExecuteAsync(
        Scenario scenario,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);
}
