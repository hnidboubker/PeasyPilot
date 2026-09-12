using PeasyPilot.Core.Tests.Abstractions;

namespace PeasyPilot.Core.Tests.Integration;

public class TestService : ITestService
{
    public string GetValue() => "test-value";
}
