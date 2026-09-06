using Xunit;
using PeasyPilot.TestAssistant.Rules;
using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Tests.RulesTests;

public class StringValueRuleTests
{
    [Fact]
    public void CanHandle_WithStringType_ReturnsTrue()
    {
        var rule = new StringValueRule();
        Assert.True(rule.CanHandle(typeof(string)));
    }

    [Fact]
    public void GenerateCases_WithStringType_ReturnsMultipleCases()
    {
        var rule = new StringValueRule();
        var context = new ValueGenerationContext();
        var cases = rule.GenerateCases(typeof(string), context);

        Assert.NotEmpty(cases);
        Assert.True(cases.Count >= 2);
    }
}
