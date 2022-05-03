using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Services
{
    public interface ITranscriptService : IBaseTranscriptService
    {
        TranscriptOutputDto CleanTranscript(TranscriptInputDto inputDto);
        string[] GetTranscriptList(string srt);
        void ArchiveTranscript(string transcriptFile);
        void SaveTranscript(TranscriptOutputDto outputDto);
    }
}
