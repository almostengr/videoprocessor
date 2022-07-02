using Almostengr.VideoProcessor.Core.VideoDashCam;

namespace Almostengr.VideoProcessor.Worker
{
    public class DashCamVideoWorker : BackgroundService
    {
        private readonly IDashCamVideoService _videoService;

        public DashCamVideoWorker(IDashCamVideoService dashCamVideoService)
        {
            _videoService = dashCamVideoService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _videoService.StartAsync(stoppingToken);
            await _videoService.ExecuteServiceAsync(stoppingToken);
        }

    }
}
