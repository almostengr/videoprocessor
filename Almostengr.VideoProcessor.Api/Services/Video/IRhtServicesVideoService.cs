using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services.Video
{
    public interface IRhtServicesVideoService : IVideoService
    {
        Task AddAudioToTimelapseAsync(string workingDirectory, CancellationToken cancellationToken);
        Task ConvertVideoFilesToTsAsync(string workingDirectory, CancellationToken stoppingToken);
    }
}