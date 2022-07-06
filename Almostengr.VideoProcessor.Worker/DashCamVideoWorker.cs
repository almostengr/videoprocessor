using Almostengr.VideoProcessor.Core.Configuration;
using Almostengr.VideoProcessor.Core.VideoDashCam;

namespace Almostengr.VideoProcessor.Worker
{
    public class DashCamVideoWorker : BackgroundService
    {
        private readonly IDashCamVideoService _videoService;
        private readonly AppSettings _appSettings;

        public DashCamVideoWorker(IDashCamVideoService dashCamVideoService, AppSettings appSettings)
        {
            _videoService = dashCamVideoService;
            _appSettings = appSettings;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _videoService.StartAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var incomingDirectory = Path.Combine(_appSettings.Directories.DashCamBaseDirectory, "incoming");
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
