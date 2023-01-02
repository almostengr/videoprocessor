namespace Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

internal class SubtitleFilePathIsNullOrWhiteSpaceException : Exception
{
    public SubtitleFilePathIsNullOrWhiteSpaceException()
    {
    }

    public SubtitleFilePathIsNullOrWhiteSpaceException(string? message) : base(message)
    {
    }
}