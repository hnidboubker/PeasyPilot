namespace PeasyPilot.Core.Tests.Storage;

using TestContext = PeasyPilot.Core.Context.TestContext;
using ITestContext = PeasyPilot.Core.Abstractions.ITestContext;
using TestResult = PeasyPilot.Core.Models.TestResult;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Storage;
using Xunit;
using PeasyPilot.XUnit;

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
        XAssert.Equal(2, result.Count);
        XAssert.Contains(result, u => u.Name == "Alice");
        XAssert.Contains(result, u => u.Name == "Bob");
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
        XAssert.NotNull(user);
        XAssert.Equal("Bob", user.Name);
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
        XAssert.IsEmpty(result);
    }

    [Fact]
    public void TestCase_DefaultsToUnitKind()
    {
        var testCase = new TestCase { Name = "SampleTest" };
        XAssert.Equal(TestKind.Unit, testCase.Kind);
    }

    [Fact]
    public void TestResult_CanContainStandardizedTestFailure()
    {
        var failure = new TestFailure
        {
            Message = "XAssert.Equal failed",
            Expected = "5",
            Actual = "10",
            StackTrace = "at Method()"
        };

        var result = new TestResult
        {
            Name = "SampleTest",
            Failure = failure
        };

        XAssert.NotNull(result.Failure);
        XAssert.Equal("5", result.Failure.Expected);
        XAssert.Equal("10", result.Failure.Actual);
    }
}
