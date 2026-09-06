namespace PeasyPilot.BDD.FileLoading;

/// <summary>
/// Loads Gherkin feature files from disk and converts them to Feature objects.
/// </summary>
public interface IFeatureFileLoader
{
    /// <summary>
    /// Loads all .feature files from a directory recursively.
    /// </summary>
    /// <param name="directoryPath">Path to features directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of loaded features.</returns>
    Task<IReadOnlyList<Feature>> LoadFromDirectoryAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a single .feature file.
    /// </summary>
    /// <param name="filePath">Path to feature file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Loaded feature.</returns>
    Task<Feature> LoadFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}
