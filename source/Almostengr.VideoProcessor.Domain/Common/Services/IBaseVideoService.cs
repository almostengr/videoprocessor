namespace Almostengr.VideoProcessor.Domain.Common.Services;

public interface IBaseVideoService 
{
    Task ProcessVideosAsync(CancellationToken stoppingToken);
}