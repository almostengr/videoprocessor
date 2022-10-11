using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos;

public sealed record DashCamVideo : VideoBase
{
    public DashCamVideo()
    {
        BaseDirectory = Constants.DashCamBaseDirectory;
    }

    public override string BoxColor()
    {
        return FfMpegColors.Black;
    }

    public override string ChannelBannerText()
    {
        return "Kenny Ram Dash Cam";
    }

    public override string TextColor()
    {
        if (Title.ToLower().Contains("night"))
        {
            return FfMpegColors.Orange;
        }

        return FfMpegColors.White;
    }
}