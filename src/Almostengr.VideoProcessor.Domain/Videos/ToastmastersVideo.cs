using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos;

public sealed record ToastmastersVideo : VideoBase
{
    public ToastmastersVideo()
    {
        BaseDirectory = Constants.ToastmastersBaseDirectory;
    }

    public override string BoxColor()
    {
        return FfMpegColors.Black;
    }

    public override string ChannelBannerText()
    {
        return "towertoastmasters.org";
    }

    public override string TextColor()
    {
        return FfMpegColors.White;
    }
}