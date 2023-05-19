using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.DashCam;
using Almostengr.VideoProcessor.Core.Handyman;
using Almostengr.VideoProcessor.Core.TechTalk;
using Almostengr.VideoProcessor.Core.Toastmasters;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class VideoWorker : BaseWorker
{
    private readonly IDashCamVideoService _dashCamVideoService;
    private readonly IHandymanVideoService _handymanVideoService;
    private readonly ITechTalkVideoService _techTalkVideoService;
    private readonly IToastmastersVideoService _toastmastersVideoService;

    public VideoWorker(IDashCamVideoService dashCamVideoService,
        IHandymanVideoService handymanVideoService, ITechTalkVideoService techTalkVideoService,
        IToastmastersVideoService toastmastersVideoService, AppSettings appSettings) : base(appSettings)
    {
        _dashCamVideoService = dashCamVideoService;
        _handymanVideoService = handymanVideoService;
        _techTalkVideoService = techTalkVideoService;
        _toastmastersVideoService = toastmastersVideoService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DateTime previousTime = DateTime.Now;

        while (!cancellationToken.IsCancellationRequested)
        {
            await _handymanVideoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
            await _handymanVideoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
            await _handymanVideoService.ProcessVideoProjectAsync(cancellationToken);

            await _techTalkVideoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
            await _techTalkVideoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
            await _techTalkVideoService.ProcessVideoProjectAsync(cancellationToken);

            await _toastmastersVideoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
            await _toastmastersVideoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
            await _toastmastersVideoService.ProcessVideoProjectAsync(cancellationToken);

            await _dashCamVideoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
            await _dashCamVideoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
            await _dashCamVideoService.ProcessReviewedFilesAsync(cancellationToken);
            await _dashCamVideoService.ProcessVideoProjectAsync(cancellationToken);

            previousTime = await DelayWhenLoopingQuickly(_delayLoopTime, cancellationToken, previousTime);
        }
    }
}