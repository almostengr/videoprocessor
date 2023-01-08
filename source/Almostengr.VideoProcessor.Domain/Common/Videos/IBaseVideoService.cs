namespace Almostengr.VideoProcessor.Domain.Common.Videos;

public interface IBaseVideoService 
{
    Task<bool> ProcessVideoAsync(CancellationToken stoppingToken);
}