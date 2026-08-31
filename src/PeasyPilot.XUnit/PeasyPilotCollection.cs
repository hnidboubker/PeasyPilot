using Xunit;
namespace PeasyPilot.XUnit;
/// <summary>
/// Collection definition for shared fixtures across xUnit tests.
/// </summary>
[CollectionDefinition("PeasyPilot Collection")]
public class PeasyPilotCollection : ICollectionFixture<PeasyPilotTestBase>
{
    // This class has no code, and is never created. Its purpose is to define
    // the collection for xUnit.
}
