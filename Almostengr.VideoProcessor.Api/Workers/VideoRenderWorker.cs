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
                ChannelPropertiesDto channelProperties = 
                    _channelPropertiesService.GetChannelProperties(ChannelName.RhtServices);

                _videoRenderService.RenderChannelVideos(channelProperties);

                channelProperties = 
                    _channelPropertiesService.GetChannelProperties(ChannelName.DashCam);

                _videoRenderService.RenderChannelVideos(channelProperties);
                    
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
        
    }
}