using Almostengr.VideoProcessor.Application.Video;

namespace Almostengr.VideoProcessor.Worker;

public class HandyTechVideoWorker : BackgroundService
{
    private readonly IHandyTechVideoService _videoService;
    private readonly ILogger<HandyTechVideoWorker> _logger;

    public HandyTechVideoWorker(IHandyTechVideoService videoService, ILogger<HandyTechVideoWorker> logger)
    {
        _videoService = videoService;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _videoService.ExecuteAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
