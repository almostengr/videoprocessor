namespace Almostengr.VideoProcessor.Domain.Common.Services;

public interface IBaseSubtitleService
{
    abstract Task ExecuteAsync(CancellationToken cancellationToken);
}