using System;
using System.Threading;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Services;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Workers
{
    public class VideoRenderWorker : BaseWorker
    {
        private readonly IChannelPropertiesService _channelPropertiesService;
        private readonly IVideoRenderService _videoRenderService;

        public VideoRenderWorker(ILogger<BaseWorker> logger,
            IChannelPropertiesService channelPropertiesService,
            IVideoRenderService videoRenderService
            ) : base(logger)
        {
            _channelPropertiesService = channelPropertiesService;
            _videoRenderService = videoRenderService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime startTime = DateTime.Now;

                ChannelPropertiesDto channelProperties =
                    _channelPropertiesService.GetChannelProperties(ChannelName.RhtServices);

                await _videoRenderService.RenderChannelVideosAsync(channelProperties);

                channelProperties =
                    _channelPropertiesService.GetChannelProperties(ChannelName.DashCam);

                await _videoRenderService.RenderChannelVideosAsync(channelProperties);

                TimeSpan elapsedTime = DateTime.Now - startTime;

                if (elapsedTime.TotalMinutes < 1)
                {
                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                }

            }
        }

    }
}