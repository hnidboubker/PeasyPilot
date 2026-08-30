namespace global::PeasyPilot.Core.Extensions;

using Microsoft.Extensions.DependencyInjection;
using global::PeasyPilot.Core.Abstractions;
using global::PeasyPilot.Core.Context;
using global::PeasyPilot.Core.Configuration;

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
        services.AddSingleton(options);
        
        return services;
    }
}
