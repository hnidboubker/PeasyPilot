using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Rules;

public class NullableValueRule : ITypeShapeValueRule
{
    public bool CanHandle(Type type) =>
        type.IsValueType && Nullable.GetUnderlyingType(type) != null ||
        (!type.IsValueType && !type.IsArray && type != typeof(string));

    public IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context)
    {
        return new[] { new ParameterValueCase("Null", "null", "Null reference/nullable value") };
    }
}
