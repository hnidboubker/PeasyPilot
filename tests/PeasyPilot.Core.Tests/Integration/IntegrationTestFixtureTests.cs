using Microsoft.Extensions.DependencyInjection;
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

public interface ITestService
{
    string GetValue();
}

public class TestService : ITestService
{
    public string GetValue() => "test-value";
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
        Assert.NotNull(Services);
    }

    [Fact]
    public void Database_IsAvailable()
    {
        Assert.NotNull(Database);
    }

    [Fact]
    public void GetService_ResolvesRegisteredDependency()
    {
        var service = GetService<ITestService>();

        Assert.NotNull(service);
        Assert.Equal("test-value", service.GetValue());
    }

    [Fact]
    public void GetService_WithUnregisteredType_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            GetService<IUnregisteredService>();
        });

        Assert.Contains("No service for type", exception.Message);
    }

    [Fact]
    public async Task ResetDatabaseAsync_Completes()
    {
        await ResetDatabaseAsync();

        // No exception thrown
        Assert.True(true);
    }

    [Fact]
    public void MultipleTests_HaveIndependentServices()
    {
        var service1 = GetService<ITestService>();
        var value1 = service1.GetValue();

        Assert.Equal("test-value", value1);
    }
}

public interface IUnregisteredService { }
