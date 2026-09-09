using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Analysis;

/// <summary>
/// Analyzes C# code using Roslyn to extract testable patterns and metadata.
/// </summary>
public class CSharpCodeAnalyzer : ICodeAnalyzer
{
    private readonly DependencyAnalyzer _dependencyAnalyzer = new();

    /// <summary>
    /// Analyzes a C# source file and extracts testable methods.
    /// </summary>
    public async Task<IReadOnlyList<MethodTestModel>> AnalyzeFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var sourceCode = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = await tree.GetRootAsync();

        var methods = new List<MethodTestModel>();

        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classDecl in classDeclarations)
        {
            var className = classDecl.Identifier.Text;
            var methodDeclarations = classDecl.Members.OfType<MethodDeclarationSyntax>();

            foreach (var methodDecl in methodDeclarations)
            {
                var model = ExtractMethodMetadata(className, classDecl, methodDecl);
                if (model != null)
                {
                    methods.Add(model);
                }
            }
        }

        return methods.AsReadOnly();
    }

    /// <summary>
    /// Analyzes a .NET type and extracts testable methods.
    /// </summary>
    public async Task<IReadOnlyList<MethodTestModel>> AnalyzeTypeAsync(Type type)
    {
        var methods = new List<MethodTestModel>();

        var publicMethods = type.GetMethods(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);

        foreach (var method in publicMethods)
        {
            if (method.DeclaringType == type && !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_"))
            {
                var model = ExtractMethodMetadataFromReflection(type, method);
                methods.Add(model);
            }
        }

        return await Task.FromResult(methods.AsReadOnly());
    }

    /// <summary>
    /// Analyzes a specific method and extracts testable scenarios.
    /// </summary>
    public async Task<MethodTestModel?> AnalyzeMethodAsync(Type type, string methodName)
    {
        var method = type.GetMethod(methodName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);

        if (method == null)
        {
            return null;
        }

        return await Task.FromResult(ExtractMethodMetadataFromReflection(type, method));
    }

    /// <summary>
    /// Extracts method metadata from a Roslyn MethodDeclarationSyntax.
    /// </summary>
    private MethodTestModel? ExtractMethodMetadata(string className, ClassDeclarationSyntax classDecl, MethodDeclarationSyntax methodDecl)
    {
        var methodName = methodDecl.Identifier.Text;
        var returnType = methodDecl.ReturnType.ToString();
        var isAsync = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
        var isPublic = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        var isStatic = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        if (!isPublic)
        {
            return null;
        }

        var parameters = ExtractParameters(methodDecl);
        var exceptions = ExtractExceptions(methodDecl);
        var dependencies = ExtractConstructorDependencies(classDecl);
        var scenarios = GenerateTestableScenarios(methodName, parameters, exceptions, dependencies);

        var complexityScore = CalculateComplexity(parameters.Count, exceptions.Count, dependencies.Count);
        var estimatedTestCases = EstimateTestCases(scenarios);

        return new MethodTestModel
        {
            TypeName = className,
            MethodName = methodName,
            ReturnTypeName = returnType,
            IsAsync = isAsync,
            IsPublic = isPublic,
            IsStatic = isStatic,
            Parameters = parameters,
            Dependencies = dependencies,
            PossibleExceptions = exceptions,
            TestableScenarios = scenarios,
            ComplexityScore = complexityScore,
            EstimatedTestCases = estimatedTestCases
        };
    }

    /// <summary>
    /// Extracts method metadata from .NET Reflection.
    /// </summary>
    private MethodTestModel ExtractMethodMetadataFromReflection(Type type, System.Reflection.MethodInfo method)
    {
        var parameters = ExtractParametersFromReflection(method);
        var exceptions = ExtractExceptionsFromReflection(method);
        var dependencies = ExtractConstructorDependenciesFromReflection(type);
        var scenarios = GenerateTestableScenarios(method.Name, parameters, exceptions, dependencies);

        var isAsync = method.ReturnType == typeof(Task) ||
                     (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));

        var complexityScore = CalculateComplexity(parameters.Count, exceptions.Count, dependencies.Count);
        var estimatedTestCases = EstimateTestCases(scenarios);

        return new MethodTestModel
        {
            TypeName = type.Name,
            ResolvedType = type,
            MethodName = method.Name,
            ReturnTypeName = method.ReturnType.Name,
            ResolvedReturnType = method.ReturnType,
            IsAsync = isAsync,
            IsPublic = method.IsPublic,
            IsStatic = method.IsStatic,
            Parameters = parameters,
            Dependencies = dependencies,
            PossibleExceptions = exceptions,
            TestableScenarios = scenarios,
            ComplexityScore = complexityScore,
            EstimatedTestCases = estimatedTestCases
        };
    }

    /// <summary>
    /// Extracts method parameters from Roslyn syntax.
    /// </summary>
    private List<MethodParameterInfo> ExtractParameters(MethodDeclarationSyntax methodDecl)
    {
        var parameters = new List<MethodParameterInfo>();

        foreach (var param in methodDecl.ParameterList.Parameters)
        {
            var paramName = param.Identifier.Text;
            var paramType = param.Type?.ToString() ?? "object";
            var isNullable = paramType.EndsWith("?");

            parameters.Add(new MethodParameterInfo
            {
                Name = paramName,
                TypeName = paramType,
                IsNullable = isNullable,
                IsInterface = paramType.Contains("I") && char.IsUpper(paramType[0]),
                IsCollection = paramType.Contains("IEnumerable") || paramType.Contains("List") || paramType.Contains("[]"),
                IsValueType = IsValueTypeString(paramType),
                SuggestedTestValues = GenerateSuggestedValues(paramType)
            });
        }

        return parameters;
    }

    /// <summary>
    /// Extracts method parameters from .NET Reflection.
    /// </summary>
    private List<MethodParameterInfo> ExtractParametersFromReflection(System.Reflection.MethodInfo method)
    {
        var parameters = new List<MethodParameterInfo>();

        foreach (var param in method.GetParameters())
        {
            var paramType = param.ParameterType;
            // Nullable if: value type with nullable wrapper OR reference type (string, class, interface)
            var isNullable = Nullable.GetUnderlyingType(paramType) != null || !paramType.IsValueType;
            var isInterface = paramType.IsInterface;

            parameters.Add(new MethodParameterInfo
            {
                Name = param.Name ?? "param",
                TypeName = paramType.FullName ?? paramType.Name,
                ResolvedType = paramType,
                IsNullable = isNullable,
                IsInterface = isInterface,
                ResolvedInterfaceType = isInterface ? paramType : null,
                IsCollection = typeof(System.Collections.IEnumerable).IsAssignableFrom(paramType) && paramType != typeof(string),
                IsValueType = paramType.IsValueType,
                SuggestedTestValues = GenerateSuggestedValuesForType(paramType)
            });
        }

        return parameters;
    }

    /// <summary>
    /// Extracts possible exceptions from method attributes or syntax.
    /// </summary>
    private List<ExceptionInfo> ExtractExceptions(MethodDeclarationSyntax methodDecl)
    {
        var exceptions = new List<ExceptionInfo>();

        // Look for throw statements
        var throwStatements = methodDecl.DescendantNodes().OfType<ThrowStatementSyntax>();
        foreach (var throwStmt in throwStatements)
        {
            if (throwStmt.Expression is ObjectCreationExpressionSyntax objCreation)
            {
                var exceptionType = objCreation.Type.ToString();
                exceptions.Add(new ExceptionInfo
                {
                    ExceptionTypeName = exceptionType,
                    Description = $"Thrown by {methodDecl.Identifier.Text}"
                });
            }
        }

        return exceptions;
    }

    /// <summary>
    /// Extracts possible exceptions from .NET Reflection.
    /// </summary>
    private List<ExceptionInfo> ExtractExceptionsFromReflection(System.Reflection.MethodInfo method)
    {
        var exceptions = new List<ExceptionInfo>();

        var throwsAttribute = method.GetCustomAttributes(false)
            .FirstOrDefault(a => a.GetType().Name == "ThrowsAttribute");

        if (throwsAttribute != null)
        {
            // Custom attribute handling would go here
        }

        return exceptions;
    }

    /// <summary>
    /// Extracts constructor dependencies from a class declaration.
    /// </summary>
    private List<DependencyInfo> ExtractConstructorDependencies(ClassDeclarationSyntax classDecl)
    {
        var dependencies = new List<DependencyInfo>();

        var constructors = classDecl.Members.OfType<ConstructorDeclarationSyntax>();
        foreach (var constructor in constructors)
        {
            foreach (var param in constructor.ParameterList.Parameters)
            {
                var paramType = param.Type?.ToString() ?? "";
                var paramName = param.Identifier.Text;

                if (paramType.StartsWith("I") && char.IsUpper(paramType[1]))
                {
                    dependencies.Add(new DependencyInfo
                    {
                        InterfaceName = paramType,
                        InjectionType = "Constructor",
                        IsRequired = !param.Default?.Equals(SyntaxKind.NullKeyword) ?? true,
                        SuggestedStrategy = "Moq.Mock"
                    });
                }
            }
        }

        return dependencies;
    }

    /// <summary>
    /// Extracts constructor dependencies from .NET Reflection.
    /// </summary>
    private List<DependencyInfo> ExtractConstructorDependenciesFromReflection(Type type)
    {
        var dependencies = new List<DependencyInfo>();

        var constructors = type.GetConstructors();
        foreach (var constructor in constructors)
        {
            foreach (var param in constructor.GetParameters())
            {
                if (param.ParameterType.IsInterface)
                {
                    dependencies.Add(new DependencyInfo
                    {
                        InterfaceName = param.ParameterType.FullName ?? param.ParameterType.Name,
                        ResolvedType = param.ParameterType,
                        InjectionType = "Constructor",
                        IsRequired = !param.HasDefaultValue,
                        SuggestedStrategy = "Moq.Mock"
                    });
                }
            }
        }

        return dependencies;
    }

    /// <summary>
    /// Generates testable scenarios based on method characteristics.
    /// </summary>
    private List<TestableScenario> GenerateTestableScenarios(
        string methodName,
        List<MethodParameterInfo> parameters,
        List<ExceptionInfo> exceptions,
        List<DependencyInfo> dependencies)
    {
        var scenarios = new List<TestableScenario>
        {
            new()
            {
                Type = ScenarioType.HappyPath,
                Description = $"Happy path for {methodName} with valid inputs",
                SuggestedTestName = $"{methodName}_WithValidInputs_ReturnsExpected",
                RiskLevel = RiskLevel.High,
                TestCategory = "Unit"
            }
        };

        // Add boundary test if there are numeric parameters
        var hasNumericParam = parameters.Any(p => p.IsValueType && IsNumericType(p.TypeName));
        if (hasNumericParam)
        {
            scenarios.Add(new()
            {
                Type = ScenarioType.Boundary,
                Description = $"Boundary conditions for {methodName} (zero, negative, max values)",
                SuggestedTestName = $"{methodName}_WithBoundaryValues_HandlesCorrectly",
                RiskLevel = RiskLevel.High
            });
        }

        // Add error test if there are exceptions
        if (exceptions.Any())
        {
            scenarios.Add(new()
            {
                Type = ScenarioType.Error,
                Description = $"Error handling for {methodName} when exceptions occur",
                SuggestedTestName = $"{methodName}_WhenExceptionThrown_PropagatesCorrectly",
                RiskLevel = RiskLevel.Critical
            });
        }

        // Add dependency failure test if there are mocked dependencies
        if (dependencies.Any())
        {
            scenarios.Add(new()
            {
                Type = ScenarioType.DependencyFailure,
                Description = $"Dependency failure handling for {methodName}",
                SuggestedTestName = $"{methodName}_WhenDependencyFails_HandlesGracefully",
                RiskLevel = RiskLevel.High
            });
        }

        // Add null/empty parameter test
        if (parameters.Any(p => p.IsNullable))
        {
            scenarios.Add(new()
            {
                Type = ScenarioType.Error,
                Description = $"Null/empty parameter handling for {methodName}",
                SuggestedTestName = $"{methodName}_WithNullParameter_ThrowsOrHandles",
                RiskLevel = RiskLevel.High
            });
        }

        return scenarios;
    }

    /// <summary>
    /// Calculates a complexity score for a method.
    /// </summary>
    private int CalculateComplexity(int parameterCount, int exceptionCount, int dependencyCount)
    {
        var score = 1;
        score += parameterCount;
        score += exceptionCount * 2;
        score += dependencyCount * 3;
        return Math.Min(score, 10);
    }

    /// <summary>
    /// Estimates the number of test cases needed.
    /// </summary>
    private int EstimateTestCases(List<TestableScenario> scenarios)
    {
        return Math.Max(scenarios.Count * 2, 3);
    }

    /// <summary>
    /// Determines if a type string represents a value type.
    /// </summary>
    private bool IsValueTypeString(string typeString)
    {
        return typeString switch
        {
            "int" or "long" or "short" or "byte" or "decimal" or "double" or "float" or "bool" or "char" => true,
            _ => typeString.EndsWith("?") && IsValueTypeString(typeString.Substring(0, typeString.Length - 1))
        };
    }

    /// <summary>
    /// Generates suggested test values for a parameter type.
    /// </summary>
    private List<string> GenerateSuggestedValues(string typeString)
    {
        return typeString switch
        {
            "int" => new() { "0", "1", "-1", "int.MinValue", "int.MaxValue" },
            "string" => new() { "\"valid\"", "\"\"", "null" },
            "bool" => new() { "true", "false" },
            "decimal" or "double" or "float" => new() { "0", "1.5", "-1.5", "decimal.MaxValue" },
            _ => new() { "default" }
        };
    }

    /// <summary>
    /// Determines if a type string represents a numeric type.
    /// </summary>
    private bool IsNumericType(string typeString)
    {
        return typeString.Contains("int") || typeString.Contains("Int")
            || typeString.Contains("long") || typeString.Contains("Long")
            || typeString.Contains("short") || typeString.Contains("Short")
            || typeString.Contains("decimal") || typeString.Contains("Decimal")
            || typeString.Contains("double") || typeString.Contains("Double")
            || typeString.Contains("float") || typeString.Contains("Single")
            || typeString.Contains("byte") || typeString.Contains("Byte");
    }

    /// <summary>
    /// Generates suggested test values for a CLR type.
    /// </summary>
    private List<string> GenerateSuggestedValuesForType(Type type)
    {
        if (type == typeof(int))
            return new() { "0", "1", "-1" };

        if (type == typeof(string))
            return new() { "\"test\"", "\"\"", "null" };

        if (type == typeof(bool))
            return new() { "true", "false" };

        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
            return new() { "0", "1.5", "-1.5" };

        return new() { "null" };
    }
}
