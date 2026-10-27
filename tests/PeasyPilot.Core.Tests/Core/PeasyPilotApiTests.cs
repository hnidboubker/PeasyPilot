namespace PeasyPilot.Core.Tests.Core;

using static PeasyPilot.Core.TestRunner;
using PeasyPilot.Core.Assertions;
using Xunit;

public class PeasyPilotApiTests
{
    [Fact]
    public void Test_Method_ShouldRunWithoutFrameworkAttributes()
    {
        var result = Test("sum is correct", () =>
        {
            var value = 2 + 3;
            PeasyAssert.Equal(5, value);
        });

        PeasyAssert.Equal(PeasyPilot.Core.Eums.TestRunStatus.Passed, result.Status);
        PeasyAssert.Equal(1, result.Passed);
    }
}
