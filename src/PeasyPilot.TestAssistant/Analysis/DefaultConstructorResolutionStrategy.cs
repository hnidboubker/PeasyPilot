using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Analysis;

public class DefaultConstructorResolutionStrategy : IConstructorResolutionStrategy
{
    public ConstructorResolution Resolve(Type targetType)
    {
        var constructors = targetType.GetConstructors();
        var selectedCtor = constructors
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (selectedCtor is null)
        {
            throw new InvalidOperationException($"No public constructor found for type {targetType.Name}");
        }

        var resolvedParams = selectedCtor.GetParameters()
            .Select(p => new ResolvedParameter
            {
                Parameter = p,
                ResolutionKind = ClassifyParameter(p.ParameterType),
                Description = $"Parameter '{p.Name}' of type '{p.ParameterType.Name}'"
            })
            .ToList();

        return new ConstructorResolution
        {
            Constructor = selectedCtor,
            Parameters = resolvedParams
        };
    }

    private static ParameterResolutionKind ClassifyParameter(Type paramType)
    {
        if (IsNumericType(paramType) || paramType == typeof(string) || paramType == typeof(bool) ||
            paramType == typeof(DateTime) || paramType == typeof(Guid))
        {
            return ParameterResolutionKind.Primitive;
        }

        if (paramType.IsInterface || paramType.IsAbstract)
        {
            return ParameterResolutionKind.InterfaceOrAbstractNeedsMock;
        }

        if (paramType.IsValueType)
        {
            return ParameterResolutionKind.Primitive;
        }

        if (HasParameterlessConstructor(paramType))
        {
            return ParameterResolutionKind.ConcreteNewable;
        }

        return ParameterResolutionKind.Unresolvable;
    }

    private static bool IsNumericType(Type type)
    {
        var coreType = Nullable.GetUnderlyingType(type) ?? type;
        return coreType == typeof(byte) || coreType == typeof(sbyte) || coreType == typeof(short) ||
               coreType == typeof(ushort) || coreType == typeof(int) || coreType == typeof(uint) ||
               coreType == typeof(long) || coreType == typeof(ulong) || coreType == typeof(float) ||
               coreType == typeof(double) || coreType == typeof(decimal);
    }

    private static bool HasParameterlessConstructor(Type type)
    {
        return type.GetConstructor(Type.EmptyTypes) is not null;
    }
}
