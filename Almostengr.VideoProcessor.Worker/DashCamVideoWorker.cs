using Almostengr.VideoProcessor.Application.Video;

namespace Almostengr.VideoProcessor.Worker;
public class DashCamVideoWorker : BackgroundService
{
    private readonly IDashCamVideoService _videoService;
    private readonly ILogger<DashCamVideoWorker> _logger;

    public DashCamVideoWorker(IDashCamVideoService dashCamVideoService, ILogger<DashCamVideoWorker> logger)
    {
        _videoService = dashCamVideoService;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _videoService.ExecuteAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
        }
    }
}
