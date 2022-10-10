using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos;

public sealed class VideoInvalidChannelTypeException : VideoProcessorException
{
    public VideoInvalidChannelTypeException()
    {
    }

    public VideoInvalidChannelTypeException(string message) : base(message)
    {
    }

    public VideoInvalidChannelTypeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public VideoInvalidChannelTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
