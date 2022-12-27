namespace Almostengr.VideoProcessor.Domain.Common.Exceptions.Subtitles;

public sealed class SrtSubtitleContentsAreInvalidException : VideoProcessorException
{
    public SrtSubtitleContentsAreInvalidException()
    {
    }

    public SrtSubtitleContentsAreInvalidException(string message) : base(message)
    {
    }
}
