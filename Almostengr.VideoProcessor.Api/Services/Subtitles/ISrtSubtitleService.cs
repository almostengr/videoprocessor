using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services.Subtitles
{
    public interface ISrtSubtitleService : ISubtitleService
    {
        void ArchiveTranscript(string transcriptFilePath, string archiveDirectory);
        SubtitleOutputDto CleanTranscript(SubtitleInputDto inputDto);
        string[] GetIncomingTranscripts(string directory);
        void SaveTranscript(SubtitleOutputDto transcriptDto, string archiveDirectory);
    }
}
