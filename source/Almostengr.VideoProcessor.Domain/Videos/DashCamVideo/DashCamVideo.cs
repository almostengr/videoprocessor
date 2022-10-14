namespace Almostengr.VideoProcessor.Domain.Videos.DashCamVideo;

public sealed record DashCamVideo : BaseVideo
{
    private readonly string Night = "night";

    public DashCamVideo(string baseDirectory)
    {
        BaseDirectory = baseDirectory;
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
        if (Title.ToLower().Contains(Night))
        {
            return FfMpegColors.Orange;
        }

        return FfMpegColors.White;
    }
}