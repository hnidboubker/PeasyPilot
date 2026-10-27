namespace PeasyPilot.Core.Assertions;

/// <summary>
/// Framework-free assertions for PeasyPilot's own tests. Failures throw <see cref="AssertionException"/>,
/// which any test framework reports as a failure. Argument order is expected first, actual second, except
/// for the two Contains overloads, which take the item (or substring) first.
/// </summary>
public static class PeasyAssert
{
    /// <summary>
    /// Asserts that two values are equal.
    /// </summary>
    public static void Equal<T>(T expected, T actual, string? message = null) =>
        new AssertThat<T>(actual, message).IsEqualTo(expected);

    /// <summary>
    /// Asserts that two values are not equal.
    /// </summary>
    public static void NotEqual<T>(T unexpected, T actual, string? message = null) =>
        new AssertThat<T>(actual, message).IsNotEqualTo(unexpected);

    /// <summary>
    /// Asserts that a condition is true.
    /// </summary>
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new AssertionException(message ?? "Expected true, but was false.");
        }
    }

    /// <summary>
    /// Asserts that a condition is false.
    /// </summary>
    public static void False(bool condition, string? message = null)
    {
        if (condition)
        {
            throw new AssertionException(message ?? "Expected false, but was true.");
        }
    }

    /// <summary>
    /// Asserts that a value is null.
    /// </summary>
    public static void Null(object? value, string? message = null) =>
        new AssertThat<object?>(value, message).IsNull();

    /// <summary>
    /// Asserts that a value is not null.
    /// </summary>
    public static void NotNull(object? value, string? message = null) =>
        new AssertThat<object?>(value, message).IsNotNull();

    /// <summary>
    /// Asserts that a collection contains an item.
    /// </summary>
    public static void Contains<T>(T item, IEnumerable<T> collection, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (!collection.Contains(item))
        {
            throw new AssertionException(message ?? $"Expected the collection to contain {item}.");
        }
    }

    /// <summary>
    /// Asserts that a text contains a substring.
    /// </summary>
    public static void Contains(string substring, string text, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(substring);
        ArgumentNullException.ThrowIfNull(text);
        if (!text.Contains(substring, StringComparison.Ordinal))
        {
            throw new AssertionException(message ?? $"Expected \"{text}\" to contain \"{substring}\".");
        }
    }

    /// <summary>
    /// Asserts that a collection is empty.
    /// </summary>
    public static void IsEmpty<T>(IEnumerable<T> collection, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (collection.Any())
        {
            throw new AssertionException(message ?? "Expected an empty collection.");
        }
    }

    /// <summary>
    /// Asserts that a collection has at least one item.
    /// </summary>
    public static void NotEmpty<T>(IEnumerable<T> collection, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (!collection.Any())
        {
            throw new AssertionException(message ?? "Expected a non-empty collection.");
        }
    }

    /// <summary>
    /// Asserts that a collection has exactly one item, and returns it.
    /// </summary>
    public static T Single<T>(IEnumerable<T> collection, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(collection);
        var items = collection.Take(2).ToList();
        if (items.Count != 1)
        {
            throw new AssertionException(message ?? "Expected exactly one item in the collection.");
        }

        return items[0];
    }

    /// <summary>
    /// Asserts that an action throws an exception of the given type, and returns it.
    /// </summary>
    public static TException Throws<TException>(Action code, string? message = null) where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(code);
        try
        {
            code();
        }
        catch (TException expected)
        {
            return expected;
        }
        catch (Exception other)
        {
            throw new AssertionException(message ?? $"Expected {typeof(TException).Name}, but {other.GetType().Name} was thrown.", other);
        }

        throw new AssertionException(message ?? $"Expected {typeof(TException).Name}, but nothing was thrown.");
    }

    /// <summary>
    /// Asserts that an async action throws an exception of the given type, and returns it.
    /// </summary>
    public static async Task<TException> ThrowsAsync<TException>(Func<Task> code, string? message = null) where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(code);
        try
        {
            await code();
        }
        catch (TException expected)
        {
            return expected;
        }
        catch (Exception other)
        {
            throw new AssertionException(message ?? $"Expected {typeof(TException).Name}, but {other.GetType().Name} was thrown.", other);
        }

        throw new AssertionException(message ?? $"Expected {typeof(TException).Name}, but nothing was thrown.");
    }
}
