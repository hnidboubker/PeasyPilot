namespace PeasyPilot.TUnit;

using global::TUnit.Assertions;

/// <summary>
/// Strongly-typed assertion builder for TUnit.
/// Provides chainable async assertions with compile-time type safety.
/// </summary>
public class TAssertThat<T>
{
    private readonly T _value;

    /// <summary>
    /// Initializes a new instance of the TAssertThat builder.
    /// </summary>
    public TAssertThat(T value) => _value = value;

    /// <summary>
    /// Asserts that the value equals the expected value.
    /// </summary>
    public async Task IsEqualTo(T expected) =>
        await Assert.That(_value).IsEqualTo(expected);

    /// <summary>
    /// Asserts that the value does not equal the expected value.
    /// </summary>
    public async Task IsNotEqualTo(T expected) =>
        await Assert.That(_value).IsNotEqualTo(expected);

    /// <summary>
    /// Asserts that the value is null.
    /// </summary>
    public async Task IsNull() =>
        await Assert.That(_value).IsNull();

    /// <summary>
    /// Asserts that the value is not null.
    /// </summary>
    //public async Task IsNotNull() where T : class =>
    //    await Assert.That(_value).IsNotNull();

    ///// <summary>
    ///// Asserts that the collection is empty.
    ///// </summary>
    //public async Task IsEmpty() where T : IEnumerable =>
    //    await Assert.That((IEnumerable)_value).IsEmpty();

    ///// <summary>
    ///// Asserts that the collection is not empty.
    ///// </summary>
    //public async Task IsNotEmpty() where T : IEnumerable =>
    //    await Assert.That((IEnumerable)_value).IsNotEmpty();

    ///// <summary>
    ///// Asserts that the collection contains the specified item.
    ///// </summary>
    //public async Task Contains<TItem>(TItem item) where T : IEnumerable<TItem> =>
    //    await Assert.That((IEnumerable<TItem>)_value).Contains(item);

    ///// <summary>
    ///// Asserts that the collection does not contain the specified item.
    ///// </summary>
    //public async Task DoesNotContain<TItem>(TItem item) where T : IEnumerable<TItem> =>
    //    await Assert.That((IEnumerable<TItem>)_value).DoesNotContain(item);

    ///// <summary>
    ///// Asserts that the string starts with the specified value.
    ///// </summary>
    //public async Task StartsWith(string value) where T : class =>
    //    await Assert.That((string)(object)_value).StartsWith(value);

    ///// <summary>
    ///// Asserts that the string ends with the specified value.
    ///// </summary>
    //public async Task EndsWith(string value) where T : class =>
    //    await Assert.That((string)(object)_value).EndsWith(value);

    ///// <summary>
    ///// Asserts that the value is greater than the specified value.
    ///// </summary>
    //public async Task IsGreaterThan(T value) where T : IComparable<T> =>
    //    await Assert.That(_value).IsGreaterThan(value);

    ///// <summary>
    ///// Asserts that the value is less than the specified value.
    ///// </summary>
    //public async Task IsLessThan(T value) where T : IComparable<T> =>
    //    await Assert.That(_value).IsLessThan(value);

    ///// <summary>
    ///// Asserts that the value is greater than or equal to the specified value.
    ///// </summary>
    //public async Task IsGreaterThanOrEqualTo(T value) where T : IComparable<T> =>
    //    await Assert.That(_value).IsGreaterThanOrEqualTo(value);

    ///// <summary>
    ///// Asserts that the value is less than or equal to the specified value.
    ///// </summary>
    //public async Task IsLessThanOrEqualTo(T value) where T : IComparable<T> =>
    //    await Assert.That(_value).IsLessThanOrEqualTo(value);

    /// <summary>
    /// Asserts that the value is assignable to the specified type.
    /// </summary>
    public async Task IsAssignableTo<TTarget>() =>
        await Assert.That(_value).IsAssignableTo<TTarget>();

    /// <summary>
    /// Asserts that the value is of the specified type.
    /// </summary>
    //public async Task IsOfType<TTarget>() =>
    //    await Assert.That(_value).IsOfType<TTarget>();

    ///// <summary>
    ///// Asserts that the string has the specified length.
    ///// </summary>
    //public async Task HasLength(int length) where T : class =>
    //    await Assert.That((string)(object)_value).HasLength(length);

