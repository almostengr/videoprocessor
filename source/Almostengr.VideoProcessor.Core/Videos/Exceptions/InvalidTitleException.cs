using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Videos.Exceptions;

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