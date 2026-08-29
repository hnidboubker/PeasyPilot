namespace PeasyPilot.Integration.Fixtures;

using Xunit;

public abstract class IntegrationTestFixture : IAsyncLifetime
{
    public abstract Task InitializeAsync();
    public abstract Task DisposeAsync();
}
