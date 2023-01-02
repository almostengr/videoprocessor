namespace Almostengr.VideoProcessor.Domain.Common.Subtitles;

public abstract class BaseSubtitleService : IBaseSubtitleService
{
    public abstract Task ExecuteAsync(CancellationToken stoppingToken);
}