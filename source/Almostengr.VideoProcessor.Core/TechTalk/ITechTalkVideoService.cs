using Almostengr.VideoProcessor.Core.Common.Transcripts;
using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.TechTalk
{
    public interface ITechTalkVideoService : IBaseVideoAudioService, IBaseTranscriptionService
    {
        void CreateThumbnails();
        
    }
}