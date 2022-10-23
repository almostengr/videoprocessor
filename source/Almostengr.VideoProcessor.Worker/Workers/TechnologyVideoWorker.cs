using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Videos.TechnologyVideo;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class TechnologyVideoWorker : BaseWorker
{
    private readonly ITechnologyVideoService _videoService;
    private readonly AppSettings _appSettings;

    public TechnologyVideoWorker(ITechnologyVideoService videoService,
        AppSettings appSettings) : base(appSettings)
    {
        _videoService = videoService;
        _appSettings = appSettings;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _videoService.ProcessVideosAsync(stoppingToken);
            await Task.Delay(_appSettings.WorkerDelay, stoppingToken);
        }
    }
}
