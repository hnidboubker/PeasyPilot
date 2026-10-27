using PeasyPilot.Core.Assertions;
using PeasyPilot.XUnit;
using Xunit;

namespace PeasyPilot.Core.Tests.Assertions;

public class PeasyAssertTests
{
    [Fact]
    public void Equal_DifferentValues_Throws()
    {
        XAssert.Throws<AssertionException>(() => PeasyAssert.Equal(1, 2));
    }

    [Fact]
    public void Equal_SameValues_Passes()
    {
        PeasyAssert.Equal("a", "a");
    }

    [Fact]
    public void True_False_Throws()
    {
        XAssert.Throws<AssertionException>(() => PeasyAssert.True(false));
        XAssert.Throws<AssertionException>(() => PeasyAssert.False(true));
    }

    [Fact]
    public void Null_NotNull_Throws()
    {
        XAssert.Throws<AssertionException>(() => PeasyAssert.Null("x"));
        XAssert.Throws<AssertionException>(() => PeasyAssert.NotNull(null));
    }

    [Fact]
    public void Contains_ItemOrSubstring_ChecksPresence()
    {
        PeasyAssert.Contains(2, new[] { 1, 2, 3 });
        PeasyAssert.Contains("SUMMARY", "PEASYPILOT SUMMARY");
        XAssert.Throws<AssertionException>(() => PeasyAssert.Contains("missing", "PEASYPILOT SUMMARY"));
    }

    [Fact]
    public void IsEmpty_NotEmpty_ChecksCount()
    {
        PeasyAssert.IsEmpty(Array.Empty<int>());
        PeasyAssert.NotEmpty(new[] { 1 });
        XAssert.Throws<AssertionException>(() => PeasyAssert.IsEmpty(new[] { 1 }));
        XAssert.Throws<AssertionException>(() => PeasyAssert.NotEmpty(Array.Empty<int>()));
    }

    [Fact]
    public void Single_ReturnsTheOnlyItem_AndRejectsOtherCounts()
    {
        XAssert.Equal(7, PeasyAssert.Single(new[] { 7 }));
        XAssert.Throws<AssertionException>(() => PeasyAssert.Single(new[] { 1, 2 }));
        XAssert.Throws<AssertionException>(() => PeasyAssert.Single(Array.Empty<int>()));
    }

    [Fact]
    public void Throws_ReturnsExpectedException_AndRejectsNothingThrown()
    {
        var ex = PeasyAssert.Throws<InvalidOperationException>(() => throw new InvalidOperationException("boom"));

        XAssert.Equal("boom", ex.Message);
        XAssert.Throws<AssertionException>(() => PeasyAssert.Throws<InvalidOperationException>(() => { }));
    }

    [Fact]
    public async Task ThrowsAsync_ReturnsExpectedException_AndRejectsNothingThrown()
    {
        var ex = await PeasyAssert.ThrowsAsync<InvalidOperationException>(() => throw new InvalidOperationException("boom"));

        XAssert.Equal("boom", ex.Message);
        await XAssert.ThrowsAsync<AssertionException>(() => PeasyAssert.ThrowsAsync<InvalidOperationException>(() => Task.CompletedTask));
    }
}
