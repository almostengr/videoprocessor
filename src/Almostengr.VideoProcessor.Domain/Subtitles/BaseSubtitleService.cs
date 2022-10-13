namespace Almostengr.VideoProcessor.Domain.Subtitles;

public abstract class BaseSubtitleService : IBaseSubtitleService
{
    public abstract Task ExecuteAsync(CancellationToken stoppingToken);
}