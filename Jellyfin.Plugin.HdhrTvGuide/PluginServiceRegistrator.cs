using Jellyfin.Plugin.HdhrTvGuide.Guide;
using Jellyfin.Plugin.HdhrTvGuide.HdHomeRun;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.HdhrTvGuide;

/// <summary>
/// Registers this plugin's services with Jellyfin's dependency injection container.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<HdHomeRunClient>();
        serviceCollection.AddSingleton<GuideSyncService>();
    }
}
