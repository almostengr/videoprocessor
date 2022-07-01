using Almostengr.VideoProcessor.Core.Services.Video;

namespace Almostengr.VideoProcessor.Worker
{
    public class HandyTechVideoWorker : BackgroundService
    {
        private readonly IHandyTechVideoService _videoService;

        public HandyTechVideoWorker(IHandyTechVideoService handyTechVideoService)
        {
            _videoService = handyTechVideoService;
        }

        public override Task ExecuteTask => base.ExecuteTask;

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _videoService.StartAsync(stoppingToken);
            await _videoService.ExecuteServiceAsync(stoppingToken);
        }

        // public async override Task StartAsync(CancellationToken cancellationToken)
        // {
        //     await _videoService.StartAsync(cancellationToken);
        // }

    }
}
