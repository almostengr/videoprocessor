namespace Almostengr.VideoProcessor.Domain.Common.Subtitles;
public interface IBaseSubtitleService
{
    abstract Task<bool> ExecuteAsync(CancellationToken cancellationToken);
}