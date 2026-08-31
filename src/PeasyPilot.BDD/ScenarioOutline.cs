namespace PeasyPilot.BDD;

/// <summary>
/// Represents a Gherkin Scenario Outline that expands into multiple parameterized scenarios from an example table.
/// </summary>
public sealed class ScenarioOutline
{
    /// <summary>
    /// Gets the scenario outline name template.
    /// </summary>
    public string NameTemplate { get; }

    private readonly List<(string GivenText, string WhenText, string ThenText)> _stepTemplates = new();
    private readonly List<IReadOnlyDictionary<string, string>> _examples = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioOutline"/> class.
    /// </summary>
    /// <param name="nameTemplate">Scenario name template.</param>
    public ScenarioOutline(string nameTemplate)
    {
        NameTemplate = nameTemplate ?? throw new ArgumentNullException(nameof(nameTemplate));
    }

    /// <summary>
    /// Adds step templates to the outline.
    /// </summary>
    /// <param name="given">Given step template.</param>
    /// <param name="when">When step template.</param>
    /// <param name="then">Then step template.</param>
    /// <returns>This scenario outline for chaining.</returns>
    public ScenarioOutline AddSteps(string given, string when, string then)
    {
        _stepTemplates.Add((given, when, then));
        return this;
    }

    /// <summary>
    /// Adds a row of example parameters.
    /// </summary>
    /// <param name="parameters">Dictionary of placeholder parameter key-values.</param>
    /// <returns>This scenario outline for chaining.</returns>
    public ScenarioOutline AddExample(IReadOnlyDictionary<string, string> parameters)
    {
        _examples.Add(parameters);
        return this;
    }

    /// <summary>
    /// Expands the outline into a list of concrete <see cref="Scenario"/> instances.
    /// </summary>
    /// <returns>List of expanded scenarios.</returns>
    public IReadOnlyCollection<Scenario> Expand()
    {
        var scenarios = new List<Scenario>();
        var index = 1;

        foreach (var example in _examples)
        {
            var scenarioName = ReplacePlaceholders(NameTemplate, example) + $" (Example #{index++})";
            var scenario = new Scenario(scenarioName);

            foreach (var (given, when, then) in _stepTemplates)
            {
                scenario.Given(ReplacePlaceholders(given, example));
                scenario.When(ReplacePlaceholders(when, example));
                scenario.Then(ReplacePlaceholders(then, example));
            }

            scenarios.Add(scenario);
        }

        return scenarios;
    }

    private static string ReplacePlaceholders(string template, IReadOnlyDictionary<string, string> parameters)
    {
        var result = template;
        foreach (var (key, value) in parameters)
        {
            result = result.Replace($"<{key}>", value);
        }
        return result;
    }
}
