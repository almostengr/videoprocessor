using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

[Serializable]
internal class SrtSubtitleBaseDirectoryIsNullOrEmptyException : VideoProcessorException
{
    public SrtSubtitleBaseDirectoryIsNullOrEmptyException()
    {
    }

    public SrtSubtitleBaseDirectoryIsNullOrEmptyException(string? message) : base(message)
    {
    }

    public SrtSubtitleBaseDirectoryIsNullOrEmptyException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SrtSubtitleBaseDirectoryIsNullOrEmptyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
