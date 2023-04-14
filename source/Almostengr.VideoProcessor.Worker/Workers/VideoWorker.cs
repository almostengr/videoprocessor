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
    private readonly AppSettings _appSettings;
    private readonly TimeSpan _delayLoopTime;

    public VideoWorker(IDashCamVideoService dashCamVideoService,
        IHandymanVideoService handymanVideoService, ITechTalkVideoService techTalkVideoService,
        IToastmastersVideoService toastmastersVideoService, AppSettings appSettings) : base(appSettings)
    {
        _dashCamVideoService = dashCamVideoService;
        _handymanVideoService = handymanVideoService;
        _techTalkVideoService = techTalkVideoService;
        _toastmastersVideoService = toastmastersVideoService;
        _appSettings = appSettings;

        _delayLoopTime = new TimeSpan(0, 0, 5);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DateTime previousTime = DateTime.Now;

        while (cancellationToken.IsCancellationRequested)
        {
            await _dashCamVideoService.ProcessIncomingVideosWithGraphicsAsync(cancellationToken);
            await _dashCamVideoService.ProcessIncomingTarballFilesAsync(cancellationToken);
            await _dashCamVideoService.CreateTarballsFromDirectoriesAsync(cancellationToken);

            // await _handymanVideoService.ProcessIncomingTarballFilesAsync(cancellationToken);
            // await _handymanVideoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
            await _handymanVideoService.CompressTarballsInArchiveFolderAsync(cancellationToken);

            // await _techtalkVideoService.ProcessIncomingTarballFilesAsync(cancellationToken);
            // await _techtalkVideoService.CreateTarballsFromDirectoriesAsync(cancellationToken);
            await _techTalkVideoService.CompressTarballsInArchiveFolderAsync(cancellationToken);

            await _toastmastersVideoService.ProcessIncomingTarballFilesAsync(cancellationToken);
            await _toastmastersVideoService.CreateTarballsFromDirectoriesAsync(cancellationToken);

            previousTime = await DelayWhenLoopingQuickly(cancellationToken, previousTime);
        }
    }

    private async Task<DateTime> DelayWhenLoopingQuickly(CancellationToken cancellationToken, DateTime previousTime)
    {
        if ((previousTime - DateTime.Now) < _delayLoopTime)
        {
            await Task.Delay(_appSettings.LongWorkerDelay, cancellationToken);
        }

        return DateTime.Now;
    }

}