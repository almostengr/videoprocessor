using Almostengr.VideoProcessor.Api.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IVideoRenderService
    {
        void RenderChannelVideos(ChannelPropertiesDto channelProperties);
    }
}