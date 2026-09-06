using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Rules;

public class FallbackValueRule : ITypeShapeValueRule
{
    public bool CanHandle(Type type) => true;

    public IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context) =>
        new[]
        {
            new ParameterValueCase(
                "Default",
                "default",
                $"TODO: provide a value for type '{type.Name}' — could not infer a suitable default"
            )
        };
}
