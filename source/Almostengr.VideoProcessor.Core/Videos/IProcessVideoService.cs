namespace Almostengr.VideoProcessor.Core.Videos;

public interface IProcessVideoService
{
    Task CompressTarballsInArchiveFolderAsync(string archiveDirectory, CancellationToken cancellationToken);
    Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken);
    Task<bool> RenderVideoAsync(CancellationToken cancellationToken);
}