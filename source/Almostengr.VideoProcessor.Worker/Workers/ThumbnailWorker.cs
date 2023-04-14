using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Handyman;
using Almostengr.VideoProcessor.Core.TechTalk;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class ThumbnailWorker : BaseWorker
{
    private readonly IHandymanVideoService _handymanVideoService;
    private readonly ITechTalkVideoService _techTalkVideoService;
    private readonly AppSettings _appSettings;

    public ThumbnailWorker(AppSettings appSettings,
        IHandymanVideoService handymanVideoService, ITechTalkVideoService techtalkvideoservice) : base(appSettings)
    {
        _handymanVideoService = handymanVideoService;
        _techTalkVideoService = techtalkvideoservice;
        _appSettings = appSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _handymanVideoService.CreateThumbnails();
        _techTalkVideoService.CreateThumbnails();
        await Task.Delay(_appSettings.LongWorkerDelay, cancellationToken);
    }
}