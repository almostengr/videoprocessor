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
                await _videoService.ProcessIncomingVideoTarballsAsync(cancellationToken);
            }
            catch (NoFilesMatchException)
            {
                await _videoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
                await _videoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
                await Task.Delay(_appSettings.WorkerDelay, cancellationToken);
            }
        }
    }
}
