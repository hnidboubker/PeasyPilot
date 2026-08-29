namespace PeasyPilot.Unit.Builders;

/// <summary>
/// Base class for building test objects using the builder pattern.
/// </summary>
/// <typeparam name="T">The type of object being built.</typeparam>
public abstract class BuilderBase<T> where T : class
{
    /// <summary>
    /// Gets or sets the instance being built.
    /// </summary>
    protected T Instance { get; set; } = Activator.CreateInstance<T>()!;

    /// <summary>
    /// Builds and returns the instance.
    /// </summary>
    /// <returns>The built instance.</returns>
    public virtual T Build() => Instance;

    /// <summary>
    /// Resets the builder to a fresh state.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public virtual BuilderBase<T> Reset()
    {
        Instance = Activator.CreateInstance<T>()!;
        return this;
    }
}
