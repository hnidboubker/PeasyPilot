using PeasyPilot.BDD.StepDefinitions;

namespace PeasyPilot.BDD.Execution;

/// <summary>
/// Executes BDD scenarios step-by-step with error handling and step binding resolution.
/// </summary>
public class ScenarioExecutor : IScenarioExecutor
{
    private readonly IStepBindingResolver? _resolver;

    /// <summary>
    /// Initializes a new instance of ScenarioExecutor with optional step binding resolver.
    /// </summary>
    /// <param name="resolver">Optional resolver for step bindings. If null, uses step text directly.</param>
    public ScenarioExecutor(IStepBindingResolver? resolver = null)
    {
        _resolver = resolver;
    }
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
                    // Resolve step binding if resolver available and step has no action yet
                    if (_resolver != null && step.Execute == null)
                    {
                        step.Execute = _resolver.ResolveStep(step.Type, step.Text, serviceProvider);
                    }

                    // Execute step
                    if (step.Execute != null)
                    {
                        await step.ExecuteAsync();
                    }
                    else if (_resolver != null)
                    {
                        stepResult.Passed = false;
                        stepResult.Error = $"No step binding found for: {step.Text}";
                        result.Status = ScenarioStatus.Failed;
                    }

                    // Validate step (for Then steps)
                    if (stepResult.Passed && step.ExecuteValidation())
                    {
                        // Validation passed
                    }
                    else if (stepResult.Passed)
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
