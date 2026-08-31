using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Configuration;
using PeasyPilot.Core.Context;
using PeasyPilot.Core.Discovery;
using PeasyPilot.Core.Engines;
using PeasyPilot.Core.ImpactAnalysis;
using PeasyPilot.Core.Orchestration;
using PeasyPilot.Core.Reporting;
using PeasyPilot.Core.Scheduling;
using PeasyPilot.Core.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace PeasyPilot.Core.Extensions;

/// <summary>
/// Extension methods for registering PeasyPilot services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PeasyPilot testing services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPeasyPilotCore(this IServiceCollection services)
    {
        services.AddScoped<ITestContext, TestContext>();
        services.AddSingleton<ITestEngine, TestEngine>();
        return services;
    }

    /// <summary>
    /// Adds PeasyPilot testing services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPeasyPilotCore(
        this IServiceCollection services,
        Action<TestOptions> configure)
    {
        var options = new TestOptions();
        configure(options);
        
        services.AddScoped<ITestContext, TestContext>();
        services.AddSingleton<ITestEngine, TestEngine>();
        services.AddSingleton(options);
        
        return services;
    }

    /// <summary>
    /// Registers full PeasyPilot pipeline services including discovery, scheduling, impact analysis,
    /// orchestration, reporters, and storage.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPeasyPilotPipeline(this IServiceCollection services)
    {
        services.AddScoped<ITestContext, TestContext>();
        services.AddSingleton<ITestEngine, TestEngine>();
        services.AddSingleton<ITestDiscovery>(new DefaultTestDiscovery());
        services.AddSingleton<ITestScheduler, DefaultTestScheduler>();
        services.AddSingleton<ITestImpactAnalyzer, DefaultTestImpactAnalyzer>();
        services.AddSingleton<ITestRunStore, InMemoryTestRunStore>();
        services.AddSingleton<ITestStore, InMemoryTestStore>();
        services.AddSingleton<ITestDiagnostic, SmartDiagnosticProvider>();
        services.AddSingleton<ITestPipelineOrchestrator, TestPipelineOrchestrator>();
        services.AddSingleton<ITestReporter, ConsoleReporter>();

        return services;
    }
}
