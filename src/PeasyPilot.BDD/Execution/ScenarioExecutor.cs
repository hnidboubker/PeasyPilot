namespace PeasyPilot.BDD.Execution;

/// <summary>
/// Executes BDD scenarios step-by-step with error handling.
/// </summary>
public class ScenarioExecutor : IScenarioExecutor
{
    /// <inheritdoc />
    public async Task<ScenarioExecutionResult> ExecuteAsync(
        Scenario scenario,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var result = new ScenarioExecutionResult
        {
            ScenarioName = scenario.Name,
            Status = ScenarioStatus.Passed,
            Steps = new List<StepExecutionResult>()
        };

        var startTime = DateTime.UtcNow;
        var stepResults = new List<StepExecutionResult>();

        try
        {
            foreach (var step in scenario.Steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stepResult = new StepExecutionResult
                {
                    Text = step.Text,
                    Type = step.Type,
                    Passed = true
                };

                var stepStartTime = DateTime.UtcNow;

                try
                {
                    // Execute step
                    await step.ExecuteAsync();

                    // Validate step (for Then steps)
                    var validationPassed = step.ExecuteValidation();
                    if (!validationPassed)
                    {
                        stepResult.Passed = false;
                        stepResult.Error = "Step validation failed";
                        result.Status = ScenarioStatus.Failed;
                    }
                }
                catch (Exception ex)
                {
                    stepResult.Passed = false;
                    stepResult.Error = ex.Message;
                    result.Status = ScenarioStatus.Failed;
                }

                stepResult.Duration = DateTime.UtcNow - stepStartTime;
                stepResults.Add(stepResult);

                // Stop on first failure
                if (!stepResult.Passed)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            result.Status = ScenarioStatus.Skipped;
            result.ErrorMessage = "Execution cancelled";
        }
        catch (Exception ex)
        {
            result.Status = ScenarioStatus.Failed;
            result.ErrorMessage = ex.Message;
        }

        result.Duration = DateTime.UtcNow - startTime;
        ((List<StepExecutionResult>)result.Steps).AddRange(stepResults);

        return result;
    }
}
