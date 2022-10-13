namespace Almostengr.VideoProcessor.Domain.Subtitles;
public interface ISubtitleService
{
    abstract Task ExecuteAsync(CancellationToken cancellationToken);
}