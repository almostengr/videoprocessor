using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
{
    public class VideoRenderService : IVideoRenderService
    {
        private readonly ILogger<VideoRenderService> _logger;

        public VideoRenderService(ILogger<VideoRenderService> logger)
        {
            _logger = logger;
        }

        public void RenderChannelVideos(ChannelPropertiesDto channelProperties)
        {
            _logger.LogInformation($"VideoRenderService: Rendering {channelProperties.Name} videos");

            // check the input directory for the channel

            // untar the files to the working directory

            // change to the working directory

            // if files are not mp4, convert them to mp4

            // remove non mp4 video files from the working directory

            // create the output video
        }
    }
}