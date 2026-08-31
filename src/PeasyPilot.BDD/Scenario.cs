namespace PeasyPilot.BDD;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

/// <summary>
/// Represents a Gherkin scenario step.
/// </summary>
public enum StepType
{
    /// <summary>
    /// Given step - setup context.
    /// </summary>
    Given,

    /// <summary>
    /// When step - action.
    /// </summary>
    When,

    /// <summary>
    /// Then step - assertion.
    /// </summary>
    Then,

    /// <summary>
    /// And step - continuation.
    /// </summary>
    And,

    /// <summary>
    /// But step - negation.
    /// </summary>
    But
}

/// <summary>
/// Represents a BDD scenario step with execution logic.
/// </summary>
public class Step
{
    /// <summary>
    /// Gets the step type.
    /// </summary>
    public StepType Type { get; }

    /// <summary>
    /// Gets the step text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets or sets the execution action.
    /// </summary>
    public Func<Task>? Execute { get; set; }

    /// <summary>
    /// Gets or sets the validation function.
    /// </summary>
    public Func<bool>? Validate { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Step"/> class.
    /// </summary>
    /// <param name="type">The step type.</param>
    /// <param name="text">The step text.</param>
    public Step(StepType type, string text)
    {
        Type = type;
        Text = text;
    }

    /// <summary>
    /// Executes the step asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        if (Execute != null)
        {
            await Execute();
        }
    }

    /// <summary>
    /// Validates the step result.
    /// </summary>
    /// <returns>True if validation passes; otherwise, false.</returns>
    public bool ExecuteValidation()
    {
        return Validate?.Invoke() ?? true;
    }
}

/// <summary>
/// Represents a complete BDD scenario.
/// </summary>
public class Scenario
{
    /// <summary>
    /// Gets the scenario name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the steps.
    /// </summary>
    public IReadOnlyList<Step> Steps { get; private set; }

    private readonly List<Step> _steps = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Scenario"/> class.
    /// </summary>
    /// <param name="name">The scenario name.</param>
    public Scenario(string name)
    {
        Name = name;
        Steps = _steps.AsReadOnly();
    }

    /// <summary>
    /// Adds a Given step.
    /// </summary>
    /// <param name="text">The step text.</param>
    /// <param name="execute">The execution action.</param>
    /// <returns>This scenario for chaining.</returns>
    public Scenario Given(string text, Func<Task>? execute = null)
    {
        var step = new Step(StepType.Given, text) { Execute = execute };
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Adds a When step.
    /// </summary>
    /// <param name="text">The step text.</param>
    /// <param name="execute">The execution action.</param>
    /// <returns>This scenario for chaining.</returns>
    public Scenario When(string text, Func<Task>? execute = null)
    {
        var step = new Step(StepType.When, text) { Execute = execute };
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Adds a Then step.
    /// </summary>
    /// <param name="text">The step text.</param>
    /// <param name="validate">The validation function.</param>
    /// <returns>This scenario for chaining.</returns>
    public Scenario Then(string text, Func<bool>? validate = null)
    {
        var step = new Step(StepType.Then, text) { Validate = validate };
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Adds an And step.
    /// </summary>
    /// <param name="text">The step text.</param>
    /// <param name="execute">The execution action.</param>
    /// <returns>This scenario for chaining.</returns>
    public Scenario And(string text, Func<Task>? execute = null)
    {
        var step = new Step(StepType.And, text) { Execute = execute };
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Executes the scenario asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync()
    {
        foreach (var step in _steps)
        {
            await step.ExecuteAsync();
        }
    }

    /// <summary>
    /// Validates all scenario steps.
    /// </summary>
    /// <returns>True if all validations pass; otherwise, false.</returns>
    public bool Validate()
    {
        return _steps.All(s => s.ExecuteValidation());
    }

    /// <summary>
    /// Gets a string representation of the scenario in Gherkin format.
    /// </summary>
    /// <returns>The Gherkin representation.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Scenario: {Name}");

        foreach (var step in _steps)
        {
            sb.AppendLine($"  {step.Type} {step.Text}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts this scenario into a unified PeasyPilot <see cref="TestCase"/>.
    /// </summary>
    /// <param name="featureName">Optional parent feature name.</param>
    /// <returns>A unified test case instance with Kind = TestKind.Bdd.</returns>
    public TestCase ToTestCase(string? featureName = null)
    {
        return new TestCase
        {
            Name = string.IsNullOrEmpty(featureName) ? Name : $"{featureName} - {Name}",
            Category = string.IsNullOrEmpty(featureName) ? "BDD" : featureName,
            Kind = TestKind.Bdd,
            Metadata = new Dictionary<string, string>
            {
                ["Gherkin"] = ToString(),
                ["StepCount"] = _steps.Count.ToString()
            }
        };
    }

    /// <summary>
    /// Executes the scenario and returns a unified PeasyPilot <see cref="TestResult"/>.
    /// </summary>
    /// <param name="featureName">Optional parent feature name.</param>
    /// <returns>The unified execution result.</returns>
    public async Task<TestResult> ExecuteAndAsTestResultAsync(string? featureName = null)
    {
        var testCase = ToTestCase(featureName);
        var start = DateTime.UtcNow;

        try
        {
            await ExecuteAsync();
            var valid = Validate();

            return new TestResult
            {
                Name = testCase.Name,
                Category = testCase.Category,
                Status = valid ? TestRunStatus.Passed : TestRunStatus.Failed,
                Duration = DateTime.UtcNow - start,
                Message = valid ? "Scenario validation passed." : "Scenario validation failed.",
                Failure = valid ? null : new TestFailure
                {
                    Message = "One or more Then step validations returned false.",
                    Expected = "All steps valid (true)",
                    Actual = "Validation failed"
                }
            };
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                Name = testCase.Name,
                Category = testCase.Category,
                Status = TestRunStatus.Failed,
                Duration = DateTime.UtcNow - start,
                Message = ex.Message,
                Failure = new TestFailure
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                }
            };
        }
    }
}
