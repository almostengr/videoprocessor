using Almostengr.VideoProcessor.Domain.Videos.DashCamVideo;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class DashCamVideoWorker : BaseWorker
{
    private readonly IDashCamVideoService _videoService;

    public DashCamVideoWorker(IDashCamVideoService videoService)
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
