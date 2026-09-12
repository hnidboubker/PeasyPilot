namespace PeasyPilot.TUnit;

using global::TUnit.Assertions;

/// <summary>
/// Framework-specific Assert alias for TUnit.
/// Provides explicit clarity when using TUnit assertions.
/// </summary>
public static class TAssert
{
#pragma warning disable TUnitAssertions0002 // TUnit analyzer: Assert statements must be awaited
    public static dynamic That<T>(T value) =>
        Assert.That(value);
#pragma warning restore TUnitAssertions0002

    public static async Task Equal<T>(T expected, T actual) =>
        await Assert.That(actual).IsEqualTo(expected);

    public static async Task NotEqual<T>(T expected, T actual) =>
        await Assert.That(actual).IsNotEqualTo(expected);

    public static async Task True(bool condition) =>
        await Assert.That(condition).IsTrue();

    public static async Task False(bool condition) =>
        await Assert.That(condition).IsFalse();

    public static async Task Null(object? obj) =>
        await Assert.That(obj).IsNull();

    public static async Task NotNull(object? obj) =>
        await Assert.That(obj).IsNotNull();

    public static async Task Contains<T>(T item, IEnumerable<T> collection) =>
        await Assert.That(collection).Contains(item);

    public static async Task DoesNotContain<T>(T item, IEnumerable<T> collection) =>
        await Assert.That(collection).DoesNotContain(item);

    public static void Throws<TException>(Action code) where TException : Exception =>
        Assert.Throws<TException>(code);

    public static Task ThrowsAsync<TException>(Func<Task> code) where TException : Exception =>
        Assert.ThrowsAsync<TException>(code);

    public static async Task IsEmpty<T>(IEnumerable<T> collection) =>
        await Assert.That(collection).IsEmpty();

    public static async Task IsNotEmpty<T>(IEnumerable<T> collection) =>
        await Assert.That(collection).IsNotEmpty();

    public static async Task StartsWith(string text, string value) =>
        await Assert.That(text).StartsWith(value);

    public static async Task EndsWith(string text, string value) =>
        await Assert.That(text).EndsWith(value);
}
