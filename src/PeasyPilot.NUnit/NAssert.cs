namespace PeasyPilot.NUnit;

using global::NUnit.Framework;
using global::NUnit.Framework.Constraints;

/// <summary>
/// Strongly-typed assertion builder for NUnit.
/// Provides chainable sync assertions with compile-time type safety.
/// </summary>
public class NAssertThat<T>
{
    private readonly T _value;

    /// <summary>
    /// Initializes a new instance of the NAssertThat builder.
    /// </summary>
    public NAssertThat(T value) => _value = value;

    /// <summary>
    /// Asserts that the value equals the expected value.
    /// </summary>
    public void IsEqualTo(T expected) =>
        Assert.That(_value, Is.EqualTo(expected));

    /// <summary>
    /// Asserts that the value does not equal the expected value.
    /// </summary>
    public void IsNotEqualTo(T expected) =>
        Assert.That(_value, Is.Not.EqualTo(expected));

    /// <summary>
    /// Asserts that the value is null.
    /// </summary>
    public void IsNull() =>
        Assert.That(_value, Is.Null);

    /// <summary>
    /// Asserts that the value is not null.
    /// </summary>
    public void IsNotNull() =>
        Assert.That(_value, Is.Not.Null);

    /// <summary>
    /// Asserts that the value is of the specified type.
    /// </summary>
    public void IsOfType<TTarget>() =>
        Assert.That(_value, Is.TypeOf<TTarget>());

    /// <summary>
    /// Asserts that the value is assignable to the specified type.
    /// </summary>
    public void IsAssignableTo<TTarget>() =>
        Assert.That(_value, Is.InstanceOf<TTarget>());
}

/// <summary>
/// Framework-specific Assert alias for NUnit.
/// Provides explicit clarity when using NUnit assertions.
/// </summary>
public static class NAssert
{
    /// <summary>
    /// Creates a strongly-typed assertion builder for the specified value.
    /// </summary>
    public static NAssertThat<T> That<T>(T value) =>
        new(value);

    /// <summary>
    /// Asserts that a value satisfies a constraint.
    /// </summary>
    public static void That<T>(T actual, IResolveConstraint expression) =>
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
