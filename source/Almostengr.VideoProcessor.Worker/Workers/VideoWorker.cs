using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Videos;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class VideoWorker : BaseWorker
{
    private readonly IProcessVideoService _processVideoService;

    public VideoWorker(AppSettings appSettings, IProcessVideoService processVideoService) : base(appSettings)
    {
        _processVideoService = processVideoService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DateTime previousTime = DateTime.Now;

        while (!cancellationToken.IsCancellationRequested)
        {
            await _processVideoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
            bool processedVideo = await _processVideoService.RenderVideoAsync(cancellationToken);

            if (!processedVideo)
            {
                previousTime = await DelayWhenLoopingQuickly(_delayLoopTime, cancellationToken, previousTime);
            }
        }
    }
}