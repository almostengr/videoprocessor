using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

[Serializable]
internal class SrtSubtitleContentsAreInvalidException : VideoProcessorException
{
    public SrtSubtitleContentsAreInvalidException()
    {
    }

    public SrtSubtitleContentsAreInvalidException(string message) : base(message)
    {
    }

    public SrtSubtitleContentsAreInvalidException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SrtSubtitleContentsAreInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
