using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

[Serializable]
internal class TarballCreationException : Exception
{
    public TarballCreationException()
    {
    }

    public TarballCreationException(string? message) : base(message)
    {
    }

    public TarballCreationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected TarballCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}