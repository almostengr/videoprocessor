namespace Almostengr.VideoProcessor.Domain.Subtitles;

public interface IBaseSubtitleService
{
    abstract Task ExecuteAsync(CancellationToken cancellationToken);
}