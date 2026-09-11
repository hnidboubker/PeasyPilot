namespace PeasyPilot.XUnit.Tests;

using Xunit;
using PeasyPilot.XUnit;

public class XAssertTests
{
    [Fact]
    public void Equal_WithEqualInts_Passes()
    {
        XAssert.Equal(5, 5);
    }

    [Fact]
    public void Equal_WithUnequalInts_Throws()
    {
        Assert.Throws<Xunit.Sdk.EqualException>(() => XAssert.Equal(5, 3));
    }

    [Fact]
    public void NotEqual_WithUnequalInts_Passes()
    {
        XAssert.NotEqual(5, 3);
    }

    [Fact]
    public void NotEqual_WithEqualInts_Throws()
    {
        Assert.Throws<Xunit.Sdk.NotEqualException>(() => XAssert.NotEqual(5, 5));
    }

    [Fact]
    public void True_WithTrue_Passes()
    {
        XAssert.True(true);
    }

    [Fact]
    public void True_WithFalse_Throws()
    {
        Assert.Throws<Xunit.Sdk.TrueException>(() => XAssert.True(false));
    }

    [Fact]
    public void False_WithFalse_Passes()
    {
        XAssert.False(false);
    }

    [Fact]
    public void False_WithTrue_Throws()
    {
        Assert.Throws<Xunit.Sdk.FalseException>(() => XAssert.False(true));
    }

    [Fact]
    public void Null_WithNull_Passes()
    {
        XAssert.Null((object?)null);
    }

    [Fact]
    public void Null_WithObject_Throws()
    {
        Assert.Throws<Xunit.Sdk.NullException>(() => XAssert.Null("not null"));
    }

    [Fact]
    public void NotNull_WithObject_Passes()
    {
        XAssert.NotNull("not null");
    }

    [Fact]
    public void NotNull_WithNull_Throws()
    {
        Assert.Throws<Xunit.Sdk.NotNullException>(() => XAssert.NotNull((object?)null));
    }

    [Fact]
    public void Throws_WithThrowingAction_Passes()
    {
        XAssert.Throws<InvalidOperationException>(() => throw new InvalidOperationException());
    }

    [Fact]
    public void Throws_WithNonThrowingAction_Throws()
    {
        Assert.Throws<Xunit.Sdk.XunitException>(() => XAssert.Throws<InvalidOperationException>(() => { }));
    }

    [Fact]
    public void Empty_WithEmptyCollection_Passes()
    {
        XAssert.Empty(new int[] { });
    }

    [Fact]
    public void Empty_WithNonEmptyCollection_Throws()
    {
        Assert.Throws<Xunit.Sdk.EmptyException>(() => XAssert.Empty(new[] { 1, 2, 3 }));
    }

    [Fact]
    public void NotEmpty_WithNonEmptyCollection_Passes()
    {
        XAssert.NotEmpty(new[] { 1, 2, 3 });
    }

    [Fact]
    public void NotEmpty_WithEmptyCollection_Throws()
    {
        Assert.Throws<Xunit.Sdk.NotEmptyException>(() => XAssert.NotEmpty(new int[] { }));
    }

    [Fact]
    public void Contains_WithItemPresent_Passes()
    {
        XAssert.Contains(2, new[] { 1, 2, 3 });
    }

    [Fact]
    public void Contains_WithItemAbsent_Throws()
    {
        Assert.Throws<Xunit.Sdk.ContainsException>(() => XAssert.Contains(5, new[] { 1, 2, 3 }));
    }

    [Fact]
    public void IsType_WithCorrectType_Passes()
    {
        object obj = "string";
        XAssert.IsType<string>(obj);
    }

    [Fact]
    public void IsType_WithWrongType_Throws()
    {
        object obj = "string";
        Assert.Throws<Xunit.Sdk.IsTypeException>(() => XAssert.IsType<int>(obj));
    }
}
