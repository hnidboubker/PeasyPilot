namespace PeasyPilot.Core.Filters;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

/// <summary>
/// Extension methods for composing test filters.
/// </summary>
public static class TestFilterExtensions
{
    /// <summary>
    /// Combines two filters with AND logic: both must match.
    /// </summary>
    /// <param name="left">The first filter.</param>
    /// <param name="right">The second filter.</param>
    /// <returns>A combined filter.</returns>
    public static ITestFilter And(this ITestFilter left, ITestFilter right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return new CompositeAndFilter(left, right);
    }

    /// <summary>
    /// Combines two filters with OR logic: either can match.
    /// </summary>
    /// <param name="left">The first filter.</param>
    /// <param name="right">The second filter.</param>
    /// <returns>A combined filter.</returns>
    public static ITestFilter Or(this ITestFilter left, ITestFilter right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return new CompositeOrFilter(left, right);
    }

    /// <summary>
    /// Inverts filter logic: matches when the filter doesn't match.
    /// </summary>
    /// <param name="filter">The filter to negate.</param>
    /// <returns>A negated filter.</returns>
    public static ITestFilter Negate(this ITestFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return new CompositeNotFilter(filter);
    }

    /// <summary>
    /// AND combinator filter.
    /// </summary>
    private sealed class CompositeAndFilter(ITestFilter left, ITestFilter right) : ITestFilter
    {
        public bool Matches(TestCase test) => left.Matches(test) && right.Matches(test);
    }

    /// <summary>
    /// OR combinator filter.
    /// </summary>
    private sealed class CompositeOrFilter(ITestFilter left, ITestFilter right) : ITestFilter
    {
        public bool Matches(TestCase test) => left.Matches(test) || right.Matches(test);
    }

    /// <summary>
    /// NOT combinator filter.
    /// </summary>
    private sealed class CompositeNotFilter(ITestFilter filter) : ITestFilter
    {
        public bool Matches(TestCase test) => !filter.Matches(test);
    }
}
