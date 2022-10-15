using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Common.Exceptions;

[Serializable]
public class NoSrtFilesPresentException : VideoProcessorException
{
    public NoSrtFilesPresentException()
    {
    }

    public NoSrtFilesPresentException(string? message) : base(message)
    {
    }

    public NoSrtFilesPresentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected NoSrtFilesPresentException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}