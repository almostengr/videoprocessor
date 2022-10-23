using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

internal sealed class SrtSubtitleTextIsNullOrWhiteSpaceException : VideoProcessorException
{
    public SrtSubtitleTextIsNullOrWhiteSpaceException()
    {
    }

    public SrtSubtitleTextIsNullOrWhiteSpaceException(string message) : base(message)
    {
    }
}
