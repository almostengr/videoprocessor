using System.Collections.Generic;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IVideoRenderService : IBaseService
    {
        string[] GetVideosFromInputDirectory();
        bool IsDiskSpaceAvailable();
        Task RenderChannelVideosAsync(ChannelPropertiesDto channelProperties);
        void CleanWorkingDirectory();
        bool DoesChannelFileExist();
        string GetSubtitlesFilter();
        string GetDestinationFilter(string workingDirectory);
        string GetMajorRoadFilter(string workingDirectory);
        string GetChannelBrandingText(List<string> texts);
        Task ExtractTarFileToWorkingDirectoryAsync(string tarFile);
        Task RenderVideoToUploadDirectoryAsync();
        Task ArchiveWorkingDirectoryContentsAsync();
        string GetBrandingTextColor(string channelName, string videoTitle);
        Task ConvertVideoFilesToMp4Async();
        string GetMajorRoadFilter();
        string GetDestinationFilter();
    }
}