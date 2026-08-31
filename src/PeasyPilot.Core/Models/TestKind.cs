namespace PeasyPilot.Core.Models;

/// <summary>
/// Specifies the classification or style of a test case.
/// </summary>
public enum TestKind
{
    /// <summary>
    /// Unit test.
    /// </summary>
    Unit,

    /// <summary>
    /// Integration test.
    /// </summary>
    Integration,

    /// <summary>
    /// Behavior-driven development test/scenario.
    /// </summary>
    Bdd,

    /// <summary>
    /// Property-based test.
    /// </summary>
    Property,

    /// <summary>
    /// Contract test.
    /// </summary>
    Contract
}
