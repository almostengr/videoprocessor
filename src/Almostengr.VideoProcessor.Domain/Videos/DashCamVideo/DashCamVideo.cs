namespace Almostengr.VideoProcessor.Domain.Videos.DashCamVideo;

public sealed record DashCamVideo : BaseVideo
{
    public DashCamVideo()
    {
        BaseDirectory = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/Kenny Ram Dash Cam";
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