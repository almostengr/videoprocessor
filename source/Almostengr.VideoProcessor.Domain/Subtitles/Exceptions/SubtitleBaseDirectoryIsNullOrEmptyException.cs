using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

public sealed class SrtSubtitleBaseDirectoryIsNullOrEmptyException : VideoProcessorException
{
    public SrtSubtitleBaseDirectoryIsNullOrEmptyException()
    {
    }

    public SrtSubtitleBaseDirectoryIsNullOrEmptyException(string message) : base(message)
    {
    }
}
