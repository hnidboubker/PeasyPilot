namespace PeasyPilot.TestAssistant.Abstractions;

public interface ITypeShapeValueRule
{
    bool CanHandle(Type type);

    IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context);
}

public record ParameterValueCase(
    string VariantName,
    string ValueExpression,
    string? Description = null);

public record ValueGenerationContext(int MaxEnumCases = 8);
