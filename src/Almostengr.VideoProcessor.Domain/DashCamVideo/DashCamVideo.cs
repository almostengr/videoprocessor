using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Videos;

namespace Almostengr.VideoProcessor.Domain.DashCamVideo;

public sealed record DashCamVideo : BaseVideo
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