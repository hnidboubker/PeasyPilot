namespace PeasyPilot.Core.Tests.Context;

using PeasyPilot.Core.Context;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Tests for the TestContext class.
/// </summary>
public class TestContextTests
{
    /// <summary>
    /// Verifies that TestContext stores and retrieves data thread-safely.
    /// </summary>
    [Fact]
    public void TestContext_ShouldStoreAndRetrieveDataThreadSafely()
    {
        var context = new TestContext();

        Parallel.For(0, 100, i =>
        {
            var value = context.GetOrAdd("key", () => "value");
            Assert.Equal("value", value);
        });
    }
}
