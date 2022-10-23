using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

public sealed class SrtSubtitleContentsAreInvalidException : VideoProcessorException
{
    public SrtSubtitleContentsAreInvalidException()
    {
    }

    public SrtSubtitleContentsAreInvalidException(string message) : base(message)
    {
    }
}
