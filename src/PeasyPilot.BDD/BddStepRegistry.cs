using System.Text.RegularExpressions;

namespace PeasyPilot.BDD;

/// <summary>
/// Central registry for binding Gherkin step text patterns to executable delegate actions.
/// </summary>
public sealed class BddStepRegistry
{
    private readonly List<(Regex Pattern, Func<Task> Action)> _asyncSteps = new();

    /// <summary>
    /// Registers an executable step delegate matching a regex pattern.
    /// </summary>
    /// <param name="pattern">Regex pattern matching step text.</param>
    /// <param name="action">Executable async delegate.</param>
    public void RegisterStep(string pattern, Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(action);

        _asyncSteps.Add((new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled), action));
    }

    /// <summary>
    /// Finds a matching registered action for a given step text.
    /// </summary>
    /// <param name="stepText">Step text string.</param>
    /// <returns>Matching async Func or null.</returns>
    public Func<Task>? FindMatch(string stepText)
    {
        foreach (var (pattern, action) in _asyncSteps)
        {
            if (pattern.IsMatch(stepText))
            {
                return action;
            }
        }
        return null;
    }
}
