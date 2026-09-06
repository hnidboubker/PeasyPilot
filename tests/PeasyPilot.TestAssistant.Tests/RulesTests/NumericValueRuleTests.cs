using Xunit;
using PeasyPilot.TestAssistant.Rules;
using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Tests.RulesTests;

public class NumericValueRuleTests
{
    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(long))]
    [InlineData(typeof(double))]
    [InlineData(typeof(decimal))]
    public void CanHandle_WithNumericType_ReturnsTrue(Type numericType)
    {
        var rule = new NumericValueRule();
        Assert.True(rule.CanHandle(numericType));
    }

    [Fact]
    public void GenerateCases_WithSignedInt_IncludesZeroAndNegative()
    {
        var rule = new NumericValueRule();
        var context = new ValueGenerationContext();
        var cases = rule.GenerateCases(typeof(int), context);

        Assert.Contains(cases, c => c.ValueExpression == "0");
        Assert.Contains(cases, c => c.ValueExpression == "-1");
    }

    [Fact]
    public void GenerateCases_WithUnsignedInt_OnlyIncludesZero()
    {
        var rule = new NumericValueRule();
        var context = new ValueGenerationContext();
        var cases = rule.GenerateCases(typeof(uint), context);

        Assert.Contains(cases, c => c.ValueExpression == "0");
        Assert.DoesNotContain(cases, c => c.ValueExpression == "-1");
    }
}
