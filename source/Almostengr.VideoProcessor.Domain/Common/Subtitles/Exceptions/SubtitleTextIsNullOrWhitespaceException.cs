
namespace Almostengr.VideoProcessor.Domain.Common.Subtitles;

internal class SubtitleTextIsNullOrWhitespaceException : VideoProcessorException
{
    public SubtitleTextIsNullOrWhitespaceException() : base()
    {
    }

    public SubtitleTextIsNullOrWhitespaceException(string message) : base(message)
    {
    }
}