using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HdhrTvGuide.HdHomeRun;

/// <summary>
/// Talks to a local HDHomeRun tuner's HTTP API.
/// </summary>
public class HdHomeRunClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HdHomeRunClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HdHomeRunClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{HdHomeRunClient}"/> interface.</param>
    public HdHomeRunClient(IHttpClientFactory httpClientFactory, ILogger<HdHomeRunClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _logger = logger;
    }

    /// <summary>
    /// Queries a tuner's local <c>/discover.json</c> endpoint for its DeviceAuth token.
    /// </summary>
    /// <param name="baseAddress">The tuner's base address, e.g. "192.168.1.50" or "http://192.168.1.50".</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    /// <returns>The tuner's DeviceAuth token, or <c>null</c> if it could not be retrieved.</returns>
    public async Task<HdHomeRunDiscoverResponse?> GetDeviceInfoAsync(string baseAddress, CancellationToken cancellationToken)
    {
        var normalized = NormalizeBaseAddress(baseAddress);
        var discoverUrl = $"{normalized}/discover.json";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<HdHomeRunDiscoverResponse>(discoverUrl, cancellationToken)
                .ConfigureAwait(false);

            if (response is null || string.IsNullOrWhiteSpace(response.DeviceAuth))
            {
                _logger.LogWarning("Tuner at {Url} did not return a DeviceAuth token", discoverUrl);
                return null;
            }

            return response;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            _logger.LogWarning(ex, "Could not query HDHomeRun tuner at {Url}", discoverUrl);
            return null;
        }
    }

    private static string NormalizeBaseAddress(string address)
    {
        var trimmed = address.Trim().TrimEnd('/');

        if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = "http://" + trimmed;
        }

        return trimmed;
    }
}
