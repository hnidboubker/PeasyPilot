using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Rules;

public class StringValueRule : ITypeShapeValueRule
{
    public bool CanHandle(Type type) => type == typeof(string);

    public IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context) =>
        new[]
        {
            new ParameterValueCase("Sample", "\"sample-value\"", "Sample string"),
            new ParameterValueCase("Empty", "string.Empty", "Empty string"),
            new ParameterValueCase("Whitespace", "\" \"", "Whitespace only"),
            new ParameterValueCase("VeryLong", "new string('a', 1000)", "Very long string")
        };
}
