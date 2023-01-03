namespace Almostengr.VideoProcessor.Domain.Common.Videos;

public interface IBaseVideoService 
{
    Task<bool> ProcessVideosAsync(CancellationToken stoppingToken);
}