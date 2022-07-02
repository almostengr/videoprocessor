using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.VideoCommon
{
    public interface IVideoService : IBaseService
    {
        Task ExecuteServiceAsync(CancellationToken cancellationToken);
        Task StartAsync(CancellationToken cancellationToken);
    }
}