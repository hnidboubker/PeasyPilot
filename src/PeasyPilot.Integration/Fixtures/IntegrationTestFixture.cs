using Xunit;
namespace PeasyPilot.Integration.Fixtures;
public abstract class IntegrationTestFixture : IAsyncLifetime
{
    public abstract Task InitializeAsync();
    public abstract Task DisposeAsync();
}
