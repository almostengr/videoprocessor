using Almostengr.VideoProcessor.Domain.Videos;

namespace Almostengr.VideoProcessor.Domain.ToastmastersVideo;

public sealed record ToastmastersVideo : BaseVideo
{
    public ToastmastersVideo(string baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public override string BoxColor()
    {
        return FfMpegColors.SteelBlue;
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