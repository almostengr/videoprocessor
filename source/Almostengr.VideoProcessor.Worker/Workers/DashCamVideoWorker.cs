using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.DashCam;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class DashCamVideoWorker : BaseWorker
{
    private readonly IDashCamVideoService _videoService;
    private readonly AppSettings _appSettings;

    public DashCamVideoWorker(IDashCamVideoService videoService,
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
            catch (NoTarballsPresentException)
            {
                await _videoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
                await Task.Delay(_appSettings.WorkerDelay, cancellationToken);
            }
        }
    }
}
