namespace Almostengr.VideoProcessor.Core.Common.Videos;

public interface IBaseVideoService
{
    Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken);
    Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken);
    Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken);
    Task ProcessReviewedFilesAsync(CancellationToken cancellationToken);

    // Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken);
    // Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken);
    // Task ReceivedToStitchedAsync(CancellationToken cancellationToken);
    // Task ReviewingToAnimated(CancellationToken cancellationToken);
}