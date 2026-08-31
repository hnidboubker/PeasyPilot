namespace PeasyPilot.Core.Tests.Storage;

using PeasyPilot.Core.Models;
using PeasyPilot.Core.Storage;
using Xunit;
using Assert = Xunit.Assert;

public class InMemoryTestStoreTests
{
    private record SampleUser(int Id, string Name, string Email);

    [Fact]
    public async Task SeedAndGetAll_ReturnsSeededEntities()
    {
        // Arrange
        var store = new InMemoryTestStore();
        var users = new[]
        {
            new SampleUser(1, "Alice", "alice@example.com"),
            new SampleUser(2, "Bob", "bob@example.com")
        };

        // Act
        await store.SeedAsync(users);
        var result = await store.GetAllAsync<SampleUser>();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Name == "Alice");
        Assert.Contains(result, u => u.Name == "Bob");
    }

    [Fact]
    public async Task FindAsync_MatchesPredicate_ReturnsEntity()
    {
        // Arrange
        var store = new InMemoryTestStore();
        await store.SeedAsync([
            new SampleUser(1, "Alice", "alice@example.com"),
            new SampleUser(2, "Bob", "bob@example.com")
        ]);

        // Act
        var user = await store.FindAsync<SampleUser>(u => u.Id == 2);

        // Assert
        Assert.NotNull(user);
        Assert.Equal("Bob", user.Name);
    }

    [Fact]
    public async Task ResetAsync_ClearsStoredEntities()
    {
        // Arrange
        var store = new InMemoryTestStore();
        await store.SeedAsync([new SampleUser(1, "Alice", "alice@example.com")]);

        // Act
        await store.ResetAsync();
        var result = await store.GetAllAsync<SampleUser>();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void TestCase_DefaultsToUnitKind()
    {
        var testCase = new TestCase { Name = "SampleTest" };
        Assert.Equal(TestKind.Unit, testCase.Kind);
    }

    [Fact]
    public void TestResult_CanContainStandardizedTestFailure()
    {
        var failure = new TestFailure
        {
            Message = "Assert.Equal failed",
            Expected = "5",
            Actual = "10",
            StackTrace = "at Method()"
        };

        var result = new TestResult
        {
            Name = "SampleTest",
            Failure = failure
        };

        Assert.NotNull(result.Failure);
        Assert.Equal("5", result.Failure.Expected);
        Assert.Equal("10", result.Failure.Actual);
    }
}
