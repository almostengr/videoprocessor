using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.Core.Subtitles;

namespace Almostengr.VideoProcessor.Worker
{
    public class HandyTechSubtitleWorker : BackgroundService
    {
        private readonly ISrtSubtitleService _srtSubtitleService;
        private readonly AppSettings _appSettings;

        public HandyTechSubtitleWorker(ISrtSubtitleService srtSubtitleService, AppSettings appSettings)
        {
            _srtSubtitleService = srtSubtitleService;
            _appSettings = appSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _srtSubtitleService.StartAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var incomingDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "incoming");
                string subtitleFile = _srtSubtitleService.GetRandomSubtitleFile(incomingDirectory);
                bool isDiskSpaceAvailable =
                    _srtSubtitleService.IsDiskSpaceAvailable(incomingDirectory, _appSettings.DiskSpaceThreshold);

                if (string.IsNullOrEmpty(subtitleFile) || isDiskSpaceAvailable == false)
                {
                    await _srtSubtitleService.WorkerIdleAsync(stoppingToken);
                    continue;
                }

                await _srtSubtitleService.ExecuteServiceAsync(incomingDirectory, stoppingToken);
            }
        }

    }
}
