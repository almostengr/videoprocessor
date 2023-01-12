
namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

public sealed class SubtitleBaseDirectoryIsNullOrEmptyException : VideoProcessorException
{
    public SubtitleBaseDirectoryIsNullOrEmptyException() : base()
    {
    }

    public SubtitleBaseDirectoryIsNullOrEmptyException(string message) : base(message)
    {
    }
}
