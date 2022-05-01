using Almostengr.VideoProcessor.Api.Constants;
using Almostengr.VideoProcessor.Api.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IChannelPropertiesService
    {
        // ChannelPropertiesDto GetChannelProperties(ChannelName channelName);
        ChannelPropertiesDto GetChannelProperties(string channelName);
    }
}