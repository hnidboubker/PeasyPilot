using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Rules;

public class TypeShapeValueRuleRegistry
{
    private readonly List<ITypeShapeValueRule> _rules;

    public TypeShapeValueRuleRegistry()
    {
        _rules = new List<ITypeShapeValueRule>
        {
            new NullableValueRule(),
            new StringValueRule(),
            new BooleanValueRule(),
            new NumericValueRule(),
            new EnumValueRule(),
            new DateTimeValueRule(),
            new GuidValueRule(),
            new CollectionValueRule(),
            new FallbackValueRule()
        };
    }

    public IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context)
    {
        var rule = _rules.FirstOrDefault(r => r.CanHandle(type));
        if (rule is null)
        {
            return new[] { new ParameterValueCase("Nominal", "default", "No rule matched") };
        }

        return rule.GenerateCases(type, context);
    }
}
