using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Handyman;
using Almostengr.VideoProcessor.Core.TechTalk;

namespace Almostengr.VideoProcessor.Worker.Workers;

internal sealed class SubtitleWorker : BaseWorker
{
    private readonly IHandymanVideoService _handymanService;
    private readonly ITechTalkVideoService _techTalkService;

    public SubtitleWorker(IHandymanVideoService handymanService, ITechTalkVideoService techTalkService,
        AppSettings appSettings) : base(appSettings)
    {
        _handymanService = handymanService;
        _techTalkService = techTalkService;
        _appSettings = appSettings;
    }

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        DateTime previousTime = DateTime.Now;

        while (!cancellationToken.IsCancellationRequested)
        {
            _handymanService.ProcessSrtSubtitleFile();
            _techTalkService.ProcessSrtSubtitleFile();
            previousTime = await DelayWhenLoopingQuickly(_delayLoopTime, cancellationToken, previousTime);
        }
    }
}