namespace Almostengr.VideoProcessor.Core.Services
{
    public interface IVideoService
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
        Task StartAsync(CancellationToken cancellationToken);
    }
}