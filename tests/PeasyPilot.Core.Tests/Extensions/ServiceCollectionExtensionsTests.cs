namespace PeasyPilot.Core.Tests.Extensions;

using TestContext = PeasyPilot.Core.Context.TestContext;
using ITestContext = PeasyPilot.Core.Abstractions.ITestContext;
using TestResult = PeasyPilot.Core.Models.TestResult;
using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Extensions;
using Xunit;
using PeasyPilot.XUnit;

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
        XAssert.NotNull(provider.GetService<ITestContext>());
        XAssert.NotNull(provider.GetService<ITestEngine>());
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
        XAssert.NotNull(provider.GetService<ITestContext>());
        XAssert.NotNull(provider.GetService<ITestEngine>());
        XAssert.NotNull(provider.GetService<ITestDiscovery>());
        XAssert.NotNull(provider.GetService<ITestScheduler>());
        XAssert.NotNull(provider.GetService<ITestImpactAnalyzer>());
        XAssert.NotNull(provider.GetService<ITestRunStore>());
        XAssert.NotNull(provider.GetService<ITestPipelineOrchestrator>());
        XAssert.NotNull(provider.GetService<ITestReporter>());
    }
}
