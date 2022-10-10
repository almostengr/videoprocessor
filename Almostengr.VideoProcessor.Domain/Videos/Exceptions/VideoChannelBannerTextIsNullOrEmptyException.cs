using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos;

internal sealed class VideoChannelBannerTextIsNullOrEmptyException : VideoProcessorException
{
    public VideoChannelBannerTextIsNullOrEmptyException()
    {
    }

    public VideoChannelBannerTextIsNullOrEmptyException(string message) : base(message)
    {
    }

    public VideoChannelBannerTextIsNullOrEmptyException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public VideoChannelBannerTextIsNullOrEmptyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}