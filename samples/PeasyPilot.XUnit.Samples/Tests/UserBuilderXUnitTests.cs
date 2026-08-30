namespace PeasyPilot.XUnit.Samples;

using Xunit;
using PeasyPilot.XUnit;
using PeasyPilot.Unit.Builders;
using PeasyPilot.Unit.Fixtures;
using PeasyPilot.XUnit.Samples.Services;

/// <summary>
/// Tests demonstrating the builder pattern with xUnit.
/// </summary>
public class UserBuilderXUnitTests : PeasyPilotTestBase
{
    private UserService _service = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _service = new UserService();
    }

    [Fact]
    public void BuildUser_WithAllProperties_CreatesCompleteUser()
    {
        // Arrange
        var user = new UserBuilder()
            .WithName("Alice Johnson")
            .WithEmail("alice@example.com")
            .Build();

        // Act
        var result = _service.Create(user);

        // Assert
        Assert.Equal("Alice Johnson", result.Name);
        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public void BuildUser_WithFluentApi_SimplifiesObjectCreation()
    {
        // Arrange & Act
        var user = new UserBuilder()
            .WithName("Bob")
            .WithEmail("bob@test.com")
            .Build();

        // Assert
        Assert.NotNull(user);
        Assert.Equal("Bob", user.Name);
    }

    [Fact]
    public void BuildUser_ResetClears_PreviousConfiguration()
    {
        // Arrange
        var builder = new UserBuilder()
            .WithName("Original")
            .WithEmail("original@example.com");

        // Act
        builder.Reset();
        var user = builder
            .WithName("New Name")
            .WithEmail("new@example.com")
            .Build();

        // Assert
        Assert.Equal("New Name", user.Name);
        Assert.Equal("new@example.com", user.Email);
    }

    [Fact]
    public void BuildUser_WithMinimalData_StillValid()
    {
        // Arrange & Act
        var user = new UserBuilder()
            .WithName("Minimal User")
            .Build();

        // Assert
        Assert.Equal("Minimal User", user.Name);
        Assert.Empty(user.Email);
    }
}
