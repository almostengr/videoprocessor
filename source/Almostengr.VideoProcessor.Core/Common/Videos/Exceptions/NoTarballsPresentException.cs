using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

[Serializable]
public class NoTarballsPresentException : VideoProcessorException
{
    public NoTarballsPresentException()
    {
    }

    public NoTarballsPresentException(string? message) : base(message)
    {
    }

    public NoTarballsPresentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected NoTarballsPresentException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}