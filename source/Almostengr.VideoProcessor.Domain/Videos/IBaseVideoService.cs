namespace Almostengr.VideoProcessor.Domain.Videos;

public interface IBaseVideoService 
{
    Task ProcessVideosAsync(CancellationToken stoppingToken);
}