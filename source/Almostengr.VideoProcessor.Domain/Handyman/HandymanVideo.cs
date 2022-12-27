using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos.Handyman;

public sealed record HandymanVideo : BaseVideo
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