using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services.Video
{
    public interface IVideoService
    {
        abstract Task RenderVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken);
        abstract string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties);
        string[] GetVideoArchivesInDirectory(string directory);
        string GetSubtitlesFilter(string workingDirectory);
        Task ExtractTarFileAsync(string tarFile, string workingDirectory, CancellationToken cancellationToken);
        Task ArchiveDirectoryContentsAsync(string directoryToArchive, string archiveName, string archiveDestination, CancellationToken cancellationToken);
        Task ConvertVideoFilesToMp4Async(string directory, CancellationToken cancellationToken);
        void PrepareFileNamesInDirectory(string directory);
        void CheckOrCreateFfmpegInputFile(string workingDirectory);
        abstract Task CreateThumbnailsFromFinalVideoAsync(VideoPropertiesDto videoProperties, CancellationToken cancellationToken);
        Task CleanUpBeforeArchivingAsync(string workingDirectory);
        Task WorkerIdleAsync(CancellationToken cancellationToken);
    }
}