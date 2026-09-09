namespace PeasyPilot.Mcp.Models;

public class TestExecutionResult
{
    public int Total { get; init; }
    public int Passed { get; init; }
    public int Failed { get; init; }
    public List<TestFailure> Failures { get; init; } = new();
}

public class TestFailure
{
    public required string TestName { get; init; }
    public required string Message { get; init; }
    public string? StackTrace { get; init; }
}
