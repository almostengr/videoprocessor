using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface ITranscriptService : IBaseTranscriptService
    {
        TranscriptOutputDto CleanTranscript(TranscriptInputDto inputDto);
        string[] GetIncomingTranscripts();
        void ArchiveTranscript(string transcriptFile);
        void SaveTranscript(TranscriptOutputDto outputDto);
    }
}
