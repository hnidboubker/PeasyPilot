using System.Dynamic;

namespace PeasyPilot.Mcp.Tools;

/// <summary>
/// MCP Tool: Diagnose test failures
/// Input: testName, errorMessage, stackTrace (optional)
/// Output: failureType, reason, suggestions, riskLevel
/// </summary>
public class AnalyzeFailureTool : IMcpTool
{
    public string Name => "analyze_failure";
    public string Description => "Diagnose test failures and suggest fixes";

    public async Task<dynamic> ExecuteAsync(Dictionary<string, object> parameters)
    {
        try
        {
            var testName = parameters.TryGetValue("testName", out var nameObj)
                ? nameObj?.ToString()
                : "Unknown";

            var errorMessage = parameters.TryGetValue("errorMessage", out var errObj)
                ? errObj?.ToString() ?? throw new ArgumentException("errorMessage is required")
                : throw new ArgumentException("errorMessage is required");

            var stackTrace = parameters.TryGetValue("stackTrace", out var stackObj)
                ? stackObj?.ToString()
                : null;

            var (failureType, reason, suggestions) = DiagnoseFailure(errorMessage ?? string.Empty, stackTrace);

            dynamic result = new ExpandoObject();
            result.success = true;
            result.testName = testName;
            result.failureType = failureType;
            result.reason = reason;
            result.suggestions = suggestions;
            result.riskLevel = DetermineRiskLevel(failureType);
            return result;
        }
        catch (Exception ex)
        {
            dynamic result = new ExpandoObject();
            result.success = false;
            result.error = ex.Message;
            return result;
        }
    }

    private (string failureType, string reason, List<string> suggestions) DiagnoseFailure(string errorMessage, string? stackTrace)
    {
        var lowerError = errorMessage.ToLowerInvariant();

        // Check specific patterns BEFORE general patterns
        if (lowerError.Contains("null") || lowerError.Contains("nullreferenceexception"))
        {
            return ("NULL_REFERENCE",
                "A null reference was encountered during test execution.",
                new List<string>
                {
                    "Check that all dependencies are properly mocked or initialized",
                    "Verify that objects are not null before assertions",
                    "Use SetUp or constructor injection to initialize test fixtures"
                });
        }

        // Timeout before general "expected"
        if (lowerError.Contains("timeout") || lowerError.Contains("timed out") || lowerError.Contains("deadlock"))
        {
            return ("TIMEOUT",
                "Test execution exceeded time limit or deadlock detected.",
                new List<string>
                {
                    "Increase test timeout if operation is legitimately slow",
                    "Check for infinite loops or blocking operations",
                    "Verify async/await usage is correct"
                });
        }

        // Mock patterns before general "assert"/"expected"
        if (lowerError.Contains("mock") || lowerError.Contains("not called") || lowerError.Contains("expectation"))
        {
            return ("MOCK_EXPECTATION",
                "Mock was not called as expected or setup was incorrect.",
                new List<string>
                {
                    "Verify mock expectations match actual method calls",
                    "Check that mocked dependencies are properly configured",
                    "Ensure mock.Verify() assertions are correct"
                });
        }

        // Setup errors before general assertions
        if (lowerError.Contains("setup") || lowerError.Contains("initialize"))
        {
            return ("SETUP_ERROR",
                "Test setup or initialization failed.",
                new List<string>
                {
                    "Check SetUp/TearDown methods for errors",
                    "Verify test data is correctly initialized",
                    "Ensure dependencies are available before test execution"
                });
        }

        // General assertion patterns last
        if (lowerError.Contains("assert") || lowerError.Contains("expected"))
        {
            return ("ASSERTION_FAILED",
                "Test assertion did not match expected value.",
                new List<string>
                {
                    "Verify the actual value matches the expected condition",
                    "Check that the method under test produces the correct output",
                    "Add logging to understand what value was actually produced"
                });
        }

        return ("UNKNOWN",
            "Unable to classify the specific failure type. Check stack trace for more details.",
            new List<string>
            {
                "Review the complete error message and stack trace",
                "Check test logs for context",
                "Verify test environment is correctly configured"
            });
    }

    private string DetermineRiskLevel(string failureType) => failureType switch
    {
        "NULL_REFERENCE" => "CRITICAL",
        "TIMEOUT" => "HIGH",
        "ASSERTION_FAILED" => "MEDIUM",
        "MOCK_EXPECTATION" => "MEDIUM",
        "SETUP_ERROR" => "HIGH",
        _ => "LOW"
    };
}
