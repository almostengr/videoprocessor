using Almostengr.VideoProcessor.Domain.Common.Videos;

namespace Almostengr.VideoProcessor.Domain.DashCam;

public interface IDashCamVideoService : IBaseVideoService
{
    Task CompressTarballsInArchiveFolderAsync(CancellationToken stoppingToken);
}