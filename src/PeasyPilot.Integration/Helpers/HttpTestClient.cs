using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PeasyPilot.Integration.Helpers;

/// <summary>
/// Helper client wrapper around <see cref="HttpClient"/> providing convenient JSON requests and status assertions.
/// </summary>
public class HttpTestClient
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Gets the underlying HttpClient instance.
    /// </summary>
    public HttpClient Client => _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpTestClient"/> class.
    /// </summary>
    /// <param name="client">The http client to wrap.</param>
    public HttpTestClient(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Sends a GET request and deserializes the JSON response.
    /// </summary>
    /// <typeparam name="T">The response model type.</typeparam>
    /// <param name="requestUri">The endpoint URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Deserialized response object.</returns>
    public async Task<T?> GetJsonAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    /// <summary>
    /// Sends a POST request with JSON payload and deserializes the response.
    /// </summary>
    /// <typeparam name="TRequest">The request body type.</typeparam>
    /// <typeparam name="TResponse">The response body type.</typeparam>
    /// <param name="requestUri">The endpoint URL.</param>
    /// <param name="payload">The payload object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Deserialized response object.</returns>
    public async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(requestUri, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    /// <summary>
    /// Asserts that a response has the expected HTTP status code.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="expectedCode">The expected status code.</param>
    public static void AssertStatusCode(HttpResponseMessage response, HttpStatusCode expectedCode)
    {
        if (response.StatusCode != expectedCode)
        {
            throw new InvalidOperationException($"Expected HTTP status code {expectedCode} ({(int)expectedCode}), but received {response.StatusCode} ({(int)response.StatusCode}).");
        }
    }
}
