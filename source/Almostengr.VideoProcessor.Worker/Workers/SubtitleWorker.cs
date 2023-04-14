using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Handyman;
using Almostengr.VideoProcessor.Core.TechTalk;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class SubtitleWorker : BaseWorker
{
    private readonly IHandymanVideoService _handymanService;
    private readonly ITechTalkVideoService _techTalkService;
    private readonly AppSettings _appSettings;

    public SubtitleWorker(IHandymanVideoService handymanService, ITechTalkVideoService techTalkService,
        AppSettings appSettings) : base(appSettings)
    {
        _handymanService = handymanService;
        _techTalkService = techTalkService;
        _appSettings = appSettings;
    }

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // await _handymanService.ProcessIncomingSubtitlesAsync(cancellationToken);
            // await _techTalkService.ProcessIncomingSubtitlesAsync(cancellationToken);
            await Task.Delay(_appSettings.LongWorkerDelay);
        }
    }
}