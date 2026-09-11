namespace PeasyPilot.TUnit;

using global::TUnit.Assertions;
using global::TUnit.Assertions.Extensions;

/// <summary>
/// Framework-specific Assert alias for TUnit.
/// Provides explicit clarity when using TUnit assertions.
/// </summary>
public static class TAssert
{
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
        await Assert.That(collection).ContainsItem(item);

    public static async Task DoesNotContain<T>(T item, IEnumerable<T> collection) =>
        await Assert.That(collection).DoesNotContainItem(item);

    public static async Task Throws<TException>(Func<Task> code) where TException : Exception =>
        await Assert.Throws<TException>(code);

    public static async Task ThrowsAsync<TException>(Func<Task> code) where TException : Exception =>
        await Assert.ThrowsAsync<TException>(code);

    public static async Task Greater<T>(T expected, T actual) where T : IComparable =>
        await Assert.That(((IComparable)actual).CompareTo(expected)).IsGreaterThan(0);

    public static async Task Less<T>(T expected, T actual) where T : IComparable =>
        await Assert.That(((IComparable)actual).CompareTo(expected)).IsLessThan(0);

    public static async Task IsEmpty<T>(IEnumerable<T> collection) =>
        await Assert.That(collection).IsEmpty();

    public static async Task IsNotEmpty<T>(IEnumerable<T> collection) =>
        await Assert.That(collection).IsNotEmpty();
}
