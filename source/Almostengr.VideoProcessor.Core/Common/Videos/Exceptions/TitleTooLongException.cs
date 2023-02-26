using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

[Serializable]
internal class TitleTooLongException : VideoProcessorException
{
    public TitleTooLongException()
    {
    }

    public TitleTooLongException(string? message) : base(message)
    {
    }

    public TitleTooLongException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected TitleTooLongException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}