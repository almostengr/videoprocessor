namespace Almostengr.VideoProcessor.Core.Common.Transcripts;

public interface IBaseTranscriptionService
{
    Task ProcessSrtSubtitlesAsync(CancellationToken cancellationToken);
}