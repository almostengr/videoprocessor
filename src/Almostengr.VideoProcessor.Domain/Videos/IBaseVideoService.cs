namespace Almostengr.VideoProcessor.Domain.Videos;

public interface IBaseVideoService 
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}