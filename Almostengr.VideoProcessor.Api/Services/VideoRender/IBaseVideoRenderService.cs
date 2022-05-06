using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IBaseVideoRenderService : IBaseService
    {
        abstract Task RenderVideoAsync(VideoPropertiesDto videoProperties);
        abstract string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties);
        string[] GetVideoArchivesInDirectory(string directory);
        void CleanDirectory(string directory);
        string GetSubtitlesFilter(string workingDirectory);
        Task ExtractTarFileToWorkingDirectoryAsync(string tarFile, string workingDirectory);
        Task ArchiveWorkingDirectoryContentsAsync(string workingDirectory, string archiveDirectory);
        Task ConvertVideoFilesToMp4Async(string directory);
        void LowerCaseFileNamesInDirectory(string directory);
        void CheckOrCreateFfmpegInputFile(string workingDirectory);
        void SaveVideoMetaData(VideoPropertiesDto videoProperties);
        void MoveProcessedVideoArchiveToArchive(string workingDirectory, string archiveDirectory);
        void CreateThumbnailsFromFinalVideo(VideoPropertiesDto videoProperties);
    }
}
