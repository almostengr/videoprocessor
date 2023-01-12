
namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

internal class SubtitleTextIsNullOrWhitespaceException : VideoProcessorException
{
    public SubtitleTextIsNullOrWhitespaceException() : base()
    {
    }

    public SubtitleTextIsNullOrWhitespaceException(string message) : base(message)
    {
    }
}