    ///// <summary>
    ///// Asserts that the collection has the specified count.
    ///// </summary>
    //public async Task CountIs(int count) where T : IEnumerable =>
    //    await Assert.That((IEnumerable)_value).CountIs(count);
}

/// <summary>
/// Framework-specific Assert alias for TUnit.
/// Provides explicit clarity when using TUnit assertions.
/// </summary>
public static class TAssert
{
    /// <summary>
    /// Creates a strongly-typed assertion builder for the specified value.
    /// </summary>
    public static TAssertThat<T> That<T>(T value) =>
        new(value);

    /// <summary>
    /// Asserts that two values are equal.
    /// </summary>
    public static async Task Equal<T>(T expected, T actual) =>
        await Assert.That(actual).IsEqualTo(expected);

    /// <summary>
    /// Asserts that two values are not equal.
    /// </summary>
    public static async Task NotEqual<T>(T expected, T actual) =>
        await Assert.That(actual).IsNotEqualTo(expected);

    /// <summary>
    /// Asserts that a condition is true.
    /// </summary>
    public static async Task True(bool condition) =>
        await Assert.That(condition).IsTrue();

    /// <summary>
    /// Asserts that a condition is false.
    /// </summary>
    public static async Task False(bool condition) =>
        await Assert.That(condition).IsFalse();

    /// <summary>
    /// Asserts that a value is null.
    /// </summary>
    public static async Task Null(object? obj) =>
        await Assert.That(obj).IsNull();

    /// <summary>
    /// Asserts that a value is not null.
    /// </summary>
    public static async Task NotNull(object? obj)
    {
        if (obj is null)
            throw new InvalidOperationException("Value is null");

        await Assert.That(obj).IsNotDefault();
        //await Assert.That(obj).IsNotNull();
    }
       

    public static async Task<T> NotNull<T>(this T? obj) where T : class
    {
        
        if (obj is null)
            throw new InvalidOperationException("Value is null");
        
        await Assert.That(obj).IsNotDefault();

       
        return obj!;  // ✅ null-forgiving operator
    }

    /// <summary>
    /// Asserts that a collection contains an item.
    /// </summary>
    public static async Task Contains<T>(T item, IEnumerable<T> collection) =>
        await Assert.That(collection).Contains(item);

    /// <summary>
    /// Asserts that a collection does not contain an item.
    /// </summary>
    public static async Task DoesNotContain<T>(T item, IEnumerable<T> collection) =>
        await Assert.That(collection).DoesNotContain(item);

    /// <summary>
    /// Asserts that an action throws an exception.
    /// </summary>
    public static void Throws<TException>(Action code) where TException : Exception =>
        Assert.Throws<TException>(code);

    /// <summary>
    /// Asserts that an async function throws an exception.
    /// </summary>
    public static Task ThrowsAsync<TException>(Func<Task> code) where TException : Exception =>
        Assert.ThrowsAsync<TException>(code);

    /// <summary>
    /// Asserts that a collection is empty.
    /// </summary>
    public static async Task IsEmpty<T>(IEnumerable<T> collection) =>
        await Assert.That(collection).IsEmpty();

    /// <summary>
    /// Asserts that a collection is not empty.
    /// </summary>
    public static async Task IsNotEmpty<T>(IEnumerable<T> collection) =>
        await Assert.That(collection).IsNotEmpty();

#pragma warning disable TUnitAssertions0002 // TUnit analyzer: Assert statements must be awaited
    public static T IsNotNull<T>(this T? value) where T : class
    {
        if (value is null)
            throw new InvalidOperationException("Value is null");
        return value;  // ✅ Compilateur sait que c'est non-null
    }

    public static async Task<T> IsNotNull<T>(this Task<T?> task) where T : class
    {
        var value = await task;
        if (value is null)
            throw new InvalidOperationException("Value is null");
        return value;  // ✅ Compilateur sait que c'est non-null
    }


    //public static async Task IsNotEmpty<T>(this IEnumerable<T> collection)
    //{
    //    await Assert.That(collection).IsNotNull();
    //    await Assert.That(collection).IsNotEmpty();
    //}
#pragma warning restore TUnitAssertions0002


    /// <summary>
    /// Asserts that a string starts with a value.
    /// </summary>
    public static async Task StartsWith(string text, string value) =>
        await Assert.That(text).StartsWith(value);



    /// <summary>
    /// Asserts that a string ends with a value.
    /// </summary>
    public static async Task EndsWith(string text, string value) =>
        await Assert.That(text).EndsWith(value);
}
