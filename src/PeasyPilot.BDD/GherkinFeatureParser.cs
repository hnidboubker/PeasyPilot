namespace PeasyPilot.BDD;

/// <summary>
/// Native Gherkin feature text parser converting Gherkin specifications into PeasyPilot <see cref="Feature"/> instances.
/// </summary>
public static class GherkinFeatureParser
{
    /// <summary>
    /// Parses a Gherkin specification string into a <see cref="Feature"/> object graph.
    /// </summary>
    /// <param name="gherkinContent">Gherkin text content.</param>
    /// <returns>Parsed Feature instance.</returns>
    public static Feature Parse(string gherkinContent)
    {
        ArgumentNullException.ThrowIfNull(gherkinContent);

        var lines = gherkinContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Feature? currentFeature = null;
        Scenario? currentScenario = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("Feature:", StringComparison.OrdinalIgnoreCase))
            {
                var featureName = line["Feature:".Length..].Trim();
                currentFeature = new Feature(featureName);
            }
            else if (line.StartsWith("Scenario:", StringComparison.OrdinalIgnoreCase) && currentFeature != null)
            {
                var scenarioName = line["Scenario:".Length..].Trim();
                currentScenario = currentFeature.AddScenario(scenarioName);
            }
            else if (currentScenario != null)
            {
                if (line.StartsWith("Given ", StringComparison.OrdinalIgnoreCase))
                {
                    currentScenario.Given(line["Given ".Length..].Trim());
                }
                else if (line.StartsWith("When ", StringComparison.OrdinalIgnoreCase))
                {
                    currentScenario.When(line["When ".Length..].Trim());
                }
                else if (line.StartsWith("Then ", StringComparison.OrdinalIgnoreCase))
                {
                    currentScenario.Then(line["Then ".Length..].Trim());
                }
                else if (line.StartsWith("And ", StringComparison.OrdinalIgnoreCase))
                {
                    currentScenario.And(line["And ".Length..].Trim());
                }
            }
        }

        return currentFeature ?? new Feature("Untitled Feature");
    }
}
