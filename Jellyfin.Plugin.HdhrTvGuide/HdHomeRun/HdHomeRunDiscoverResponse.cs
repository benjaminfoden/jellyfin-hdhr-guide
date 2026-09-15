using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.HdhrTvGuide.HdHomeRun;

/// <summary>
/// The subset of fields Jellyfin needs from a tuner's local
/// <c>/discover.json</c> endpoint.
/// </summary>
public class HdHomeRunDiscoverResponse
{
    /// <summary>
    /// Gets or sets the tuner's friendly name, e.g. "HDHomeRun FLEX 4K".
    /// </summary>
    [JsonPropertyName("FriendlyName")]
    public string? FriendlyName { get; set; }

    /// <summary>
    /// Gets or sets the tuner's device id.
    /// </summary>
    [JsonPropertyName("DeviceID")]
    public string? DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the token used to authenticate this device against
    /// SiliconDust's cloud APIs, including the guide feed.
    /// </summary>
    [JsonPropertyName("DeviceAuth")]
    public string? DeviceAuth { get; set; }
}
