using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Rules;

public class EnumValueRule : ITypeShapeValueRule
{
    public bool CanHandle(Type type)
    {
        var coreType = Nullable.GetUnderlyingType(type) ?? type;
        return coreType.IsEnum;
    }

    public IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context)
    {
        var coreType = Nullable.GetUnderlyingType(type) ?? type;
        var values = Enum.GetValues(coreType).Cast<object>().Take(context.MaxEnumCases).ToList();

        var cases = values.Select((val, idx) =>
            new ParameterValueCase(
                $"Member{idx}",
                $"{coreType.Name}.{Enum.GetName(coreType, val)}"
            )
        ).ToList();

        return cases;
    }
}
