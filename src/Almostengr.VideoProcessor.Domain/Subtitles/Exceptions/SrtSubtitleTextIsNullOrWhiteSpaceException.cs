using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

[Serializable]
internal class SrtSubtitleTextIsNullOrWhiteSpaceException : Exception
{
    public SrtSubtitleTextIsNullOrWhiteSpaceException()
    {
    }

    public SrtSubtitleTextIsNullOrWhiteSpaceException(string message) : base(message)
    {
    }

    public SrtSubtitleTextIsNullOrWhiteSpaceException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SrtSubtitleTextIsNullOrWhiteSpaceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
