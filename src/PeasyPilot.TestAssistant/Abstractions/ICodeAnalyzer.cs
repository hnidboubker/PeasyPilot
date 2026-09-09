using PeasyPilot.TestAssistant.Models;
namespace PeasyPilot.TestAssistant.Abstractions;
public interface ICodeAnalyzer
{
    Task<IReadOnlyList<MethodTestModel>> AnalyzeFileAsync(string filePath);
    Task<IReadOnlyList<MethodTestModel>> AnalyzeTypeAsync(Type type);
    Task<MethodTestModel?> AnalyzeMethodAsync(Type? type, string methodName);
}
