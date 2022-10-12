using Almostengr.VideoProcessor.Domain.Subtitles.Services;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class HandyTechSubtitleWorker : BaseWorker
{
    private readonly IHandyTechSrtSubtitleService _subtitleService;

    public HandyTechSubtitleWorker(IHandyTechSrtSubtitleService subtitleService)
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