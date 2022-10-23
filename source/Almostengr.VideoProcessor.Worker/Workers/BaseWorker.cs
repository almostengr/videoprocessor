using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal abstract class BaseWorker : BackgroundService
{
    protected BaseWorker(AppSettings appSettings)
    {
    }
}