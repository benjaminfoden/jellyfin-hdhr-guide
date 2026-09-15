using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.HdhrTvGuide.Configuration;
using Jellyfin.Plugin.HdhrTvGuide.HdHomeRun;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HdhrTvGuide.Guide;

/// <summary>
/// Discovers configured HDHomeRun tuners and points Jellyfin's XMLTV listings
/// provider at SiliconDust's cloud guide feed for those devices.
/// </summary>
public class GuideSyncService
{
    private const string GuideFeedBaseUrl = "https://api.hdhomerun.com/api/xmltv";

    private readonly IServerConfigurationManager _config;
    private readonly IListingsManager _listingsManager;
    private readonly HdHomeRunClient _hdHomeRunClient;
    private readonly ILogger<GuideSyncService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuideSyncService"/> class.
    /// </summary>
    /// <param name="config">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
    /// <param name="listingsManager">Instance of the <see cref="IListingsManager"/> interface.</param>
    /// <param name="hdHomeRunClient">Instance of the <see cref="HdHomeRunClient"/> class.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{GuideSyncService}"/> interface.</param>
    public GuideSyncService(
        IServerConfigurationManager config,
        IListingsManager listingsManager,
        HdHomeRunClient hdHomeRunClient,
        ILogger<GuideSyncService> logger)
    {
        _config = config;
        _listingsManager = listingsManager;
        _hdHomeRunClient = hdHomeRunClient;
        _logger = logger;
    }

    /// <summary>
    /// Queries every configured HDHomeRun tuner for its DeviceAuth token and updates
    /// the XMLTV listings provider this plugin manages with SiliconDust's guide feed URL.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
    public async Task SyncAsync(CancellationToken cancellationToken)
    {
        var plugin = Plugin.Instance ?? throw new InvalidOperationException("Plugin instance is not available.");
        var pluginConfig = plugin.Configuration;

        var tunerAddresses = GetTunerAddresses(pluginConfig);
        if (tunerAddresses.Count == 0)
        {
            _logger.LogWarning("No HDHomeRun tuners are configured; skipping guide sync");
            return;
        }

        var deviceAuthTokens = new List<string>();

        foreach (var address in tunerAddresses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deviceInfo = await _hdHomeRunClient.GetDeviceInfoAsync(address, cancellationToken).ConfigureAwait(false);
            if (deviceInfo?.DeviceAuth is { Length: > 0 } deviceAuth)
            {
                deviceAuthTokens.Add(deviceAuth);
            }
        }

        if (deviceAuthTokens.Count == 0)
        {
            _logger.LogWarning("Could not retrieve a DeviceAuth token from any configured HDHomeRun tuner; leaving the existing guide feed in place");
            return;
        }

        var listingsInfo = new ListingsProviderInfo
        {
            Id = pluginConfig.ListingsProviderId,
            Type = "xmltv",
            Path = BuildGuideUrl(deviceAuthTokens)
        };

        var savedInfo = await _listingsManager.SaveListingProvider(listingsInfo, validateLogin: false, validateListings: false)
            .ConfigureAwait(false);

        if (!string.Equals(savedInfo.Id, pluginConfig.ListingsProviderId, StringComparison.Ordinal))
        {
            pluginConfig.ListingsProviderId = savedInfo.Id;
            plugin.SaveConfiguration();
        }

        _logger.LogInformation("Updated the XMLTV listings provider with guide data for {Count} HDHomeRun tuner(s)", deviceAuthTokens.Count);
    }

    private List<string> GetTunerAddresses(PluginConfiguration pluginConfig)
    {
        var addresses = new List<string>();
        var seenHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddAddress(string? candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return;
            }

            var dedupeKey = TryGetHost(candidate, out var host) ? host : candidate.Trim();
            if (seenHosts.Add(dedupeKey))
            {
                addresses.Add(candidate.Trim());
            }
        }

        if (pluginConfig.UseConfiguredTuners)
        {
            var liveTvOptions = _config.GetConfiguration<LiveTvOptions>("livetv");
            foreach (var tunerHost in liveTvOptions.TunerHosts)
            {
                if (string.Equals(tunerHost.Type, "hdhomerun", StringComparison.OrdinalIgnoreCase))
                {
                    AddAddress(tunerHost.Url);
                }
            }
        }

        foreach (var manual in pluginConfig.ManualTunerAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            AddAddress(manual);
        }

        return addresses;
    }

    private static bool TryGetHost(string? address, out string host)
    {
        host = string.Empty;
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        var candidate = address.Contains("://", StringComparison.Ordinal) ? address : $"http://{address}";
        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.Host))
        {
            host = uri.Host;
            return true;
        }

        return false;
    }

    private static string BuildGuideUrl(IReadOnlyCollection<string> deviceAuthTokens)
    {
        var concatenated = string.Concat(deviceAuthTokens);
        return $"{GuideFeedBaseUrl}?DeviceAuth={Uri.EscapeDataString(concatenated)}";
    }
}
