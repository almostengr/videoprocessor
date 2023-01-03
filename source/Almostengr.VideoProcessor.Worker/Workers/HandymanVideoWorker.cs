using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Videos.Handyman;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class HandymanVideoWorker : BaseWorker
{
    private readonly IHandymanVideoService _videoService;
    private readonly AppSettings _appSettings;

    public HandymanVideoWorker(IHandymanVideoService videoService,
        AppSettings appSettings) : base(appSettings)
    {
        _videoService = videoService;
        _appSettings = appSettings;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            bool doSleep = await _videoService.ProcessVideosAsync(stoppingToken);

            if (doSleep)
            {
                await Task.Delay(_appSettings.WorkerDelay, stoppingToken);
            }
        }
    }
}