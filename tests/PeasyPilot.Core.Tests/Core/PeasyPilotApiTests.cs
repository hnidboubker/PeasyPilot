namespace PeasyPilot.Core.Tests.Core;

using PeasyPilot.Core;
using static PeasyPilot.Core.PeasyPilot;
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
            Assert.NotNull(value);
        });

        Assert.Equal(TestRunStatus.Passed, result.Status);
        Assert.Equal(1, result.Passed);
    }
}
