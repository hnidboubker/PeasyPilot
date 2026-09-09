using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Abstractions;

/// <summary>
/// Analyzes C# source code to extract testable patterns and metadata.
/// </summary>
public interface ICodeAnalyzer
{
    /// <summary>
    /// Analyzes a source file and extracts testable methods.
    /// </summary>
    /// <param name="filePath">Path to the C# source file</param>
    /// <returns>List of analyzable methods with metadata</returns>
    Task<IReadOnlyList<MethodTestModel>> AnalyzeFileAsync(string filePath);

    /// <summary>
    /// Analyzes a type and extracts testable methods.
    /// </summary>
    /// <param name="type">The type to analyze</param>
    /// <returns>List of analyzable methods for the type</returns>
    Task<IReadOnlyList<MethodTestModel>> AnalyzeTypeAsync(Type type);

    /// <summary>
    /// Analyzes a method and extracts testable scenarios.
    /// </summary>
    /// <param name="type">The type containing the method</param>
    /// <param name="methodName">The method name</param>
    /// <returns>Method metadata with testable scenarios</returns>
    Task<MethodTestModel?> AnalyzeMethodAsync(Type type, string methodName);
}
