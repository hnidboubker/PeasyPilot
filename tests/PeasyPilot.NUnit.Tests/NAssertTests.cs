namespace PeasyPilot.NUnit.Tests;

using global::NUnit.Framework;
using PeasyPilot.NUnit;

[TestFixture]
public class NAssertTests
{
    [Test]
    public void Equal_WithEqualInts_Passes()
    {
        NAssert.Equal(5, 5);
    }

    [Test]
    public void Equal_WithUnequalInts_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.Equal(5, 3));
    }

    [Test]
    public void NotEqual_WithUnequalInts_Passes()
    {
        NAssert.NotEqual(5, 3);
    }

    [Test]
    public void NotEqual_WithEqualInts_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.NotEqual(5, 5));
    }

    [Test]
    public void True_WithTrue_Passes()
    {
        NAssert.True(true);
    }

    [Test]
    public void True_WithFalse_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.True(false));
    }

    [Test]
    public void False_WithFalse_Passes()
    {
        NAssert.False(false);
    }

    [Test]
    public void False_WithTrue_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.False(true));
    }

    [Test]
    public void Null_WithNull_Passes()
    {
        NAssert.Null((object?)null);
    }

    [Test]
    public void Null_WithObject_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.Null("not null"));
    }

    [Test]
    public void NotNull_WithObject_Passes()
    {
        NAssert.NotNull("not null");
    }

    [Test]
    public void NotNull_WithNull_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.NotNull((object?)null));
    }

    [Test]
    public void Throws_WithThrowingAction_Passes()
    {
        NAssert.Throws<InvalidOperationException>(() => throw new InvalidOperationException());
    }

    [Test]
    public void Throws_WithNonThrowingAction_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.Throws<InvalidOperationException>(() => { }));
    }

    [Test]
    public void Greater_WithGreaterValue_Passes()
    {
        NAssert.Greater(10, 5);
    }

    [Test]
    public void Greater_WithLesserValue_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.Greater(3, 5));
    }

    [Test]
    public void Less_WithLesserValue_Passes()
    {
        NAssert.Less(3, 5);
    }

    [Test]
    public void Less_WithGreaterValue_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.Less(10, 5));
    }

    [Test]
    public void Contains_WithItemPresent_Passes()
    {
        NAssert.Contains(2, new[] { 1, 2, 3 });
    }

    [Test]
    public void Contains_WithItemAbsent_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.Contains(5, new[] { 1, 2, 3 }));
    }

    [Test]
    public void IsEmpty_WithEmptyCollection_Passes()
    {
        NAssert.IsEmpty(new int[] { });
    }

    [Test]
    public void IsEmpty_WithNonEmptyCollection_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.IsEmpty(new[] { 1, 2, 3 }));
    }

    [Test]
    public void IsNotEmpty_WithNonEmptyCollection_Passes()
    {
        NAssert.IsNotEmpty(new[] { 1, 2, 3 });
    }

    [Test]
    public void IsNotEmpty_WithEmptyCollection_Throws()
    {
        Assert.Throws<AssertionException>(() => NAssert.IsNotEmpty(new int[] { }));
    }
}
