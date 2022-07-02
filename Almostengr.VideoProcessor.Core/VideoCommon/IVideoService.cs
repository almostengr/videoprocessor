using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.VideoCommon
{
    public interface IVideoService
    {
        Task ExecuteServiceAsync(CancellationToken cancellationToken);
        Task StartAsync(CancellationToken cancellationToken);
    }
}