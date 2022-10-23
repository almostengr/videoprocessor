using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal abstract class BaseWorker : BackgroundService
{
    public readonly TimeSpan WaitDelay;

    protected BaseWorker()
    {
        WaitDelay = TimeSpan.FromMinutes(1);
    }

    protected BaseWorker(AppSettings appSettings)
    {
    }
}