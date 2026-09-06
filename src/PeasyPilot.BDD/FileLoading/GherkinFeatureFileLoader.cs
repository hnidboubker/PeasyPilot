namespace PeasyPilot.BDD.FileLoading;

/// <summary>
/// Loads Gherkin feature files from disk using GherkinFeatureParser.
/// </summary>
public class GherkinFeatureFileLoader : IFeatureFileLoader
{
    private const string FeatureFileExtension = ".feature";

    /// <inheritdoc />
    public async Task<IReadOnlyList<Feature>> LoadFromDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        var features = new List<Feature>();
        var featureFiles = Directory.EnumerateFiles(
            directoryPath,
            $"*{FeatureFileExtension}",
            SearchOption.AllDirectories);

        foreach (var filePath in featureFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var feature = await LoadFromFileAsync(filePath, cancellationToken);
                features.Add(feature);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to load feature file: {filePath}", ex);
            }
        }

        return features.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<Feature> LoadFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Feature file not found: {filePath}");
        }

        if (!filePath.EndsWith(FeatureFileExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"File must have {FeatureFileExtension} extension: {filePath}");
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var feature = GherkinFeatureParser.Parse(content);

        return feature;
    }
}
