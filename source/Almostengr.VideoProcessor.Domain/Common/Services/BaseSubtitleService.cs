namespace Almostengr.VideoProcessor.Domain.Common.Services;

public abstract class BaseSubtitleService : IBaseSubtitleService
{
    public abstract Task ExecuteAsync(CancellationToken stoppingToken);
}