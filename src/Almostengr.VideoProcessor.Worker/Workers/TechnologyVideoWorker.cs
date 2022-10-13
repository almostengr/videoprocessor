using Almostengr.VideoProcessor.Domain.TechnologyVideo;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class TechnologyVideoWorker : BaseWorker
{
    private readonly ITechnologyVideoService _videoService;

    public TechnologyVideoWorker(ITechnologyVideoService videoService)
    {
        _videoService = videoService;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _videoService.ExecuteAsync(stoppingToken);
            await Task.Delay(WaitDelay, stoppingToken);
        }
    }
}
