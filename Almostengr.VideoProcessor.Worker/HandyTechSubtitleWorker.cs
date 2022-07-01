using Almostengr.VideoProcessor.Core.Services.Subtitles;

namespace Almostengr.VideoProcessor.Worker
{
    public class HandyTechSubtitleWorker : BackgroundService
    {
        private readonly ISrtSubtitleService _subtitleService;

        public HandyTechSubtitleWorker(ILogger<HandyTechSubtitleWorker> logger, ISrtSubtitleService srtSubtitleService)
        {
            _subtitleService = srtSubtitleService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _subtitleService.StartAsync(stoppingToken);
            await _subtitleService.ExecuteAsync(stoppingToken);
        }

    }
}
