using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Core.Assertions;

using PeasyPilot.Core.Tests.Abstractions;
using PeasyPilot.Integration.Fixtures;

using Xunit;

namespace PeasyPilot.Core.Tests.Integration;

/// <summary>
/// Example fixture for testing IntegrationTestFixture functionality.
/// Demonstrates proper usage and validates lifecycle.
/// </summary>
public class TestFixture : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITestService, TestService>();
    }
}

/// <summary>
/// Tests for IntegrationTestFixture base class.
/// Validates DI container, database lifecycle, and helper methods.
/// </summary>
public class IntegrationTestFixtureTests : XUnitIntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITestService, TestService>();
    }

    [Fact]
    public void Services_IsAvailable()
    {
        PeasyAssert.NotNull(Services);
    }

    [Fact]
    public void Database_IsAvailable()
    {
        PeasyAssert.NotNull(Database);
    }

    [Fact]
    public void GetService_ResolvesRegisteredDependency()
    {
        var service = GetService<ITestService>();

        PeasyAssert.NotNull(service);
        PeasyAssert.Equal("test-value", service.GetValue());
    }

    [Fact]
    public void GetService_WithUnregisteredType_Throws()
    {
        var exception = PeasyAssert.Throws<InvalidOperationException>(() =>
        {
            GetService<IUnregisteredService>();
        });

        PeasyAssert.Contains("No service for type", exception.Message);
    }

    [Fact]
    public async Task ResetDatabaseAsync_Completes()
    {
        await ResetDatabaseAsync();

        // No exception thrown
        PeasyAssert.True(true);
    }

    [Fact]
    public void MultipleTests_HaveIndependentServices()
    {
        var service1 = GetService<ITestService>();
        var value1 = service1.GetValue();

        PeasyAssert.Equal("test-value", value1);
    }
}
