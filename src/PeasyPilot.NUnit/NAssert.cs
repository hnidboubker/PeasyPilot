namespace PeasyPilot.NUnit;

using global::NUnit.Framework;

/// <summary>
/// Framework-specific Assert alias for NUnit.
/// Provides explicit clarity when using NUnit assertions.
/// </summary>
public static class NAssert
{
    public static void That<T>(T actual, global::NUnit.Framework.Constraints.IResolveConstraint expression) =>
        Assert.That(actual, expression);

    public static void Equal<T>(T expected, T actual) =>
        Assert.That(actual, Is.EqualTo(expected));

    public static void NotEqual<T>(T expected, T actual) =>
        Assert.That(actual, Is.Not.EqualTo(expected));

    public static void True(bool condition) =>
        Assert.That(condition, Is.True);

    public static void False(bool condition) =>
        Assert.That(condition, Is.False);

    public static void Null(object? obj) =>
        Assert.That(obj, Is.Null);

    public static void NotNull(object? obj) =>
        Assert.That(obj, Is.Not.Null);

    public static void Contains<T>(T item, ICollection<T> collection) =>
        Assert.That(collection, global::NUnit.Framework.Contains.Item(item));

    public static void DoesNotContain<T>(T item, ICollection<T> collection) =>
        Assert.That(collection, global::NUnit.Framework.Does.Not.Contain(item));

    public static void Throws<TException>(TestDelegate code) where TException : Exception =>
        Assert.Throws<TException>(code);

    public static void ThrowsAsync<TException>(AsyncTestDelegate code) where TException : Exception =>
        Assert.ThrowsAsync<TException>(code);

    public static void Greater<T>(T expected, T actual) where T : IComparable =>
        Assert.That(actual, Is.GreaterThan(expected));

    public static void Less<T>(T expected, T actual) where T : IComparable =>
        Assert.That(actual, Is.LessThan(expected));

    public static void IsEmpty<T>(ICollection<T> collection) =>
        Assert.That(collection, Is.Empty);

    public static void IsNotEmpty<T>(ICollection<T> collection) =>
        Assert.That(collection, Is.Not.Empty);

    public static void AreEqual<T>(T expected, T actual) =>
        Assert.That(actual, Is.EqualTo(expected));

    public static void AreNotEqual<T>(T expected, T actual) =>
        Assert.That(actual, Is.Not.EqualTo(expected));
}
