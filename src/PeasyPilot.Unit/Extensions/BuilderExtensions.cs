namespace PeasyPilot.Unit.Extensions;

using PeasyPilot.Unit.Builders;

/// <summary>
/// Extension methods for builder pattern operations.
/// </summary>
public static class BuilderExtensions
{
    /// <summary>
    /// Chains multiple builder configurations together.
    /// </summary>
    /// <typeparam name="T">The type being built.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="configure">Configuration action.</param>
    /// <returns>The builder instance for chaining.</returns>
    public static BuilderBase<T> Configure<T>(
        this BuilderBase<T> builder,
        Action<BuilderBase<T>> configure) where T : class
    {
        configure(builder);
        return builder;
    }

    /// <summary>
    /// Chains multiple builder configurations using a fluent API.
    /// </summary>
    /// <typeparam name="T">The type being built.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="configure">Async configuration action.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ConfigureAsync<T>(
        this BuilderBase<T> builder,
        Func<BuilderBase<T>, Task> configure) where T : class
    {
        await configure(builder);
    }
}
