using Almostengr.VideoProcessor.Domain.Videos;

namespace Almostengr.VideoProcessor.Domain.ToastmastersVideo;

public sealed record ToastmastersVideo : BaseVideo
{
    public ToastmastersVideo()
    {
        BaseDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/Toastmasters";
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