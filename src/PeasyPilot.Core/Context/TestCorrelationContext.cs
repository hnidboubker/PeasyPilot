namespace PeasyPilot.Core.Context;

/// <summary>
/// Provides correlation ID and distributed tracing context for cross-service integration tests.
/// </summary>
public static class TestCorrelationContext
{
    public const string CorrelationHeaderName = "X-Correlation-ID";

    /// <summary>
    /// Generates a new unique correlation ID for a test execution.
    /// </summary>
    /// <returns>Unique correlation ID string.</returns>
    public static string CreateCorrelationId()
    {
        return $"test-trace-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Injects the correlation header into an HttpClient request headers collection.
    /// </summary>
    /// <param name="client">The http client instance.</param>
    /// <param name="correlationId">The correlation ID to inject.</param>
    public static void InjectHeader(HttpClient client, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(client);
        if (!client.DefaultRequestHeaders.Contains(CorrelationHeaderName))
        {
            client.DefaultRequestHeaders.Add(CorrelationHeaderName, correlationId);
        }
    }
}
