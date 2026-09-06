namespace PeasyPilot.Core.Tests.BDD;

using PeasyPilot.BDD.FileLoading;
using Xunit;

public class FeatureFileLoaderTests
{
    [Fact]
    public async Task LoadFromFileAsync_ValidFeatureFile_ReturnsFeature()
    {
        // Arrange
        var loader = new GherkinFeatureFileLoader();
        var solutionRoot = GetSolutionRoot();
        var filePath = Path.Combine(solutionRoot, "samples/PeasyPilot.XUnit.Samples/features/users.feature");

        // Act
        var feature = await loader.LoadFromFileAsync(filePath);

        // Assert
        Assert.NotNull(feature);
        Assert.Equal("User Management", feature.Name);
        Assert.NotEmpty(feature.Scenarios);
        Assert.True(feature.Scenarios.Count >= 4, "Should have at least 4 scenarios");
    }

    [Fact]
    public async Task LoadFromDirectoryAsync_ValidDirectory_ReturnsAllFeatures()
    {
        // Arrange
        var loader = new GherkinFeatureFileLoader();
        var solutionRoot = GetSolutionRoot();
        var dirPath = Path.Combine(solutionRoot, "samples/PeasyPilot.XUnit.Samples/features");

        // Act
        var features = await loader.LoadFromDirectoryAsync(dirPath);

        // Assert
        Assert.NotEmpty(features);
        Assert.True(features.Count >= 2, "Should find at least 2 feature files");

        var featureNames = features.Select(f => f.Name).ToList();
        Assert.Contains("User Management", featureNames);
        Assert.Contains("Order Processing", featureNames);
    }

    private static string GetSolutionRoot()
    {
        var testDir = AppContext.BaseDirectory;
        var di = new DirectoryInfo(testDir);
        while (di != null && !File.Exists(Path.Combine(di.FullName, "easy-peasy.slnx")))
        {
            di = di.Parent;
        }
        return di?.FullName ?? throw new InvalidOperationException("Could not find solution root");
    }

    [Fact]
    public async Task LoadFromFileAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var loader = new GherkinFeatureFileLoader();

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => loader.LoadFromFileAsync("nonexistent.feature"));
    }

    [Fact]
    public async Task LoadFromDirectoryAsync_NonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var loader = new GherkinFeatureFileLoader();

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => loader.LoadFromDirectoryAsync("nonexistent/directory"));
    }
}
