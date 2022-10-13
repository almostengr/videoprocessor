using Almostengr.VideoProcessor.Domain.HandymanSubtitle;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class HandymanSubtitleWorker : BaseWorker
{
    private readonly IHandymanSubtitleService _subtitleService;

    public HandymanSubtitleWorker(IHandymanSubtitleService subtitleService)
    {
        _subtitleService = subtitleService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _subtitleService.ExecuteAsync(stoppingToken);
            await Task.Delay(WaitDelay, stoppingToken);
        }
    }

}