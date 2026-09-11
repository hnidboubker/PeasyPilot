namespace PeasyPilot.XUnit;

using global::Xunit;

/// <summary>
/// Framework-specific Assert alias for xUnit.
/// Provides explicit clarity when using xUnit assertions.
/// </summary>
public static class XAssert
{
    public static void Equal<T>(T expected, T actual) =>
        Assert.Equal(expected, actual);

    public static void NotEqual<T>(T expected, T actual) =>
        Assert.NotEqual(expected, actual);

    public static void True(bool condition) =>
        Assert.True(condition);

    public static void False(bool condition) =>
        Assert.False(condition);

    public static void Null(object? obj) =>
        Assert.Null(obj);

    public static void NotNull(object? obj) =>
        Assert.NotNull(obj);

    public static void Contains<T>(T item, IEnumerable<T> collection) =>
        Assert.Contains(item, collection);

    public static void DoesNotContain<T>(T item, IEnumerable<T> collection) =>
        Assert.DoesNotContain(item, collection);

    public static void Throws<TException>(Action code) where TException : Exception =>
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
}
