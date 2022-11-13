namespace Almostengr.VideoProcessor.Domain.Videos.ChristmasLightShow;

public sealed record ChristmasLightVideo : BaseVideo
{
    public ChristmasLightVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public override string ChannelBannerText()
    {
        return "rhtservices.net/christmas";
    }

    public override string TextColor()
    {
        return FfMpegColors.Maroon;
    }

    public override string BoxColor()
    {
        return FfMpegColors.White;
    }
}
