using System.Collections.Concurrent;
using System.Net;

namespace PeasyPilot.Integration.Helpers;

/// <summary>
/// In-memory mock HTTP handler for stubbing endpoints and recording outbound HTTP requests during integration tests.
/// </summary>
public sealed class MockHttpServer : HttpMessageHandler
{
    private readonly ConcurrentDictionary<string, (HttpStatusCode StatusCode, string Content)> _routes = new();
    private readonly ConcurrentBag<HttpRequestMessage> _recordedRequests = new();

    /// <summary>
    /// Stubs a GET/POST endpoint with a specific status code and content string.
    /// </summary>
    /// <param name="path">The request path (e.g., "/api/users").</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="content">The response body content.</param>
    public void StubEndpoint(string path, HttpStatusCode statusCode, string content)
    {
        _routes[path.ToLowerInvariant()] = (statusCode, content);
    }

    /// <summary>
    /// Gets the list of recorded HTTP requests sent through this handler.
    /// </summary>
    public IReadOnlyCollection<HttpRequestMessage> RecordedRequests => _recordedRequests.ToList();

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _recordedRequests.Add(request);

        var path = request.RequestUri?.AbsolutePath.ToLowerInvariant() ?? string.Empty;

        if (_routes.TryGetValue(path, out var stub))
        {
            var response = new HttpResponseMessage(stub.StatusCode)
            {
                Content = new StringContent(stub.Content)
            };
            return await Task.FromResult(response);
        }

        return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }

    /// <summary>
    /// Creates an HttpClient backed by this mock server handler.
    /// </summary>
    /// <returns>HttpClient instance.</returns>
    public HttpClient CreateClient()
    {
        return new HttpClient(this)
        {
            BaseAddress = new Uri("http://localhost-mock")
        };
    }
}
