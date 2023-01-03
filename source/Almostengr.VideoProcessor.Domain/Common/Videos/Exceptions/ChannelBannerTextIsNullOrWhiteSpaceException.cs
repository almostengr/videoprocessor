namespace Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

public sealed class ChannelBannerTextIsNullOrWhiteSpaceException : VideoProcessorException
{
    public ChannelBannerTextIsNullOrWhiteSpaceException() : base()
    {
    }

    public ChannelBannerTextIsNullOrWhiteSpaceException(string message) : base(message)
    {
    }
}