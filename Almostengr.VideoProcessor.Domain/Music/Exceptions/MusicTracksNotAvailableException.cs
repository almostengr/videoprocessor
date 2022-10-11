using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Music.Exceptions;

[Serializable]
internal sealed  class MusicTracksNotAvailableException : Exception
{
    public MusicTracksNotAvailableException()
    {
    }

    public MusicTracksNotAvailableException(string message) : base(message)
    {
    }

    public MusicTracksNotAvailableException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public MusicTracksNotAvailableException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}