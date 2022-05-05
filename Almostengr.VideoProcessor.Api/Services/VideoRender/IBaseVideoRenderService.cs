using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IBaseVideoRenderService : IBaseService
    {
        string[] GetVideoArchivesInDirectory(string directory);
        void CleanDirectory(string directory);
        string GetSubtitlesFilter(string workingDirectory);
        Task ExtractTarFileToWorkingDirectoryAsync(string tarFile, string workingDirectory);
        Task RenderVideoToUploadDirectoryAsync(string workingDirectory, string uploadDirectory);
        Task ArchiveWorkingDirectoryContentsAsync(string workingDirectory, string archiveDirectory);
        Task ConvertVideoFilesToMp4Async(string directory);
        void LowerCaseFileNamesInDirectory(string directory);
        abstract string GetFfmpegVideoFilters(VideoPropertiesDto videoProperties);
        void CheckOrCreateFfmpegInputFile(string workingDirectory);
        void SaveVideoMetaData(VideoPropertiesDto videoProperties);
        void MoveProcessedVideoArchiveToArchive(string workingDirectory, string archiveDirectory);
        void CreateThumbnailsFromFinalVideo(VideoPropertiesDto videoProperties);
    }
}
