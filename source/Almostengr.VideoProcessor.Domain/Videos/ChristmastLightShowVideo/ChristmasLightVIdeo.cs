namespace Almostengr.VideoProcessor.Domain.Videos.ChristmasLightShow;

public sealed record ChristmasLightVideo : BaseVideo
{
    public ChristmasLightVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public override string ChannelBannerText()
    {
        return ChannelBannerTextRhtServices();
    }

    public override string TextColor()
    {
        return FfMpegColors.White;
    }

    public override string BoxColor()
    {
        return FfMpegColors.Maroon;
    }
}
