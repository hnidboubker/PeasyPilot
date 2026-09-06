using System.Reflection;

namespace PeasyPilot.TestAssistant.Models;

public class ConstructorResolution
{
    public required ConstructorInfo Constructor { get; set; }

    public List<ResolvedParameter> Parameters { get; set; } = new();
}

public class ResolvedParameter
{
    public required ParameterInfo Parameter { get; set; }

    public required ParameterResolutionKind ResolutionKind { get; set; }

    public string? Description { get; set; }
}

public enum ParameterResolutionKind
{
    Primitive,
    ConcreteNewable,
    InterfaceOrAbstractNeedsMock,
    Unresolvable
}
