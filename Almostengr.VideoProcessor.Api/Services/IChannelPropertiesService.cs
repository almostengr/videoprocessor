using Almostengr.VideoProcessor.Api.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IChannelPropertiesService
    {
        ChannelPropertiesDto GetChannelProperties(string channelName);
    }
}