namespace PeasyPilot.Core.Tests.Core;

using static PeasyPilot.Core.TestRunner;
using Xunit;

public class PeasyPilotApiTests
{
    [Fact]
    public void Test_Method_ShouldRunWithoutFrameworkAttributes()
    {
        var result = Test("sum is correct", () =>
        {
            var value = 2 + 3;
            Assert.Equal(5, value);
        });

        Assert.Equal(PeasyPilot.Core.Eums.TestRunStatus.Passed, result.Status);
        Assert.Equal(1, result.Passed);
    }
}
