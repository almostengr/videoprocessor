using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Videos.Exceptions;

[Serializable]
internal class NoTarballsPresentException : VideoProcessorException
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