namespace global::PeasyPilot.Core.Models;

/// <summary>
/// Represents a single test case in a unified run request.
/// </summary>
public class TestCase
{
    /// <summary>
    /// Gets or sets the test case name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category or suite name.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional metadata attached to the test case.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
