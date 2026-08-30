
namespace PeasyPilot.Core.Extensions;
/// <summary>
/// Entry point for fluent assertions.
/// </summary>
public static class Assert
{
    /// <summary>
    /// Begins a fluent assertion on the given value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="actual">The actual value.</param>
    /// <param name="message">Optional assertion message.</param>
    /// <returns>A new assertion builder.</returns>
    public static AssertThat<T> That<T>(T actual, string? message = null)
        => new(actual, message);
}