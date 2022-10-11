using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos;

internal sealed class VideoInvalidBaseDirectoryException : VideoProcessorException
{
    public VideoInvalidBaseDirectoryException()
    {
    }

    public VideoInvalidBaseDirectoryException(string message) : base(message)
    {
    }

    public VideoInvalidBaseDirectoryException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public VideoInvalidBaseDirectoryException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}