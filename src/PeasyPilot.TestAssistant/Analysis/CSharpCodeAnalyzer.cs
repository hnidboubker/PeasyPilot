using Microsoft.CodeAnalysis.CSharp;
using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;
namespace PeasyPilot.TestAssistant.Analysis;
public class CSharpCodeAnalyzer : ICodeAnalyzer
{
    public async Task<IReadOnlyList<MethodTestModel>> AnalyzeFileAsync(string filePath)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException($"File: {filePath}");
        var source = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(source);
        return new List<MethodTestModel>().AsReadOnly();
    }
    public async Task<IReadOnlyList<MethodTestModel>> AnalyzeTypeAsync(Type type)
    {
        var methods = new List<MethodTestModel>();
        var pubMethods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
        foreach (var m in pubMethods.Where(x => x.DeclaringType == type && !x.Name.StartsWith("get_") && !x.Name.StartsWith("set_")))
            methods.Add(Extract(type, m));
        return await Task.FromResult(methods.AsReadOnly());
    }
    public async Task<MethodTestModel?> AnalyzeMethodAsync(Type? type, string methodName)
    {
        if (type == null) return null;
        var m = type.GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
        return await Task.FromResult(m != null ? Extract(type, m) : null);
    }
    private MethodTestModel Extract(Type type, System.Reflection.MethodInfo m)
    {
        var isAsync = m.ReturnType == typeof(Task) || (m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
        var pars = m.GetParameters().Select(p => new MethodParameterInfo { Name = p.Name ?? "p", TypeName = p.ParameterType.Name, ResolvedType = p.ParameterType, IsNullable = !p.ParameterType.IsValueType || Nullable.GetUnderlyingType(p.ParameterType) != null, IsInterface = p.ParameterType.IsInterface, IsValueType = p.ParameterType.IsValueType, IsCollection = typeof(System.Collections.IEnumerable).IsAssignableFrom(p.ParameterType) && p.ParameterType != typeof(string) }).ToList();
        var deps = type.GetConstructors().SelectMany(c => c.GetParameters()).Where(p => p.ParameterType.IsInterface).Select(p => new DependencyInfo { InterfaceName = p.ParameterType.Name, ResolvedType = p.ParameterType, InjectionType = "Constructor", IsRequired = true, SuggestedStrategy = "Moq.Mock" }).ToList();
        var scn = new List<TestableScenario> { new() { Type = ScenarioType.HappyPath, Description = $"Happy path", SuggestedTestName = $"{m.Name}_Valid", RiskLevel = RiskLevel.High } };
        if (pars.Any(p => p.IsValueType)) scn.Add(new() { Type = ScenarioType.Boundary, Description = "Boundary", RiskLevel = RiskLevel.High });
        if (pars.Any(p => p.IsNullable)) scn.Add(new() { Type = ScenarioType.Error, Description = "Null tests", RiskLevel = RiskLevel.High });
        return new MethodTestModel { TypeName = type.Name, ResolvedType = type, MethodName = m.Name, ReturnTypeName = m.ReturnType.Name, ResolvedReturnType = m.ReturnType, IsAsync = isAsync, IsPublic = m.IsPublic, IsStatic = m.IsStatic, Parameters = pars, Dependencies = deps, TestableScenarios = scn, ComplexityScore = Math.Min(1 + pars.Count + (deps.Count * 2), 10), EstimatedTestCases = Math.Max(scn.Count * 2, 3) };
    }
}
