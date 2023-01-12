namespace Almostengr.VideoProcessor.Core.Videos;

public interface IBaseVideoService
{
    Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken);
}