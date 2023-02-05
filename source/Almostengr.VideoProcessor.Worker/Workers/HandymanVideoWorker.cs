using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Handyman;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

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

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // await _videoService.ProcessIncomingVideoTarballsAsync(cancellationToken);
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