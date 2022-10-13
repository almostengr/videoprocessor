namespace Almostengr.VideoProcessor.Domain.Common.Videos;

public interface IBaseVideoService 
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}