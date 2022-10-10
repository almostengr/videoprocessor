namespace Almostengr.VideoProcessor.Domain.Videos.Services;

public interface IBaseVideoService 
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}