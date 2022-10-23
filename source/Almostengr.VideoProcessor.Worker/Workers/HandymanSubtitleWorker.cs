using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class HandymanSubtitleWorker : BaseWorker
{
    private readonly IHandymanSubtitleService _subtitleService;
    private readonly AppSettings _appSettings;

    public HandymanSubtitleWorker(IHandymanSubtitleService subtitleService,
        AppSettings appSettings) : base(appSettings)
    {
        _subtitleService = subtitleService;
        _appSettings = appSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _subtitleService.ExecuteAsync(stoppingToken);
            await Task.Delay(_appSettings.WorkerDelayMinutes, stoppingToken);
        }
    }

}