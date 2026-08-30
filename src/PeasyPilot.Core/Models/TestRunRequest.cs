namespace global::PeasyPilot.Core.Models;

/// <summary>
/// Represents a request to run one or more unified test cases.
/// </summary>
public class TestRunRequest
{
    /// <summary>
    /// Gets or sets the name of the run.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a dictionary of metadata used by the engine.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of test cases to execute.
    /// </summary>
    public List<TestCase> TestCases { get; set; } = new();
}
