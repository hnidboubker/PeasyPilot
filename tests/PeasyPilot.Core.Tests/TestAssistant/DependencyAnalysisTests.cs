using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Models;
using Xunit;

namespace PeasyPilot.Core.Tests.TestAssistant;

/// <summary>
/// Tests for DependencyAnalyzer - Tier 1 of Phase 5
/// </summary>
public class DependencyAnalysisTests
{
    private readonly DependencyAnalyzer _analyzer = new();

    #region Strategy Suggestion Tests

    [Fact]
    public void SuggestStrategy_WithRepositoryInterface_ReturnsMockStrategy()
    {
        // Arrange
        var dependency = new DependencyInfo
        {
            InterfaceName = "IRepository",
            InjectionType = "Constructor",
            IsRequired = true
        };

        // Act
        var strategy = _analyzer.SuggestStrategy(dependency);

        // Assert
        Assert.Equal("Moq.Mock", strategy);
    }

    [Fact]
    public void SuggestStrategy_WithDatabaseContext_ReturnsFixtureStrategy()
    {
        // Arrange
        var dependency = new DependencyInfo
        {
            InterfaceName = "IDbContext",
            InjectionType = "Constructor",
            IsRequired = true
        };

        // Act
        var strategy = _analyzer.SuggestStrategy(dependency);

        // Assert
        Assert.Equal("IntegrationTestFixture", strategy);
    }

    [Fact]
    public void SuggestStrategy_WithLoggerInterface_ReturnsMockStrategy()
    {
        // Arrange
        var dependency = new DependencyInfo
        {
            InterfaceName = "ILogger",
            InjectionType = "Constructor",
            IsRequired = true
        };

        // Act
        var strategy = _analyzer.SuggestStrategy(dependency);

        // Assert
        Assert.Equal("Moq.Mock", strategy);
    }

    [Fact]
    public void SuggestStrategy_WithUnknownInterface_DefaultsToMock()
    {
        // Arrange
        var dependency = new DependencyInfo
        {
            InterfaceName = "IUnknownService",
            InjectionType = "Constructor",
            IsRequired = true
        };

        // Act
        var strategy = _analyzer.SuggestStrategy(dependency);

        // Assert
        Assert.Equal("Moq.Mock", strategy);
    }

    #endregion

    #region Fixture Detection Tests

    [Fact]
    public void ShouldUseFixture_WithDatabaseDependency_ReturnsTrue()
    {
        // Arrange
        var dependency = new DependencyInfo
        {
            InterfaceName = "IDbContext",
            InjectionType = "Constructor",
            IsRequired = true
        };

        // Act
        var result = _analyzer.ShouldUseFixture(dependency);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldUseFixture_WithLoggerDependency_ReturnsFalse()
    {
        // Arrange
        var dependency = new DependencyInfo
        {
            InterfaceName = "ILogger",
            InjectionType = "Constructor",
            IsRequired = true
        };

        // Act
        var result = _analyzer.ShouldUseFixture(dependency);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Mock Detection Tests

    [Fact]
    public void ShouldMock_WithServiceDependency_ReturnsTrue()
    {
        // Arrange
        var dependency = new DependencyInfo
        {
            InterfaceName = "IEmailService",
            InjectionType = "Constructor",
            IsRequired = true
        };

        // Act
        var result = _analyzer.ShouldMock(dependency);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Grouping Tests

    [Fact]
    public void GroupByStrategy_WithMixedDependencies_GroupsCorrectly()
    {
        // Arrange
        var dependencies = new List<DependencyInfo>
        {
            new() { InterfaceName = "ILogger", InjectionType = "Constructor", IsRequired = true },
            new() { InterfaceName = "IEmailService", InjectionType = "Constructor", IsRequired = true },
            new() { InterfaceName = "IDbContext", InjectionType = "Constructor", IsRequired = true }
        };

        // Act
        var grouped = _analyzer.GroupByStrategy(dependencies);

        // Assert
        Assert.Equal(2, grouped.Keys.Count);
        Assert.Contains("Moq.Mock", grouped.Keys);
        Assert.Contains("IntegrationTestFixture", grouped.Keys);
        Assert.Equal(2, grouped["Moq.Mock"].Count);
        Assert.Single(grouped["IntegrationTestFixture"]);
    }

    [Fact]
    public void GroupByStrategy_WithEmptyDependencies_ReturnsEmptyDictionary()
    {
        // Arrange
        var dependencies = new List<DependencyInfo>();

        // Act
        var grouped = _analyzer.GroupByStrategy(dependencies);

        // Assert
        Assert.Empty(grouped);
    }

    #endregion
}
