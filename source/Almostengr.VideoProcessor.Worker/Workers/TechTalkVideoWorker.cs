using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.TechTalk;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class TechTalkVideoWorker : BaseWorker
{
    private readonly ITechTalkVideoService _videoService;
    private readonly AppSettings _appSettings;

    public TechTalkVideoWorker(ITechTalkVideoService videoService,
        AppSettings appSettings) : base(appSettings)
    {
        _videoService = videoService;
        _appSettings = appSettings;
    }

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _videoService.CreateThumbnails();
                await _videoService.ProcessIncomingTarballFilesAsync(cancellationToken);
            }
            catch (NoFilesMatchException)
            {
                await _videoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
            }

            try
            {
                // await _videoService.ProcessReviewedFilesAsync(cancellationToken);
            }
            catch (NoFilesMatchException)
            {
                await _videoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
                await Task.Delay(_appSettings.WorkerDelay, cancellationToken);
            }

        }
    }
}
