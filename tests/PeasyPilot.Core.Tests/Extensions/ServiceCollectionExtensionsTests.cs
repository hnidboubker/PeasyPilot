namespace PeasyPilot.Core.Tests.Extensions;

using TestContext = PeasyPilot.Core.Context.TestContext;
using PeasyPilot.Core.Assertions;
using ITestContext = PeasyPilot.Core.Abstractions.ITestContext;
using TestResult = PeasyPilot.Core.Models.TestResult;
using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Extensions;
using Xunit;

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
        PeasyAssert.NotNull(provider.GetService<ITestContext>());
        PeasyAssert.NotNull(provider.GetService<ITestEngine>());
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
        PeasyAssert.NotNull(provider.GetService<ITestContext>());
        PeasyAssert.NotNull(provider.GetService<ITestEngine>());
        PeasyAssert.NotNull(provider.GetService<ITestDiscovery>());
        PeasyAssert.NotNull(provider.GetService<ITestScheduler>());
        PeasyAssert.NotNull(provider.GetService<ITestImpactAnalyzer>());
        PeasyAssert.NotNull(provider.GetService<ITestRunStore>());
        PeasyAssert.NotNull(provider.GetService<ITestPipelineOrchestrator>());
        PeasyAssert.NotNull(provider.GetService<ITestReporter>());
    }
}
