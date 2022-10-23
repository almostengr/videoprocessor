namespace Almostengr.VideoProcessor.Domain.Videos.HandymanVideo;

internal sealed record HandymanVideo : BaseVideo
{
    public HandymanVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public override string ChannelBannerText()
    {
        return ChannelBannerTextRhtServices();
    }

    public override string TextColor()
    {
        return FfMpegColors.RhtYellow;
    }

    public override string BoxColor()
    {
        return FfMpegColors.Black;
    }
}