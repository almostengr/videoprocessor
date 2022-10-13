using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Videos;

namespace Almostengr.VideoProcessor.Domain.ToastmastersVideo;

public sealed record ToastmastersVideo : BaseVideo
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