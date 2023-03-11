namespace Almostengr.VideoProcessor.Core.Common.Transcripts;

public interface IBaseTranscriptionService
{
    void ProcessSrtSubtitles(CancellationToken cancellationToken);
}