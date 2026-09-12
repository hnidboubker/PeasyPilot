namespace PeasyPilot.XUnit;

using System.Linq.Expressions;

using global::Xunit;

/// <summary>
/// Strongly-typed assertion builder for xUnit.
/// Provides chainable sync assertions with compile-time type safety.
/// </summary>
public class XAssertThat<T>
{
    private readonly T _value;

    /// <summary>
    /// Initializes a new instance of the XAssertThat builder.
    /// </summary>
    public XAssertThat(T value) => _value = value;

    /// <summary>
    /// Asserts that the value equals the expected value.
    /// </summary>
    public void IsEqualTo(T expected) =>
        Assert.Equal(expected, _value);

    /// <summary>
    /// Asserts that the value does not equal the expected value.
    /// </summary>
    public void IsNotEqualTo(T expected) =>
        Assert.NotEqual(expected, _value);

    /// <summary>
    /// Asserts that the value is null.
    /// </summary>
    public void IsNull() =>
        Assert.Null(_value);

    /// <summary>
    /// Asserts that the value is not null.
    /// </summary>
    public void IsNotNull() =>
        Assert.NotNull(_value);

    /// <summary>
    /// Asserts that the collection is empty.
    /// </summary>
    //public void IsEmpty() =>
    //    Assert.Empty((IEnumerable)_value!);

    ///// <summary>
    ///// Asserts that the collection is not empty.
    ///// </summary>
    //public void IsNotEmpty() =>
    //    Assert.NotEmpty((IEnumerable)_value!);

    /// <summary>
    /// Asserts that the value is of the specified type.
    /// </summary>
    public void IsOfType<TTarget>() =>
        Assert.IsType<TTarget>(_value);

    /// <summary>
    /// Asserts that the value is assignable to the specified type.
    /// </summary>
    public void IsAssignableTo<TTarget>() =>
        Assert.IsAssignableFrom<TTarget>(_value);

    /// <summary>
    /// Asserts that the value is not of the specified type.
    /// </summary>
    public void IsNotOfType<TTarget>() =>
        Assert.IsNotType<TTarget>(_value);
}

/// <summary>
/// Framework-specific Assert alias for xUnit.
/// Provides explicit clarity when using xUnit assertions.
/// </summary>
public static class XAssert
{
    /// <summary>
    /// Creates a strongly-typed assertion builder for the specified value.
    /// </summary>
    public static XAssertThat<T> That<T>(T value) =>
        new(value);

    public static void All<T>(IEnumerable<T> collection, Action<T> action) =>
    Assert.All(collection, action);

    public static void Equal<T>(T expected, T actual) =>
        Assert.Equal(expected, actual);

    public static void NotEqual<T>(T expected, T actual) =>
        Assert.NotEqual(expected, actual);

    public static void True(this bool condition) =>
        Assert.True(condition);

    public static void True(this bool condition, Expression<Func<string>> messageExpression) =>
      Assert.True(condition, messageExpression.Compile()());

    public static void True(bool condition, string message) =>
     Assert.True(condition, message);

    public static void False(bool condition) =>
        Assert.False(condition);

    public static void Null(object? obj) =>
        Assert.Null(obj);

    public static void NotNull(object? obj) =>
        Assert.NotNull(obj);

    public static void Contains<T>(this IEnumerable<T> collection, T item) =>
    Assert.Contains(item, collection);

    public static void Contains<T>(IEnumerable<T> collection, Expression<Func<T, bool>> predicate)
    {
        var func = predicate.Compile();
        var message = $"No item in collection matches: {predicate}";
        Assert.True(collection.Any(func), message);
    }

    public static void Contains<T>(T item, IEnumerable<T> collection) =>
        Assert.Contains(item, collection);

    public static void Contains(this string text, string substring) =>
       Assert.Contains(substring, text);

    public static void DoesNotContain<T>(T item, IEnumerable<T> collection) =>
        Assert.DoesNotContain(item, collection);

    public static void DoesNotContain(this string text, string substring) =>
       Assert.DoesNotContain(substring, text);

    public static TException Throws<TException>(Action code) where TException : Exception =>
        Assert.Throws<TException>(code);

    public static async Task ThrowsAsync<TException>(Func<Task> code) where TException : Exception =>
        await Assert.ThrowsAsync<TException>(code);

    public static void Greater<T>(T expected, T actual) where T : IComparable =>
        Assert.True(((IComparable)actual).CompareTo(expected) > 0);

    public static void Less<T>(T expected, T actual) where T : IComparable =>
        Assert.True(((IComparable)actual).CompareTo(expected) < 0);

    public static void IsEmpty<T>(IEnumerable<T> collection) =>
        Assert.Empty(collection);

    public static void IsNotEmpty<T>(IEnumerable<T> collection) =>
        Assert.NotEmpty(collection);

    public static void NotEmpty<T>(IEnumerable<T> collection) =>
        Assert.NotEmpty(collection);

    public static void IsType<T>(object? obj) =>
        Assert.IsType<T>(obj);

    public static void IsAssignableFrom<T>(object? obj) =>
        Assert.IsAssignableFrom<T>(obj);

    public static void IsNotType<T>(object? obj) =>
        Assert.IsNotType<T>(obj);

    public static T Single<T>(IEnumerable<T> collection) =>
        Assert.Single(collection);
}
