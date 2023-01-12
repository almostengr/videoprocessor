using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

[Serializable]
internal class InvalidVideoTitleException : VideoProcessorException
{
    public InvalidVideoTitleException()
    {
    }

    public InvalidVideoTitleException(string? message) : base(message)
    {
    }

    public InvalidVideoTitleException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected InvalidVideoTitleException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}