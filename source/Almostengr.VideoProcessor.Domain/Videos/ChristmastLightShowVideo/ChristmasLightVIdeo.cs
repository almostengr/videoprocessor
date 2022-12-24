namespace Almostengr.VideoProcessor.Domain.Videos.ChristmasLightShow;

public sealed record ChristmasLightVideo : BaseVideo
{
    public ChristmasLightVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public override string ChannelBannerText()
    {
        string[] text = { "rhtservices.net", "twitter.com/hplightshow" };
        return text[_random.Next(0, text.Count())];
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
