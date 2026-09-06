namespace PeasyPilot.BDD.StepDefinitions;

/// <summary>
/// Attribute for Given step definitions.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class GivenAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the GivenAttribute class.
    /// </summary>
    /// <param name="pattern">Step text pattern (e.g., "a user with {name}").</param>
    public GivenAttribute(string pattern)
    {
        Pattern = pattern;
    }

    /// <summary>
    /// Gets the step text pattern.
    /// </summary>
    public string Pattern { get; }
}

/// <summary>
/// Attribute for When step definitions.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class WhenAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the WhenAttribute class.
    /// </summary>
    /// <param name="pattern">Step text pattern (e.g., "I do {action}").</param>
    public WhenAttribute(string pattern)
    {
        Pattern = pattern;
    }

    /// <summary>
    /// Gets the step text pattern.
    /// </summary>
    public string Pattern { get; }
}

/// <summary>
/// Attribute for Then step definitions.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ThenAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the ThenAttribute class.
    /// </summary>
    /// <param name="pattern">Step text pattern (e.g., "the result is {expected}").</param>
    public ThenAttribute(string pattern)
    {
        Pattern = pattern;
    }

    /// <summary>
    /// Gets the step text pattern.
    /// </summary>
    public string Pattern { get; }
}

/// <summary>
/// Attribute for And step definitions.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AndAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the AndAttribute class.
    /// </summary>
    /// <param name="pattern">Step text pattern (e.g., "also {action}").</param>
    public AndAttribute(string pattern)
    {
        Pattern = pattern;
    }

    /// <summary>
    /// Gets the step text pattern.
    /// </summary>
    public string Pattern { get; }
}

/// <summary>
/// Attribute for But step definitions.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ButAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the ButAttribute class.
    /// </summary>
    /// <param name="pattern">Step text pattern (e.g., "not {condition}").</param>
    public ButAttribute(string pattern)
    {
        Pattern = pattern;
    }

    /// <summary>
    /// Gets the step text pattern.
    /// </summary>
    public string Pattern { get; }
}
