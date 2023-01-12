using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common;

public class VideoProcessorException : Exception
{
    protected VideoProcessorException() : base()
    {
    }
    
    public VideoProcessorException(string? message) : base(message)
    {
    }

    public VideoProcessorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected VideoProcessorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}