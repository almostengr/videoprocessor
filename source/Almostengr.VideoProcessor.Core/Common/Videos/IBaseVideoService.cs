namespace Almostengr.VideoProcessor.Core.Common.Videos;

public interface IBaseVideoService
{
    Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken);
    Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken);
    // Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken);
    Task ProcessVideoProjectAsync(CancellationToken cancellationToken);
}

public interface IReviewVideoService : IBaseVideoService
{
    Task ProcessReviewedFilesAsync(CancellationToken cancellationToken);
}

public interface ITranscriptionService : IReviewVideoService
{
    Task ProcessSrtSubtitlesAsync(CancellationToken cancellationToken);
}
