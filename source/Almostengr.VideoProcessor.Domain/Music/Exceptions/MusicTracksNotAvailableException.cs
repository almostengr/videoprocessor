namespace Almostengr.VideoProcessor.Domain.Music.Exceptions;

public sealed class MusicTracksNotAvailableException : Exception
{
    public MusicTracksNotAvailableException()
    {
    }

    public MusicTracksNotAvailableException(string message) : base(message)
    {
    }
}