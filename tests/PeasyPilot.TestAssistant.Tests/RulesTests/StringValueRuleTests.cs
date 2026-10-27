using Xunit;
using PeasyPilot.Core.Assertions;
using PeasyPilot.TestAssistant.Rules;
using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Tests.RulesTests;

public class StringValueRuleTests
{
    [Fact]
    public void CanHandle_WithStringType_ReturnsTrue()
    {
        var rule = new StringValueRule();
        PeasyAssert.True(rule.CanHandle(typeof(string)));
    }

    [Fact]
    public void GenerateCases_WithStringType_ReturnsMultipleCases()
    {
        var rule = new StringValueRule();
        var context = new ValueGenerationContext();
        var cases = rule.GenerateCases(typeof(string), context);

        PeasyAssert.NotEmpty(cases);
        PeasyAssert.True(cases.Count >= 2);
    }
}
