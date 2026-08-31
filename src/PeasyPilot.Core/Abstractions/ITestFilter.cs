namespace PeasyPilot.Core.Abstractions;

using PeasyPilot.Core.Models;

/// <summary>
/// Filters unified test cases before execution.
/// </summary>
public interface ITestFilter
{
    /// <summary>
    /// Determines whether a test matches the filter.
    /// </summary>
    /// <param name="test">The test case.</param>
    /// <returns>True when the test matches.</returns>
    bool Matches(TestCase test);
}
