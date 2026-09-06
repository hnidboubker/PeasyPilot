using System.Reflection;
using System.Text.RegularExpressions;

namespace PeasyPilot.BDD.StepDefinitions;

/// <summary>
/// Resolves step text to step definition methods using reflection and pattern matching.
/// </summary>
public class StepBindingResolver : IStepBindingResolver
{
    private readonly List<StepBinding> _bindings = new();

    /// <summary>
    /// Represents a resolved step binding (pattern + method).
    /// </summary>
    private class StepBinding
    {
        public StepType StepType { get; set; }
        public Regex Pattern { get; set; } = null!;
        public MethodInfo Method { get; set; } = null!;
        public Type DeclaringType { get; set; } = null!;
        public ParameterInfo[] Parameters { get; set; } = Array.Empty<ParameterInfo>();
    }

    /// <inheritdoc />
    public void RegisterStepDefinition(Type stepDefinitionType)
    {
        ArgumentNullException.ThrowIfNull(stepDefinitionType);

        if (!typeof(BddStepDefinition).IsAssignableFrom(stepDefinitionType))
        {
            throw new ArgumentException(
                $"Type {stepDefinitionType.Name} must inherit from BddStepDefinition",
                nameof(stepDefinitionType));
        }

        // Find all methods with step attributes
        var methods = stepDefinitionType.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var method in methods)
        {
            // Check for [Given] attribute
            var givenAttr = method.GetCustomAttribute<GivenAttribute>();
            if (givenAttr != null)
            {
                _bindings.Add(new StepBinding
                {
                    StepType = StepType.Given,
                    Pattern = PatternToRegex(givenAttr.Pattern),
                    Method = method,
                    DeclaringType = stepDefinitionType,
                    Parameters = method.GetParameters()
                });
            }

            // Check for [When] attribute
            var whenAttr = method.GetCustomAttribute<WhenAttribute>();
            if (whenAttr != null)
            {
                _bindings.Add(new StepBinding
                {
                    StepType = StepType.When,
                    Pattern = PatternToRegex(whenAttr.Pattern),
                    Method = method,
                    DeclaringType = stepDefinitionType,
                    Parameters = method.GetParameters()
                });
            }

            // Check for [Then] attribute
            var thenAttr = method.GetCustomAttribute<ThenAttribute>();
            if (thenAttr != null)
            {
                _bindings.Add(new StepBinding
                {
                    StepType = StepType.Then,
                    Pattern = PatternToRegex(thenAttr.Pattern),
                    Method = method,
                    DeclaringType = stepDefinitionType,
                    Parameters = method.GetParameters()
                });
            }

            // Check for [And] attribute
            var andAttr = method.GetCustomAttribute<AndAttribute>();
            if (andAttr != null)
            {
                _bindings.Add(new StepBinding
                {
                    StepType = StepType.And,
                    Pattern = PatternToRegex(andAttr.Pattern),
                    Method = method,
                    DeclaringType = stepDefinitionType,
                    Parameters = method.GetParameters()
                });
            }

            // Check for [But] attribute
            var butAttr = method.GetCustomAttribute<ButAttribute>();
            if (butAttr != null)
            {
                _bindings.Add(new StepBinding
                {
                    StepType = StepType.But,
                    Pattern = PatternToRegex(butAttr.Pattern),
                    Method = method,
                    DeclaringType = stepDefinitionType,
                    Parameters = method.GetParameters()
                });
            }
        }
    }

    /// <inheritdoc />
    public Func<Task>? ResolveStep(StepType stepType, string stepText, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(stepText);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        // Find first binding that matches step type and pattern
        var binding = _bindings.FirstOrDefault(b =>
            b.StepType == stepType && b.Pattern.IsMatch(stepText));

        if (binding == null)
        {
            return null;
        }

        // Extract parameter values from step text
        var match = binding.Pattern.Match(stepText);
        var parameterValues = ExtractParameters(match, binding.Parameters);

        // Return a task-based action that invokes the method
        return async () =>
        {
            // Try to get instance from DI, fallback to creating new instance
            var instance = serviceProvider.GetService(binding.DeclaringType);
            if (instance == null)
            {
                instance = Activator.CreateInstance(binding.DeclaringType)
                    ?? throw new InvalidOperationException($"Cannot create instance of {binding.DeclaringType.Name}");
            }

            var result = binding.Method.Invoke(instance, parameterValues);

            if (result is Task task)
            {
                await task.ConfigureAwait(false);
            }
        };
    }

    /// <summary>
    /// Converts a step pattern (e.g., "I create a user {name}") to a regex.
    /// </summary>
    private static Regex PatternToRegex(string pattern)
    {
        // First, extract parameter names before escaping
        var parameterMatches = Regex.Matches(pattern, @"\{([^}]+)\}");
        var parameters = parameterMatches.Cast<Match>().Select(m => m.Groups[1].Value).ToList();

        // Escape special regex characters but preserve placeholders temporarily
        var regexPattern = pattern;
        foreach (var param in parameters)
        {
            regexPattern = regexPattern.Replace($"{{{param}}}", $"<<{param}>>");
        }
        regexPattern = Regex.Escape(regexPattern);

        // Replace temporary placeholders with named capture groups
        foreach (var param in parameters)
        {
            regexPattern = regexPattern.Replace($"<<{param}>>", $@"(?<{param}>[^\""\\n]+?)");
        }

        return new Regex($"^{regexPattern}$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Extracts parameter values from regex match groups.
    /// </summary>
    private static object?[] ExtractParameters(Match match, ParameterInfo[] parameters)
    {
        var values = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var paramName = parameters[i].Name;
            if (string.IsNullOrEmpty(paramName))
            {
                continue;
            }

            if (!match.Groups.ContainsKey(paramName))
            {
                continue;
            }

            var groupValue = match.Groups[paramName].Value;
            var paramType = parameters[i].ParameterType;

            // Convert string to target parameter type
            values[i] = Convert.ChangeType(groupValue, paramType);
        }

        return values;
    }
}
