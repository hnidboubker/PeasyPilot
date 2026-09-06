using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Rules;

public class BooleanValueRule : ITypeShapeValueRule
{
    public bool CanHandle(Type type) => type == typeof(bool);

    public IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context) =>
        new[]
        {
            new ParameterValueCase("True", "true"),
            new ParameterValueCase("False", "false")
        };
}
