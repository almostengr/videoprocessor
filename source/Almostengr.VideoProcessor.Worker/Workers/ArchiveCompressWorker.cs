using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.DashCam;
using Almostengr.VideoProcessor.Core.Handyman;
using Almostengr.VideoProcessor.Core.TechTalk;
using Almostengr.VideoProcessor.Core.Toastmasters;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class ArchiveCompressWorker : BaseWorker
{
    private readonly IHandymanVideoService _handymanVideoService;
    private readonly ITechTalkVideoService _techTalkVideoService;
    private readonly IToastmastersVideoService _toastmastersVideoService;
    private readonly IDashCamVideoService _dashCamVideoService;
    private readonly AppSettings _appSettings;

    public ArchiveCompressWorker(AppSettings appSettings, IHandymanVideoService handymanVideoService,
        ITechTalkVideoService techTalkVideoService, IToastmastersVideoService toastmastersVideoService,
        IDashCamVideoService dashCamVideoService
    ) : base(appSettings)
    {
        _handymanVideoService = handymanVideoService;
        _techTalkVideoService = techTalkVideoService;
        _toastmastersVideoService = toastmastersVideoService;
        _dashCamVideoService = dashCamVideoService;
        _appSettings = appSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _handymanVideoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
        await _techTalkVideoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
        await _toastmastersVideoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
        await _dashCamVideoService.CompressTarballsInArchiveFolderAsync(cancellationToken);
        await Task.Delay(_appSettings.WorkerDelay);
    }
}