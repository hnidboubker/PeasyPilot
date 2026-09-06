using PeasyPilot.TestAssistant.Abstractions;
using System.Collections;

namespace PeasyPilot.TestAssistant.Rules;

public class CollectionValueRule : ITypeShapeValueRule
{
    public bool CanHandle(Type type) =>
        type.IsArray ||
        (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) ||
        type.GetInterfaces().Any(i => i == typeof(IEnumerable));

    public IReadOnlyList<ParameterValueCase> GenerateCases(Type type, ValueGenerationContext context) =>
        new[]
        {
            new ParameterValueCase("Empty", "new List<object>()", "Empty collection"),
            new ParameterValueCase("WithOneItem", "new List<object> { new() }", "Collection with one item")
        };
}
