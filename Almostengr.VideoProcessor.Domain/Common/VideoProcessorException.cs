using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Common;

public abstract class VideoProcessorException : Exception
{
    protected VideoProcessorException()
    {
    }
    
    public VideoProcessorException(string message) : base(message)
    {
    }

    public VideoProcessorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected VideoProcessorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}