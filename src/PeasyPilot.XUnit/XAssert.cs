namespace PeasyPilot.XUnit;

using System.Linq.Expressions;

using global::Xunit;

/// <summary>
/// Framework-specific Assert alias for xUnit.
/// Provides explicit clarity when using xUnit assertions.
/// </summary>
public static class XAssert
{

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
