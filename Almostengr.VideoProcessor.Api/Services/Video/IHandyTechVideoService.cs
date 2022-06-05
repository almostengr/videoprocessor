using System.Threading;
using System.Threading.Tasks;

namespace Almostengr.VideoProcessor.Api.Services.Video
{
    public interface IHandyTechVideoService : IVideoService
    {
        Task AddAudioToTimelapseAsync(string workingDirectory, CancellationToken cancellationToken);
        Task ConvertVideoFilesToCommonFormatAsync(string directory, CancellationToken stoppingToken);
        Task ConvertVideoFilesToTsAsync(string workingDirectory, CancellationToken stoppingToken);
        void CopyShowIntroToWorkingDirectory(string introVideoPath, string workingDirectory);
    }
}