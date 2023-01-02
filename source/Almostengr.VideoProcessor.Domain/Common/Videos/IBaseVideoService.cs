namespace Almostengr.VideoProcessor.Domain.Common.Videos;

public interface IBaseVideoService 
{
    Task ProcessVideosAsync(CancellationToken stoppingToken);
}