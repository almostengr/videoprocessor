using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

[Serializable]
internal sealed class SrtSubtitleTextIsNullOrWhiteSpaceException : VideoProcessorException
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
