namespace Almostengr.VideoProcessor.Domain.Subtitles.Services;

public abstract class BaseSubtitleService : ISubtitleService
{
    public abstract Task ExecuteAsync(CancellationToken stoppingToken);
}