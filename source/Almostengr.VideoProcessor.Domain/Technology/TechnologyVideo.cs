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
    
    public override string ChannelBannerText()
    {
        if (IsChristmasVideo)
        {
            string[] text = { "rhtservices.net", "twitter.com/hplightshow" };
            return text[_random.Next(0, text.Count())];
        }

        return ChannelBannerTextRhtServices();
    }

    public override string BannerTextColor()
    {
        return FfMpegColors.White;
    }

    public override string BannerBackgroundColor()
    {
        if (IsChristmasVideo)
        {
            return FfMpegColors.Maroon;
        }

        return FfMpegColors.Green;
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
