using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Models;
using Xunit;

namespace PeasyPilot.Core.Tests.TestAssistant;

/// <summary>
/// Tests for CSharpCodeAnalyzer - Tier 1 of Phase 5
/// </summary>
public class CodeAnalysisTests
{
    private readonly CSharpCodeAnalyzer _analyzer = new();

    #region Type Analysis Tests

    [Fact]
    public async Task AnalyzeTypeAsync_WithSimpleType_ExtractsAllPublicMethods()
    {
        // Arrange
        var type = typeof(SimpleTestClass);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, m => Assert.True(m.IsPublic));
    }

    [Fact]
    public async Task AnalyzeTypeAsync_WithMethodWithParameters_ExtractsParameterInfo()
    {
        // Arrange
        var type = typeof(MethodWithParametersClass);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);
        var method = result.FirstOrDefault(m => m.MethodName == nameof(MethodWithParametersClass.Add));

        // Assert
        Assert.NotNull(method);
        Assert.Equal(2, method.Parameters.Count);
        Assert.Equal("a", method.Parameters[0].Name);
        Assert.Equal("b", method.Parameters[1].Name);
    }

    [Fact]
    public async Task AnalyzeTypeAsync_WithAsyncMethod_MarksAsAsync()
    {
        // Arrange
        var type = typeof(AsyncMethodClass);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);
        var method = result.FirstOrDefault(m => m.MethodName == nameof(AsyncMethodClass.ProcessAsync));

        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsAsync);
    }

    #endregion

    #region Method Analysis Tests

    [Fact]
    public async Task AnalyzeMethodAsync_WithValidMethod_ReturnsMethodMetadata()
    {
        // Arrange
        var type = typeof(SimpleTestClass);
        var methodName = nameof(SimpleTestClass.SimpleMethod);

        // Act
        var result = await _analyzer.AnalyzeMethodAsync(type, methodName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(methodName, result.MethodName);
        Assert.Equal(type.Name, result.TypeName);
    }

    [Fact]
    public async Task AnalyzeMethodAsync_WithNonExistentMethod_ReturnsNull()
    {
        // Arrange
        var type = typeof(SimpleTestClass);

        // Act
        var result = await _analyzer.AnalyzeMethodAsync(type, "NonExistentMethod");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Parameter Extraction Tests

    [Fact]
    public async Task AnalyzeTypeAsync_WithIntParameters_IdentifiesAsValueType()
    {
        // Arrange
        var type = typeof(MethodWithParametersClass);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);
        var method = result.FirstOrDefault(m => m.MethodName == nameof(MethodWithParametersClass.Add));

        // Assert
        Assert.NotNull(method);
        Assert.All(method.Parameters, p => Assert.True(p.IsValueType));
    }

    [Fact]
    public async Task AnalyzeTypeAsync_WithStringParameter_IdentifiesAsNullable()
    {
        // Arrange
        var type = typeof(StringParameterClass);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);
        var method = result.FirstOrDefault(m => m.MethodName == nameof(StringParameterClass.ProcessString));

        // Assert
        Assert.NotNull(method);
        Assert.Single(method.Parameters);
        Assert.True(method.Parameters[0].IsNullable);
    }

    #endregion

    #region Dependency Detection Tests

    [Fact]
    public async Task AnalyzeTypeAsync_WithInterfaceDependency_DetectsDependency()
    {
        // Arrange
        var type = typeof(ClassWithDependency);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);

        // Assert
        Assert.NotEmpty(result);
        var method = result.FirstOrDefault();
        Assert.NotNull(method);
        Assert.NotEmpty(method.Dependencies);
        Assert.Contains(method.Dependencies, d => d.InterfaceName.Contains("ILogger"));
    }

    #endregion

    #region Testable Scenario Generation Tests

    [Fact]
    public async Task AnalyzeTypeAsync_GeneratesTestableScenarios_IncludesHappyPath()
    {
        // Arrange
        var type = typeof(SimpleTestClass);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);

        // Assert
        Assert.NotEmpty(result);
        var method = result.First();
        Assert.NotEmpty(method.TestableScenarios);
        Assert.Contains(method.TestableScenarios, s => s.Type == ScenarioType.HappyPath);
    }

    [Fact]
    public async Task AnalyzeTypeAsync_WithNumericParameters_GeneratesBoundaryTests()
    {
        // Arrange
        var type = typeof(MethodWithParametersClass);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);
        var method = result.FirstOrDefault(m => m.MethodName == nameof(MethodWithParametersClass.Add));

        // Assert
        Assert.NotNull(method);
        Assert.Contains(method.TestableScenarios, s => s.Type == ScenarioType.Boundary);
    }

    [Fact]
    public async Task AnalyzeTypeAsync_WithNullableParameters_GeneratesNullErrorTests()
    {
        // Arrange
        var type = typeof(StringParameterClass);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);
        var method = result.FirstOrDefault(m => m.MethodName == nameof(StringParameterClass.ProcessString));

        // Assert
        Assert.NotNull(method);
        var errorScenarios = method.TestableScenarios.Where(s => s.Type == ScenarioType.Error).ToList();
        Assert.NotEmpty(errorScenarios);
    }

    #endregion

    #region Complexity Scoring Tests

    [Fact]
    public async Task AnalyzeTypeAsync_SimpleMethod_LowComplexityScore()
    {
        // Arrange
        var type = typeof(SimpleTestClass);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);
        var method = result.FirstOrDefault();

        // Assert
        Assert.NotNull(method);
        Assert.True(method.ComplexityScore <= 3, $"Expected low complexity, got {method.ComplexityScore}");
    }

    [Fact]
    public async Task AnalyzeTypeAsync_ComplexMethod_HighComplexityScore()
    {
        // Arrange
        var type = typeof(ClassWithDependency);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);

        // Assert
        Assert.NotEmpty(result);
        Assert.True(result.Any(m => m.ComplexityScore > 3));
    }

    #endregion

    #region Estimated Test Cases Tests

    [Fact]
    public async Task AnalyzeTypeAsync_EstimatesTestCases()
    {
        // Arrange
        var type = typeof(SimpleTestClass);

        // Act
        var result = await _analyzer.AnalyzeTypeAsync(type);

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, m => Assert.True(m.EstimatedTestCases > 0));
    }

    #endregion

    #region Test Helper Classes

    public class SimpleTestClass
    {
        public void SimpleMethod()
        {
        }

        public int GetValue()
        {
            return 42;
        }
    }

    public class MethodWithParametersClass
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

    public class AsyncMethodClass
    {
        public async Task ProcessAsync()
        {
            await Task.Delay(10);
        }

        public async Task<string> GetDataAsync()
        {
            await Task.Delay(10);
            return "data";
        }
    }

    public class StringParameterClass
    {
        public void ProcessString(string? text)
        {
        }
    }

    public interface ILogger
    {
        void Log(string message);
    }

    public class ClassWithDependency
    {
        private readonly ILogger _logger;

        public ClassWithDependency(ILogger logger)
        {
            _logger = logger;
        }

        public void DoSomething()
        {
            _logger.Log("Doing something");
        }
    }

    #endregion
}
