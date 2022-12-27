namespace Almostengr.VideoProcessor.Domain.Common.Exceptions.Subtitles;

internal class InvalidSubtitleFileException : Exception
{
    public InvalidSubtitleFileException()
    {
    }

    public InvalidSubtitleFileException(string? message) : base(message)
    {
    }
}
