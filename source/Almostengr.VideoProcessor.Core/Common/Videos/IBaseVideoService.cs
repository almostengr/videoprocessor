namespace Almostengr.VideoProcessor.Core.Common.Videos;

public interface IBaseVideoService
{
    Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken);
    Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken);
    Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken);
}