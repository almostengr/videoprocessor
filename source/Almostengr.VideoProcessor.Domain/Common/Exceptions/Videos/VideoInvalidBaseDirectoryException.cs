namespace Almostengr.VideoProcessor.Domain.Common.Exceptions;

public sealed class VideoInvalidBaseDirectoryException : VideoProcessorException
{
    public VideoInvalidBaseDirectoryException()
    {
    }

    public VideoInvalidBaseDirectoryException(string message) : base(message)
    {
    }
}