namespace Almostengr.VideoProcessor.Domain.Common.Subtitles;

public sealed class SubtitleBaseDirectoryIsNullOrEmptyException : VideoProcessorException
{
    public SubtitleBaseDirectoryIsNullOrEmptyException() : base()
    {
    }

    public SubtitleBaseDirectoryIsNullOrEmptyException(string message) : base(message)
    {
    }
}
