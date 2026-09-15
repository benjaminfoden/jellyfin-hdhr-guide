using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.HdhrTvGuide.Guide;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.HdhrTvGuide.ScheduledTasks;

/// <summary>
/// Scheduled task that refreshes DeviceAuth tokens from configured HDHomeRun tuners
/// and updates the XMLTV listings provider with SiliconDust's guide feed.
/// </summary>
public class SyncGuideTask : IScheduledTask
{
    // SiliconDust asks that the guide feed not be polled at a fixed time each day;
    // a randomized 20-28 hour default interval spreads out load on their service.
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromHours(20 + (Random.Shared.NextDouble() * 8));

    private readonly GuideSyncService _guideSyncService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncGuideTask"/> class.
    /// </summary>
    /// <param name="guideSyncService">Instance of the <see cref="GuideSyncService"/> class.</param>
    public SyncGuideTask(GuideSyncService guideSyncService)
    {
        _guideSyncService = guideSyncService;
    }

    /// <inheritdoc />
    public string Name => "Sync HDHomeRun TV Guide";

    /// <inheritdoc />
    public string Key => "HdhrTvGuideSync";

    /// <inheritdoc />
    public string Description => "Reads DeviceAuth tokens from your HDHomeRun tuners and updates the XMLTV listings provider with SiliconDust's guide feed.";

    /// <inheritdoc />
    public string Category => "Live TV";

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        await _guideSyncService.SyncAsync(cancellationToken).ConfigureAwait(false);
        progress.Report(100);
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return new[]
        {
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfoType.IntervalTrigger,
                IntervalTicks = DefaultInterval.Ticks
            }
        };
    }
}
