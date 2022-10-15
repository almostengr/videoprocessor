using Almostengr.VideoProcessor.Domain.Videos.ChristmasLightShowVideo;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class ChristmasLightVideoWorker : BaseWorker
{
    private readonly IChristmasLightVideoService _videoService;

    public ChristmasLightVideoWorker(IChristmasLightVideoService videoService)
    {
        _videoService = videoService;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _videoService.ProcessVideosAsync(stoppingToken);
            await Task.Delay(WaitDelay, stoppingToken);
        }
    }
}
