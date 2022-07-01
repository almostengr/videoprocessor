namespace Almostengr.VideoProcessor.Core.Services
{
    public interface IVideoService
    {
        Task ExecuteServiceAsync(CancellationToken cancellationToken);
        Task StartAsync(CancellationToken cancellationToken);
    }
}