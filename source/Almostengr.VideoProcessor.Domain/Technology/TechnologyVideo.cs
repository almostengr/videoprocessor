using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;

namespace Almostengr.VideoProcessor.Domain.Technology;

public sealed record TechnologyVideo : BaseVideo
{
    public bool IsChristmasVideo { get; private set; }

    public TechnologyVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
        IsChristmasVideo = false;
    }

    public void ConfirmChristmasVideo()
    {
        if (Title.ToLower().Contains("christmas video"))
        {
            IsChristmasVideo = true;
        }
    }

    public override string BannerTextColor()
    {
        return FfMpegColor.White;
    }

    public override string BannerBackgroundColor()
    {
        if (IsChristmasVideo)
        {
            return FfMpegColor.Maroon;
        }

        return FfMpegColor.Green;
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
