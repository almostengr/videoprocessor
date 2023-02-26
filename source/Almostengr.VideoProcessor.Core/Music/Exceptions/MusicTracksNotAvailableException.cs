namespace Almostengr.VideoProcessor.Core.Music.Exceptions;

public sealed class MusicTracksNotAvailableException : Exception
{
    public MusicTracksNotAvailableException()
    {
    }

    public MusicTracksNotAvailableException(string message) : base(message)
    {
    }
}