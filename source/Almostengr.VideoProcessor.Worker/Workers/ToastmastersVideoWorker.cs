using Almostengr.VideoProcessor.Domain.ToastmastersVideo;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class ToastmastersVideoWorker : BaseWorker
{
    private readonly IToastmastersVideoService _videoService;

    public ToastmastersVideoWorker(IToastmastersVideoService videoService)
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
