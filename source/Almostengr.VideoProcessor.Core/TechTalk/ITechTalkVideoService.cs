using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.TechTalk
{
    public interface ITechTalkVideoService : IBaseVideoService
    {
        void CreateThumbnails();
    }
}