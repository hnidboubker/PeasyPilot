using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Rules;

public class NumericValueRule : ITypeShapeValueRule
{
    private static readonly HashSet<Type> NumericTypes = new()
    {
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
        typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal)
    };

    public bool CanHandle(Type type)
    {
        var coreType = Nullable.GetUnderlyingType(type) ?? type;
        return NumericTypes.Contains(coreType);
    }

    public IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context)
    {
        var coreType = Nullable.GetUnderlyingType(type) ?? type;
        var cases = new List<ParameterValueCase>
        {
            new ParameterValueCase("Zero", "0"),
        };

        var isSignedNumeric = coreType is not null &&
            (coreType == typeof(sbyte) || coreType == typeof(short) ||
             coreType == typeof(int) || coreType == typeof(long) ||
             coreType == typeof(float) || coreType == typeof(double) ||
             coreType == typeof(decimal));

        if (isSignedNumeric)
        {
            cases.Add(new ParameterValueCase("Negative", "-1"));
        }

        return cases;
    }
}
