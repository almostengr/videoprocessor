using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal abstract class BaseWorker : BackgroundService
{
    protected BaseWorker(AppSettings appSettings)
    {
    }
}