using System.Text;

namespace PeasyPilot.BDD;

/// <summary>
/// Exporter for generating Living Documentation (Markdown specifications) from BDD features.
/// </summary>
public static class LivingDocExporter
{
    /// <summary>
    /// Export a collection of features as a Markdown Living Documentation document.
    /// </summary>
    /// <param name="features">Collection of features.</param>
    /// <returns>Markdown formatted living documentation string.</returns>
    public static string ExportToMarkdown(IEnumerable<Feature> features)
    {
        ArgumentNullException.ThrowIfNull(features);

        var sb = new StringBuilder();
        sb.AppendLine("# 📖 PeasyPilot Living Documentation");
        sb.AppendLine($"*Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC*");
        sb.AppendLine();

        foreach (var feature in features)
        {
            sb.AppendLine($"## Feature: {feature.Name}");
            sb.AppendLine();

            foreach (var scenario in feature.Scenarios)
            {
                sb.AppendLine($"### Scenario: {scenario.Name}");
                sb.AppendLine("```gherkin");
                foreach (var step in scenario.Steps)
                {
                    sb.AppendLine($"{step.Type} {step.Text}");
                }
                sb.AppendLine("```");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}
