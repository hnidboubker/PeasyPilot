namespace PeasyPilot.TUnit.Tests;

using TUnit.Assertions;
using PeasyPilot.TUnit;

public class TAssertTests
{
    [Test]
    public void Equal_WithEqualInts_Passes()
    {
        TAssert.That(5).IsEqualTo(5);
    }

    [Test]
    public void NotEqual_WithUnequalInts_Passes()
    {
        TAssert.That(5).IsNotEqualTo(3);
    }

    [Test]
    public void IsTrue_WithTrue_Passes()
    {
        TAssert.That(true).IsTrue();
    }

    [Test]
    public void IsFalse_WithFalse_Passes()
    {
        TAssert.That(false).IsFalse();
    }

    [Test]
    public void IsNull_WithNull_Passes()
    {
        object? obj = null;
        TAssert.That(obj).IsNull();
    }

    [Test]
    public void IsNotNull_WithObject_Passes()
    {
        TAssert.That("not null").IsNotNull();
    }

    [Test]
    public void Throws_WithThrowingAction_Passes()
    {
        TAssert.Throws<InvalidOperationException>(() => throw new InvalidOperationException());
    }

    [Test]
    public void IsEmpty_WithEmptyCollection_Passes()
    {
        TAssert.That(new int[] { }).IsEmpty();
    }

    [Test]
    public void IsNotEmpty_WithNonEmptyCollection_Passes()
    {
        TAssert.That(new[] { 1, 2, 3 }).IsNotEmpty();
    }

    [Test]
    public void Contains_WithItemPresent_Passes()
    {
        TAssert.That(new[] { 1, 2, 3 }).Contains(2);
    }

    [Test]
    public void CountIs_WithCorrectCount_Passes()
    {
        TAssert.That(new[] { 1, 2, 3 }).CountIs(3);
    }

    [Test]
    public void IsGreaterThan_WithGreaterValue_Passes()
    {
        TAssert.That(10).IsGreaterThan(5);
    }

    [Test]
    public void IsLessThan_WithLesserValue_Passes()
    {
        TAssert.That(3).IsLessThan(5);
    }

    [Test]
    public void IsGreaterThanOrEqualTo_WithEqualValue_Passes()
    {
        TAssert.That(5).IsGreaterThanOrEqualTo(5);
    }

    [Test]
    public void IsLessThanOrEqualTo_WithEqualValue_Passes()
    {
        TAssert.That(5).IsLessThanOrEqualTo(5);
    }

    [Test]
    public void IsAssignableTo_WithCompatibleType_Passes()
    {
        object obj = "string";
        TAssert.That(obj).IsAssignableTo<object>();
    }

    [Test]
    public void IsOfType_WithCorrectType_Passes()
    {
        object obj = "string";
        TAssert.That(obj).IsOfType<string>();
    }

    [Test]
    public void StartsWith_WithMatchingStart_Passes()
    {
        TAssert.That("hello world").StartsWith("hello");
    }

    [Test]
    public void EndsWith_WithMatchingEnd_Passes()
    {
        TAssert.That("hello world").EndsWith("world");
    }

    [Test]
    public void Contains_WithMatchingString_Passes()
    {
        TAssert.That("hello world").Contains("lo wo");
    }

    [Test]
    public void HasLength_WithCorrectLength_Passes()
    {
        TAssert.That("hello").HasLength(5);
    }
}
