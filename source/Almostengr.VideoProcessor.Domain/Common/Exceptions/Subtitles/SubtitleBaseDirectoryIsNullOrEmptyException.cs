namespace Almostengr.VideoProcessor.Domain.Common.Exceptions.Subtitles;

public sealed class SubtitleBaseDirectoryIsNullOrEmptyException : VideoProcessorException
{
    public SubtitleBaseDirectoryIsNullOrEmptyException()
    {
    }

    public SubtitleBaseDirectoryIsNullOrEmptyException(string message) : base(message)
    {
    }
}
