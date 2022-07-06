using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.Core.VideoHandyTech;

namespace Almostengr.VideoProcessor.Worker
{
    public class HandyTechVideoWorker : BackgroundService
    {
        private readonly IHandyTechVideoService _videoService;
        private readonly AppSettings _appSettings;

        public HandyTechVideoWorker(IHandyTechVideoService handyTechVideoService, AppSettings appSettings)
        {
            _videoService = handyTechVideoService;
            _appSettings = appSettings;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _videoService.StartAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var incomingDirectory = Path.Combine(_appSettings.Directories.RhtBaseDirectory, "incoming");
                string videoArchive = _videoService.GetRandomVideoArchiveInDirectory(incomingDirectory);
                bool isDiskSpaceAvailable = _videoService.IsDiskSpaceAvailable(incomingDirectory, _appSettings.DiskSpaceThreshold);

                if (string.IsNullOrEmpty(videoArchive) || isDiskSpaceAvailable == false)
                {
                    await _videoService.WorkerIdleAsync(stoppingToken);
                    continue;
                }

                await _videoService.ExecuteServiceAsync(videoArchive, stoppingToken);
            }
        }

    }
}
