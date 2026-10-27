using PeasyPilot.Core.Assertions;
using Xunit;

namespace PeasyPilot.Core.Tests.Assertions;

public class PeasyAssertTests
{
    [Fact]
    public void Equal_DifferentValues_Throws()
    {
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.Equal(1, 2));
    }

    [Fact]
    public void Equal_SameValues_Passes()
    {
        PeasyAssert.Equal("a", "a");
    }

    [Fact]
    public void True_False_Throws()
    {
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.True(false));
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.False(true));
    }

    [Fact]
    public void Null_NotNull_Throws()
    {
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.Null("x"));
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.NotNull(null));
    }

    [Fact]
    public void Contains_ItemOrSubstring_ChecksPresence()
    {
        PeasyAssert.Contains(2, new[] { 1, 2, 3 });
        PeasyAssert.Contains("SUMMARY", "PEASYPILOT SUMMARY");
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.Contains("missing", "PEASYPILOT SUMMARY"));
    }

    [Fact]
    public void IsEmpty_NotEmpty_ChecksCount()
    {
        PeasyAssert.IsEmpty(Array.Empty<int>());
        PeasyAssert.NotEmpty(new[] { 1 });
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.IsEmpty(new[] { 1 }));
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.NotEmpty(Array.Empty<int>()));
    }

    [Fact]
    public void Single_ReturnsTheOnlyItem_AndRejectsOtherCounts()
    {
        PeasyAssert.Equal(7, PeasyAssert.Single(new[] { 7 }));
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.Single(new[] { 1, 2 }));
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.Single(Array.Empty<int>()));
    }

    [Fact]
    public void Throws_ReturnsExpectedException_AndRejectsNothingThrown()
    {
        var ex = PeasyAssert.Throws<InvalidOperationException>(() => throw new InvalidOperationException("boom"));

        PeasyAssert.Equal("boom", ex.Message);
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.Throws<InvalidOperationException>(() => { }));
    }

    [Fact]
    public async Task ThrowsAsync_ReturnsExpectedException_AndRejectsNothingThrown()
    {
        var ex = await PeasyAssert.ThrowsAsync<InvalidOperationException>(() => throw new InvalidOperationException("boom"));

        PeasyAssert.Equal("boom", ex.Message);
        await PeasyAssert.ThrowsAsync<AssertionException>(() => PeasyAssert.ThrowsAsync<InvalidOperationException>(() => Task.CompletedTask));
    }
    [Fact]
    public void All_RunsActionOnEveryItem()
    {
        var seen = new List<int>();

        PeasyAssert.All(new[] { 1, 2 }, seen.Add);

        PeasyAssert.Equal(2, seen.Count);
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.All(new[] { 1, 2 }, n => PeasyAssert.Equal(1, n)));
    }

    [Fact]
    public void DoesNotContain_ItemAndSubstring_ChecksAbsence()
    {
        PeasyAssert.DoesNotContain(9, new[] { 1, 2 });
        PeasyAssert.DoesNotContain("missing", "PEASYPILOT SUMMARY");
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.DoesNotContain("SUMMARY", "PEASYPILOT SUMMARY"));
    }

    [Fact]
    public void IsType_ExactTypeOnly_AndIsAssignableFrom_AcceptsSubtypes()
    {
        PeasyAssert.IsType<string>("x");
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.IsType<object>("x"));
        PeasyAssert.IsAssignableFrom<object>("x");
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.IsAssignableFrom<int>("x"));
    }
    [Fact]
    public void Equal_Sequences_ComparesItemByItem()
    {
        PeasyAssert.Equal<IEnumerable<string>>(new[] { "-result-trx", "r.trx" }, new List<string> { "-result-trx", "r.trx" });
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.Equal(new[] { 1, 2 }, new[] { 1, 3 }));
    }
    [Fact]
    public void DoesNotContain_Predicate_ChecksNoItemMatches()
    {
        PeasyAssert.DoesNotContain(new[] { 1, 2 }, n => n > 5);
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.DoesNotContain(new[] { 1, 2 }, n => n == 2));
    }
    [Fact]
    public void Same_ReferenceEquality_AndNullText()
    {
        var item = new object();
        PeasyAssert.Same(item, item);
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.Same(item, new object()));
        PeasyAssert.Throws<AssertionException>(() => PeasyAssert.Contains("x", (string?)null));
    }
}
