namespace Almostengr.VideoProcessor.Worker;

public class HandyTechSubtitleWorker : BackgroundService
{
    private readonly ILogger<HandyTechSubtitleWorker> _logger;

    public HandyTechSubtitleWorker(ILogger<HandyTechSubtitleWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

}