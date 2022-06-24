using Almostengr.VideoProcessor.Core.Services.Video;

namespace Almostengr.VideoProcessor.Worker
{
    public class DashCamVideoWorker : BackgroundService
    {
        private readonly IDashCamVideoService _videoService;

        public DashCamVideoWorker(ILogger<DashCamVideoWorker> logger, IDashCamVideoService dashCamVideoService)
        {
            _videoService = dashCamVideoService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _videoService.ExecuteAsync(stoppingToken);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _videoService.StartAsync(cancellationToken);
            // return base.StartAsync(cancellationToken);
        }

    }
}
