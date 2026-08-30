namespace PeasyPilot.Core.Tests.Extensions;

using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Extensions;
using Xunit;
using Assert = Xunit.Assert;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPeasyPilotCore_RegistersRequiredCoreServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPeasyPilotCore(options =>
        {
            options.Environment = "Test";
        });

        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<ITestContext>());
        Assert.NotNull(provider.GetService<ITestEngine>());
    }

    [Fact]
    public void AddPeasyPilotPipeline_RegistersAllPipelineServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPeasyPilotPipeline();
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<ITestContext>());
        Assert.NotNull(provider.GetService<ITestEngine>());
        Assert.NotNull(provider.GetService<ITestDiscovery>());
        Assert.NotNull(provider.GetService<ITestScheduler>());
        Assert.NotNull(provider.GetService<ITestImpactAnalyzer>());
        Assert.NotNull(provider.GetService<ITestRunStore>());
        Assert.NotNull(provider.GetService<ITestPipelineOrchestrator>());
        Assert.NotNull(provider.GetService<ITestReporter>());
    }
}
