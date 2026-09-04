using System.Reflection;

namespace PeasyPilot.Generator;

/// <summary>
/// A single argument-value proposal for a scenario: either the "happy path" value
/// for a parameter, or a named edge-case variant (e.g. "Null", "Empty", "Zero").
/// </summary>
/// <param name="Suffix">Empty for the happy-path value; a short PascalCase label otherwise.</param>
/// <param name="Expression">A C# expression, as source text, producing the value.</param>
internal readonly record struct ParameterValue(string Suffix, string Expression);

/// <summary>
/// The happy-path value plus every edge-case variant derived from a parameter's
/// type signature (nullability, numeric range, enum members, empty collections).
/// This is purely mechanical: it reflects on the type, it does not infer business
/// meaning, so it cannot know whether a given edge case is actually reachable or
/// meaningful for the method under test — that judgment is left to whoever reviews
/// the generated file.
/// </summary>
internal readonly record struct ParameterPlan(ParameterValue Happy, IReadOnlyList<ParameterValue> Variants);

/// <summary>
/// Computes <see cref="ParameterPlan"/>s from reflection metadata alone.
/// </summary>
internal static class ParameterScenarios
{
    private static readonly NullabilityInfoContext NullabilityContext = new();

    private static readonly HashSet<Type> SignedNumericTypes =
    [
        typeof(int), typeof(long), typeof(short), typeof(sbyte),
        typeof(double), typeof(float), typeof(decimal)
    ];

    private static readonly HashSet<Type> AllNumericTypes =
    [
        typeof(int), typeof(long), typeof(short), typeof(sbyte),
        typeof(uint), typeof(ulong), typeof(ushort), typeof(byte),
        typeof(double), typeof(float), typeof(decimal)
    ];

    public static ParameterPlan Plan(ParameterInfo parameter)
    {
        var type = parameter.ParameterType;
        var happy = new ParameterValue(string.Empty, HappyExpression(type));
        var variants = new List<ParameterValue>();

        var underlying = Nullable.GetUnderlyingType(type);
        var isNullableValue = underlying is not null;
        var isNullableReference = !type.IsValueType && IsNullableReferenceParameter(parameter);

        if (isNullableValue || isNullableReference)
        {
            variants.Add(new ParameterValue("Null", "null"));
        }

        var coreType = underlying ?? type;

        if (coreType == typeof(string))
        {
            variants.Add(new ParameterValue("Empty", "string.Empty"));
        }
        else if (coreType.IsEnum)
        {
            foreach (var value in Enum.GetValues(coreType).Cast<object>().Take(5))
            {
                variants.Add(new ParameterValue(value.ToString() ?? "Value", $"{FormatTypeName(coreType)}.{value}"));
            }
        }
        else if (AllNumericTypes.Contains(coreType))
        {
            variants.Add(new ParameterValue("Zero", "0"));
            if (SignedNumericTypes.Contains(coreType))
            {
                variants.Add(new ParameterValue("Negative", "-1"));
            }
        }
        else if (TryGetEnumerableElementType(coreType, out var elementType))
        {
            variants.Add(new ParameterValue("EmptyCollection", $"new List<{FormatTypeName(elementType!)}>()"));
        }

        return new ParameterPlan(happy, variants);
    }

    private static string HappyExpression(Type type)
    {
        if (type == typeof(string)) return "\"sample-value\"";
        if (type == typeof(bool)) return "true";
        if (type == typeof(Guid)) return "Guid.NewGuid()";
        if (type == typeof(DateTime)) return "DateTime.UtcNow";
        if (type == typeof(DateTimeOffset)) return "DateTimeOffset.UtcNow";
        if (type == typeof(TimeSpan)) return "TimeSpan.FromMinutes(1)";
        if (type == typeof(CancellationToken)) return "CancellationToken.None";

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null) return HappyExpression(underlying);

        if (type.IsEnum)
        {
            var first = Enum.GetValues(type).Cast<object>().FirstOrDefault();
            return first is not null ? $"{FormatTypeName(type)}.{first}" : $"default({FormatTypeName(type)})";
        }

        if (AllNumericTypes.Contains(type)) return "1";

        if (TryGetEnumerableElementType(type, out var elementType))
        {
            return $"new List<{FormatTypeName(elementType!)}>()";
        }

        if (type.IsInterface || type.IsAbstract)
        {
            return $"({FormatTypeName(type)})new PeasyPilot.Moq.MockFactory().Create(typeof({FormatTypeName(type)}))";
        }

        if (type.IsClass)
        {
            return $"new PeasyPilot.Bogus.TestDataFactory().Create<{FormatTypeName(type)}>()";
        }

        return $"default({FormatTypeName(type)})!";
    }

    private static bool IsNullableReferenceParameter(ParameterInfo parameter)
    {
        try
        {
            return NullabilityContext.Create(parameter).ReadState == NullabilityState.Nullable;
        }
        catch
        {
            // Nullability metadata isn't always present (e.g. types from assemblies
            // built without NRT annotations) — treat as non-nullable rather than fail.
            return false;
        }
    }

    private static bool TryGetEnumerableElementType(Type type, out Type? elementType)
    {
        elementType = null;
        if (type == typeof(string)) return false;

        if (type.IsArray)
        {
            elementType = type.GetElementType();
            return elementType is not null;
        }

        var enumerableInterface = (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? type
                : null)
            ?? type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerableInterface is null) return false;

        elementType = enumerableInterface.GetGenericArguments()[0];
        return true;
    }

    public static string FormatTypeName(Type type)
    {
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(short)) return "short";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(sbyte)) return "sbyte";
        if (type == typeof(uint)) return "uint";
        if (type == typeof(ulong)) return "ulong";
        if (type == typeof(ushort)) return "ushort";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(string)) return "string";
        if (type == typeof(object)) return "object";
        if (type == typeof(void)) return "void";

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null) return $"{FormatTypeName(underlying)}?";

        if (type.IsArray)
        {
            return $"{FormatTypeName(type.GetElementType()!)}[]";
        }

        if (type.IsGenericType)
        {
            var backtick = type.Name.IndexOf('`');
            var name = backtick >= 0 ? type.Name[..backtick] : type.Name;
            var args = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
            return $"{name}<{args}>";
        }

        return type.Name;
    }
}
