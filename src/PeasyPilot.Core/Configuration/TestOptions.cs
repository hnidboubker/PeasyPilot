namespace global::PeasyPilot.Core.Configuration;

/// <summary>
/// Configuration options for testing.
/// </summary>
public class TestOptions
{
    /// <summary>
    /// Gets or sets the test environment name.
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled.
    /// </summary>
    public bool EnableLogging { get; set; } = true;
}

