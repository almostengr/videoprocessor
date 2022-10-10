using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

[Serializable]
internal class SrtSubtitleContentsAreInvalidException : Exception
{
    public SrtSubtitleContentsAreInvalidException()
    {
    }

    public SrtSubtitleContentsAreInvalidException(string? message) : base(message)
    {
    }

    public SrtSubtitleContentsAreInvalidException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SrtSubtitleContentsAreInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
