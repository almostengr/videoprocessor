using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
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
            int count = 0;

            try
            {
                await _handymanService.ProcessIncomingSubtitlesAsync(cancellationToken);
            }
            catch (NoSubtitleFilesPresentException)
            {
                count++;
            }

            try
            {
                await _techTalkService.ProcessIncomingSubtitlesAsync(cancellationToken);
            }
            catch (NoSubtitleFilesPresentException)
            {
                count++;
            }

            if (count >= 2)
            {
                await Task.Delay(_appSettings.WorkerDelay);
            }
        }
    }
}