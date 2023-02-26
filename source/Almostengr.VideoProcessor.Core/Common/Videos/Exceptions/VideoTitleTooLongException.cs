using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

[Serializable]
internal class VideoTitleTooLongException : VideoProcessorException
{
    public VideoTitleTooLongException()
    {
    }

    public VideoTitleTooLongException(string? message) : base(message)
    {
    }

    public VideoTitleTooLongException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected VideoTitleTooLongException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}