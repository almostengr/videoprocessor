namespace Almostengr.VideoProcessor.Domain.Common.Subtitles;
public interface IBaseSubtitleService
{
    abstract Task ExecuteAsync(CancellationToken cancellationToken);
}