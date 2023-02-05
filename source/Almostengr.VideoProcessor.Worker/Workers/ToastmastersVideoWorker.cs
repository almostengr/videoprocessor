using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Toastmasters;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class ToastmastersVideoWorker : BaseWorker
{
    private readonly IToastmastersVideoService _videoService;
    private readonly AppSettings _appSettings;

    public ToastmastersVideoWorker(IToastmastersVideoService videoService,
        AppSettings appSettings) : base(appSettings)
    {
        _videoService = videoService;
        _appSettings = appSettings;
    }

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // try
            // {
            //     await _videoService.ProcessIncomingVideoTarballsAsync(cancellationToken);
            // }
            // catch (NoFilesMatchException)
            // {
            //     await _videoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
            //     await _videoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
            //     await Task.Delay(_appSettings.WorkerDelay, cancellationToken);
            // }


            try
            {
                // await _videoService.ProcessIncomingVideoTarballsAsync(cancellationToken);
                await _videoService.ProcessReviewedFilesAsync(cancellationToken);
            }
            catch (NoFilesMatchException)
            {
                await _videoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
                await _videoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
                // await _videoService.ConvertGzToXzAsync(cancellationToken);
                await Task.Delay(_appSettings.WorkerDelay, cancellationToken);
            }

            try
            {
                await _videoService.ProcessIncomingTarballFilesAsync(cancellationToken);
            }
            catch (NoFilesMatchException)
            {

            }
            
        }
    }
}
