using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal abstract class BaseWorker : BackgroundService
{
    internal AppSettings _appSettings;
    internal readonly TimeSpan _delayLoopTime;

    protected BaseWorker(AppSettings appSettings)
    {
        _appSettings = appSettings;
        _delayLoopTime = new TimeSpan(0, 0, 5);
    }

    internal async Task<DateTime> DelayWhenLoopingQuickly(
        TimeSpan delayTime, CancellationToken cancellationToken, DateTime previousTime)
    {
        if ((previousTime - DateTime.Now) < delayTime)
        {
            await Task.Delay(_appSettings.LongWorkerDelay, cancellationToken);
        }

        return DateTime.Now;
    }
}