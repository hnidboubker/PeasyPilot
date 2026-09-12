namespace PeasyPilot.TUnit.Tests;

using TUnit.Assertions;
using PeasyPilot.TUnit;

public class TAssertTests
{
    [Test]
    public async Task Equal_WithEqualInts_Passes() =>
        await TAssert.Equal(5, 5);

    [Test]
    public async Task NotEqual_WithUnequalInts_Passes() =>
        await TAssert.NotEqual(5, 3);

    [Test]
    public async Task IsTrue_WithTrue_Passes() =>
        await TAssert.True(true);

    [Test]
    public async Task IsFalse_WithFalse_Passes() =>
        await TAssert.False(false);

    [Test]
    public async Task IsNull_WithNull_Passes() =>
        await TAssert.Null((object?)null);

    [Test]
    public async Task IsNotNull_WithObject_Passes() =>
        await TAssert.NotNull("not null");

    [Test]
    public void Throws_WithThrowingAction_Passes() =>
        TAssert.Throws<InvalidOperationException>(() => throw new InvalidOperationException());

    [Test]
    public async Task IsEmpty_WithEmptyCollection_Passes() =>
        await TAssert.IsEmpty(new int[] { });

    [Test]
    public async Task IsNotEmpty_WithNonEmptyCollection_Passes() =>
        await TAssert.IsNotEmpty(new[] { 1, 2, 3 });

    [Test]
    public async Task Contains_WithItemPresent_Passes() =>
        await TAssert.Contains(2, new[] { 1, 2, 3 });

    [Test]
    public async Task IsGreaterThan_WithGreaterValue_Passes()
    {
        var value = TAssert.That(10);
        await value.IsGreaterThan(5);
    }

    [Test]
    public async Task IsLessThan_WithLesserValue_Passes()
    {
        var value = TAssert.That(3);
        await value.IsLessThan(5);
    }

    [Test]
    public async Task IsGreaterThanOrEqualTo_WithEqualValue_Passes()
    {
        var value = TAssert.That(5);
        await value.IsGreaterThanOrEqualTo(5);
    }

    [Test]
    public async Task IsLessThanOrEqualTo_WithEqualValue_Passes()
    {
        var value = TAssert.That(5);
        await value.IsLessThanOrEqualTo(5);
    }

    [Test]
    public async Task IsAssignableTo_WithCompatibleType_Passes()
    {
        object obj = "string";
        var value = TAssert.That(obj);
        await value.IsAssignableTo<object>();
    }

    [Test]
    public async Task IsOfType_WithCorrectType_Passes()
    {
        object obj = "string";
        var value = TAssert.That(obj);
        await value.IsOfType<string>();
    }

    [Test]
    public async Task StartsWith_WithMatchingStart_Passes() =>
        await TAssert.StartsWith("hello world", "hello");

    [Test]
    public async Task EndsWith_WithMatchingEnd_Passes() =>
        await TAssert.EndsWith("hello world", "world");

    [Test]
    public async Task Contains_WithMatchingString_Passes()
    {
        var value = TAssert.That("hello world");
        await value.Contains("lo wo");
    }

    [Test]
    public async Task HasLength_WithCorrectLength_Passes()
    {
        var value = TAssert.That("hello");
        await value.HasLength(5);
    }
}
