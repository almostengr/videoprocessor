using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Videos.Exceptions;

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