using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IVideoRenderService
    {
        Task RenderChannelVideosAsync(ChannelPropertiesDto channelProperties);
    }
}