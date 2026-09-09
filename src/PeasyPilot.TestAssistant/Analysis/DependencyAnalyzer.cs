using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Analysis;

/// <summary>
/// Analyzes dependencies and suggests testing strategies.
/// </summary>
public class DependencyAnalyzer
{
    private static readonly Dictionary<string, string> InterfaceMockStrategies = new()
    {
        // Data access
        { "IRepository", "Moq.Mock" },
        { "IUnitOfWork", "Moq.Mock" },
        { "IDbContext", "IntegrationTestFixture" },

        // Logging
        { "ILogger", "Moq.Mock" },
        { "ILoggerProvider", "Moq.Mock" },

        // External services
        { "IHttpClient", "Moq.Mock" },
        { "IEmailService", "Moq.Mock" },
        { "IPaymentService", "Moq.Mock" },
        { "INotificationService", "Moq.Mock" },

        // Configuration
        { "IConfiguration", "Moq.Mock" },
        { "IOptions", "Moq.Mock" },

        // Identity
        { "IAuthenticationService", "IntegrationTestFixture" },
        { "IIdentityProvider", "Moq.Mock" },

        // Default
        { "IService", "Moq.Mock" }
    };

    /// <summary>
    /// Suggests a testing strategy for a dependency.
    /// </summary>
    public string SuggestStrategy(DependencyInfo dependency)
    {
        var interfaceName = dependency.InterfaceName;

        // Exact match
        if (InterfaceMockStrategies.TryGetValue(interfaceName, out var strategy))
        {
            return strategy;
        }

        // Partial match (last part of interface name)
        var shortName = interfaceName.Split('.').Last();
        if (InterfaceMockStrategies.TryGetValue(shortName, out var shortStrategy))
        {
            return shortStrategy;
        }

        // Database-related interfaces should use fixtures
        if (shortName.Contains("Database") || shortName.Contains("DbContext") || shortName.Contains("Repository"))
        {
            return "IntegrationTestFixture";
        }

        // External service-like interfaces
        if (shortName.Contains("Service") && !shortName.Contains("Log"))
        {
            return "Moq.Mock";
        }

        // Default: mock everything else
        return "Moq.Mock";
    }

    /// <summary>
    /// Determines if a dependency should use a fixture (integration test).
    /// </summary>
    public bool ShouldUseFixture(DependencyInfo dependency)
    {
        return SuggestStrategy(dependency) == "IntegrationTestFixture";
    }

    /// <summary>
    /// Determines if a dependency should be mocked.
    /// </summary>
    public bool ShouldMock(DependencyInfo dependency)
    {
        return SuggestStrategy(dependency) == "Moq.Mock";
    }

    /// <summary>
    /// Groups dependencies by testing strategy.
    /// </summary>
    public Dictionary<string, List<DependencyInfo>> GroupByStrategy(List<DependencyInfo> dependencies)
    {
        var grouped = new Dictionary<string, List<DependencyInfo>>();

        foreach (var dep in dependencies)
        {
            var strategy = SuggestStrategy(dep);
            if (!grouped.ContainsKey(strategy))
            {
                grouped[strategy] = new();
            }

            grouped[strategy].Add(dep);
        }

        return grouped;
    }
}
