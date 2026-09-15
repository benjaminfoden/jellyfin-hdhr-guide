using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.HdhrTvGuide.Configuration;
using Jellyfin.Plugin.HdhrTvGuide.ScheduledTasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.HdhrTvGuide;

/// <summary>
/// The main plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private readonly ITaskManager _taskManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="taskManager">Instance of the <see cref="ITaskManager"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ITaskManager taskManager)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _taskManager = taskManager;
    }

    /// <inheritdoc />
    public override string Name => "HDHomeRun TV Guide";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("6277ad20-237b-493a-acec-e134d424a266");

    /// <inheritdoc />
    public override string Description =>
        "Points Jellyfin's XMLTV listings provider at your HDHomeRun tuners' guide data.";

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            }
        ];
    }

    /// <inheritdoc />
    public override void UpdateConfiguration(BasePluginConfiguration configuration)
    {
        base.UpdateConfiguration(configuration);

        _taskManager.CancelIfRunningAndQueue<SyncGuideTask>();
    }
}
