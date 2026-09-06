using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Rules;

public class DateTimeValueRule : ITypeShapeValueRule
{
    public bool CanHandle(Type type)
    {
        var coreType = Nullable.GetUnderlyingType(type) ?? type;
        return coreType == typeof(DateTime) || coreType == typeof(DateOnly) || coreType == typeof(TimeSpan);
    }

    public IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context)
    {
        var coreType = Nullable.GetUnderlyingType(type) ?? type;

        return coreType == typeof(DateTime)
            ? new[]
            {
                new ParameterValueCase("Now", "DateTime.UtcNow"),
                new ParameterValueCase("MinValue", "DateTime.MinValue")
            }
            : coreType == typeof(DateOnly)
            ? new[]
            {
                new ParameterValueCase("Today", "DateOnly.FromDateTime(DateTime.UtcNow)"),
                new ParameterValueCase("MinValue", "DateOnly.MinValue")
            }
            : new[]
            {
                new ParameterValueCase("Zero", "TimeSpan.Zero")
            };
    }
}
