namespace Almostengr.VideoProcessor.Core.Common.Videos;

public interface IBaseVideoService
{
    Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken);
    Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken);
    Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken);
}

public interface IReviewVideoService : IBaseVideoService
{
    Task ProcessReviewedFilesAsync(CancellationToken cancellationToken);
}
