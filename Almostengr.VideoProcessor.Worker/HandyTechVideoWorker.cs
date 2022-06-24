using Almostengr.VideoProcessor.Core.Services.Video;

namespace Almostengr.VideoProcessor.Worker
{
    public class HandyTechVideoWorker : BackgroundService
    {
        private readonly IHandyTechVideoService _videoService;

        public HandyTechVideoWorker(ILogger<HandyTechVideoWorker> logger, IHandyTechVideoService handyTechVideoService)
        {
            _videoService = handyTechVideoService;
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
