namespace Almostengr.VideoProcessor.Domain.Common.Subtitles;

public sealed class SrtSubtitleContentsAreInvalidException : VideoProcessorException
{
    public SrtSubtitleContentsAreInvalidException() : base()
    {
    }

    public SrtSubtitleContentsAreInvalidException(string message) : base(message)
    {
    }
}
