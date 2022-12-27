
namespace Almostengr.VideoProcessor.Domain.Common.Exceptions.Subtitles;

internal class SubtitleTextIsNullOrWhitespaceException : Exception
{
    public SubtitleTextIsNullOrWhitespaceException()
    {
    }

    public SubtitleTextIsNullOrWhitespaceException(string? message) : base(message)
    {
    }
}