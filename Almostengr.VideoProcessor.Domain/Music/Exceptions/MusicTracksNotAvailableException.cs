using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Music.Exceptions;

[Serializable]
internal class MusicTracksNotAvailableException : Exception
{
    public MusicTracksNotAvailableException()
    {
    }

    public MusicTracksNotAvailableException(string? message) : base(message)
    {
    }

    public MusicTracksNotAvailableException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected MusicTracksNotAvailableException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}