using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.DashCam;

public interface IDashCamVideoService : IBaseVideoService
{
    Task ProcessIncomingVideosWithGraphicsAsync(CancellationToken cancellationToken);
}