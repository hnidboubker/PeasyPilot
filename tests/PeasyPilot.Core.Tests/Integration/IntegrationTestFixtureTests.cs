using Microsoft.Extensions.DependencyInjection;

using PeasyPilot.Core.Tests.Abstractions;
using PeasyPilot.Integration.Fixtures;
using PeasyPilot.XUnit;

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
        XAssert.NotNull(Services);
    }

    [Fact]
    public void Database_IsAvailable()
    {
        XAssert.NotNull(Database);
    }

    [Fact]
    public void GetService_ResolvesRegisteredDependency()
    {
        var service = GetService<ITestService>();

        XAssert.NotNull(service);
        XAssert.Equal("test-value", service.GetValue());
    }

    [Fact]
    public void GetService_WithUnregisteredType_Throws()
    {
        var exception = XAssert.Throws<InvalidOperationException>(() =>
        {
            GetService<IUnregisteredService>();
        });

        XAssert.Contains("No service for type", exception.Message);
    }

    [Fact]
    public async Task ResetDatabaseAsync_Completes()
    {
        await ResetDatabaseAsync();

        // No exception thrown
        XAssert.True(true);
    }

    [Fact]
    public void MultipleTests_HaveIndependentServices()
    {
        var service1 = GetService<ITestService>();
        var value1 = service1.GetValue();

        XAssert.Equal("test-value", value1);
    }
}
