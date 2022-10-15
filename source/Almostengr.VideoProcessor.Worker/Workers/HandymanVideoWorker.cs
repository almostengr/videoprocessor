using Almostengr.VideoProcessor.Domain.Videos.HandymanVideo;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class HandymanVideoWorker : BaseWorker
{
    private readonly IHandymanVideoService _videoService;

    public HandymanVideoWorker(IHandymanVideoService videoService)
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