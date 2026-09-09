using PeasyPilot.TestAssistant.Orchestration;

namespace PeasyPilot.Mcp.Services;

/// <summary>
/// Factory to create and manage TestAssistant instances.
/// </summary>
public class TestAssistantFactory
{
    private static AITestEngineer? _instance;

    /// <summary>
    /// Get or create a singleton AITestEngineer instance.
    /// </summary>
    public static AITestEngineer GetTestEngineer() => _instance ??= new AITestEngineer();

    /// <summary>
    /// Reset the singleton (useful for testing).
    /// </summary>
    public static void Reset() => _instance = null;
}
