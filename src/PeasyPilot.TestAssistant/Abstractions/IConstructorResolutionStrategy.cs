using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Abstractions;

public interface IConstructorResolutionStrategy
{
    ConstructorResolution Resolve(Type targetType);
}
