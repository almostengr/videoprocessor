using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Technology;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class TechnologySubtitleWorker : BaseWorker
{
    private readonly ITechnologySubtitleService _service;
    private readonly AppSettings _appSettings;

    public TechnologySubtitleWorker(ITechnologySubtitleService service,
        AppSettings appSettings) : base(appSettings)
    {
        _service = service;
        _appSettings = appSettings;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _service.ExecuteAsync(stoppingToken);
            await Task.Delay(_appSettings.WorkerDelay, stoppingToken);
        }
    }
}