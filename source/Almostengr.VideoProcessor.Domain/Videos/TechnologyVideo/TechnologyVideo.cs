namespace Almostengr.VideoProcessor.Domain.Videos.TechnologyVideo;

public sealed record TechnologyVideo : BaseVideo
{
    public TechnologyVideo(string baseDirectory) : base(baseDirectory)
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
        return FfMpegColors.Green;
    }

}