namespace PeasyPilot.Core.Tests.Context;

using TestContext = PeasyPilot.Core.Context.TestContext;
using PeasyPilot.Core.Assertions;
using ITestContext = PeasyPilot.Core.Abstractions.ITestContext;
using TestResult = PeasyPilot.Core.Models.TestResult;
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
            PeasyAssert.Equal("value", value);
        });
    }
}
