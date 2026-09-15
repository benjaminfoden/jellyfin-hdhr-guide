using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.HdhrTvGuide.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        UseConfiguredTuners = true;
        ManualTunerAddresses = string.Empty;
        ListingsProviderId = string.Empty;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to include the HDHomeRun tuners already
    /// configured under Jellyfin's Live TV &gt; Tuner Devices settings.
    /// </summary>
    public bool UseConfiguredTuners { get; set; }

    /// <summary>
    /// Gets or sets a comma-separated list of additional HDHomeRun tuner addresses
    /// (IP addresses or hostnames) to query for guide data, beyond any already
    /// configured as Jellyfin tuner devices.
    /// </summary>
    public string ManualTunerAddresses { get; set; }

    /// <summary>
    /// Gets or sets the id of the XMLTV listings provider entry this plugin manages.
    /// Set automatically after the first successful sync so later runs update the
    /// existing entry instead of creating duplicates.
    /// </summary>
    public string ListingsProviderId { get; set; }
}
