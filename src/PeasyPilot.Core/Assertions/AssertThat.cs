namespace PeasyPilot.Core.Assertions;

/// <summary>
/// Fluent assertion builder for test assertions.
/// </summary>
/// <typeparam name="T">The type of the value being asserted.</typeparam>
public class AssertThat<T>
{
    private readonly T _actual;
    private readonly string? _message;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssertThat{T}"/> class.
    /// </summary>
    /// <param name="actual">The actual value.</param>
    /// <param name="message">Optional assertion message.</param>
    public AssertThat(T actual, string? message = null)
    {
        _actual = actual;
        _message = message;
    }

    /// <summary>
    /// Asserts that the value is equal to the expected value.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <returns>This assertion instance for chaining.</returns>
    public AssertThat<T> IsEqualTo(T expected)
    {
        if (!Equals(_actual, expected))
        {
            var msg = _message ?? $"Expected {expected}, but got {_actual}";
            throw new AssertionException(msg);
        }
        return this;
    }

    /// <summary>
    /// Asserts that the value is not equal to the expected value.
    /// </summary>
    /// <param name="unexpected">The unexpected value.</param>
    /// <returns>This assertion instance for chaining.</returns>
    public AssertThat<T> IsNotEqualTo(T unexpected)
    {
        if (Equals(_actual, unexpected))
        {
            var msg = _message ?? $"Expected value to not be {unexpected}";
            throw new AssertionException(msg);
        }
        return this;
    }

    /// <summary>
    /// Asserts that the value is null.
    /// </summary>
    /// <returns>This assertion instance for chaining.</returns>
    public AssertThat<T> IsNull()
    {
        if (_actual != null)
        {
            var msg = _message ?? $"Expected null, but got {_actual}";
            throw new AssertionException(msg);
        }
        return this;
    }

    /// <summary>
    /// Asserts that the value is not null.
    /// </summary>
    /// <returns>This assertion instance for chaining.</returns>
    public AssertThat<T> IsNotNull()
    {
        if (_actual == null)
        {
            var msg = _message ?? "Expected non-null value";
            throw new AssertionException(msg);
        }
        return this;
    }
}


