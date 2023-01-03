using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;

namespace Almostengr.VideoProcessor.Domain.Videos.Handyman;

public sealed record HandymanVideo : BaseVideo
{
    public HandymanVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public override string BannerTextColor()
    {
        return FfMpegColor.RhtYellow;
    }

    public override string BannerBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public override string SubtitleBackgroundColor()
    {
        return BannerBackgroundColor();
    }

    public override string SubtitleTextColor()
    {
        return BannerTextColor();
    }
}