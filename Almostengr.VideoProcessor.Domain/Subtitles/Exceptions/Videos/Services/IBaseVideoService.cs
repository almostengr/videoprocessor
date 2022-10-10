namespace Almostengr.VideoProcessor.Application.Video;

public interface IBaseVideoService 
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}