using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos.Exceptions;

internal sealed class VideoInvalidBaseDirectoryException : VideoProcessorException
{
    public VideoInvalidBaseDirectoryException()
    {
    }

    public VideoInvalidBaseDirectoryException(string message) : base(message)
    {
    }
}