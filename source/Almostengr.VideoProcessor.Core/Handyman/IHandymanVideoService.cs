using Almostengr.VideoProcessor.Core.Common.Transcripts;
using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.Handyman
{
    public interface IHandymanVideoService : IBaseVideoService, IBaseTranscriptionService
    {
        void CreateThumbnails();
    }
}