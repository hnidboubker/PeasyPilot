namespace PeasyPilot.XUnit.Samples.Tests;

using Microsoft.Extensions.Logging;
using Xunit;
using PeasyPilot.XUnit;

/// <summary>
/// Example tests demonstrating the advanced logging system with error tracking.
/// Shows how to use TestLoggerBase for debugging and error detection.
/// </summary>
public class LoggingExampleTests : PeasyPilotTestBase
{
    [Fact]
    public void Example_SuccessfulTest_LogsCorrectly()
    {
        // Arrange
        Logger.LogInformation("Starting calculation test");
        var value1 = 5;
        var value2 = 3;

        // Act
        Logger.LogInformation($"Adding {value1} + {value2}");
        var result = value1 + value2;
        Logger.LogInformation($"Result: {result}");

        // Assert
        Assert.Equal(8, result);
        Logger.LogInformation("✅ Assertion passed");
    }

    [Fact]
    public void Example_TestWithAssertion_LogsFailure()
    {
        // Arrange
        Logger.LogInformation("Testing assertion logging");
        var expected = 10;
        var actual = 5;

        // Act & Assert
        if (actual != expected)
        {
            LogAssertionFailure("Values do not match", expected, actual);
        }

        Assert.NotEmpty(Errors);
        Logger.LogInformation($"Successfully logged {Errors.Count} error(s)");
    }

    [Fact]
    public void Example_TestWithException_CatchesAndLogs()
    {
        // Arrange
        Logger.LogInformation("Testing exception handling");

        // Act & Assert
        try
        {
            Logger.LogInformation("About to throw exception");
            throw new InvalidOperationException("This is a test exception");
        }
        catch (Exception ex)
        {
            LogException(ex, "Caught and logged exception");
        }

        Assert.NotEmpty(Errors);
        Logger.LogInformation($"Successfully captured exception: {Errors[0].ErrorType}");
    }

    [Fact]
    public void Example_ComplexTest_WithMultipleSteps()
    {
        // Arrange
        Logger.LogInformation("Starting complex multi-step test");
        var step = 1;

        // Step 1
        Logger.LogInformation($"Step {step}: Initialize data");
        step++;
        var data = new { value = 42, name = "test" };

        // Step 2
        Logger.LogInformation($"Step {step}: Process data");
        step++;
        Logger.LogInformation($"  - Value: {data.value}");
        Logger.LogInformation($"  - Name: {data.name}");

        // Step 3
        Logger.LogInformation($"Step {step}: Validate results");
        if (data.value == 42)
        {
            Logger.LogInformation($"  ✅ Value validation passed");
        }
        else
        {
            LogAssertionFailure("Value check failed", 42, data.value);
        }

        // Final assertion
        Assert.Equal(42, data.value);
        Logger.LogInformation("✅ All steps completed successfully");
    }

    [Fact]
    public void Example_WarningLogging_TracksIssues()
    {
        // Arrange
        Logger.LogInformation("Testing warning system");

        // Act
        var value = 100;
        if (value > 50)
        {
            LogWarning("Value exceeds threshold of 50");
        }

        Logger.LogInformation("Warning logged successfully");

        // Assert
        Assert.True(value > 50);
    }
}
