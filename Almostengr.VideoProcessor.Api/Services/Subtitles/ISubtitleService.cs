using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface ISubtitleService : IBaseSubtitleService
    {
        void ArchiveTranscript(string transcriptFilePath, string archiveDirectory);
        SubtitleOutputDto CleanTranscript(SubtitleInputDto inputDto);
        string[] GetIncomingTranscripts(string directory);
        void SaveTranscript(SubtitleOutputDto transcriptDto, string archiveDirectory);
    }
}
