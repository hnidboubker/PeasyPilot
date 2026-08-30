namespace PeasyPilot.CLI;

/// <summary>
/// Main entry point for the PeasyPilot command line runner.
/// </summary>
public static class Program
{
    /// <summary>
    /// Program main entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Exit code (0 = success, 1 = failure).</returns>
    public static async Task<int> Main(string[] args)
    {
        return await CliRunner.RunAsync(args);
    }
}
