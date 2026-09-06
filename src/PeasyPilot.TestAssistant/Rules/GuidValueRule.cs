using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Rules;

public class GuidValueRule : ITypeShapeValueRule
{
    public bool CanHandle(Type type)
    {
        var coreType = Nullable.GetUnderlyingType(type) ?? type;
        return coreType == typeof(Guid);
    }

    public IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context) =>
        new[]
        {
            new ParameterValueCase("NewGuid", "Guid.NewGuid()"),
            new ParameterValueCase("Empty", "Guid.Empty")
        };
}
