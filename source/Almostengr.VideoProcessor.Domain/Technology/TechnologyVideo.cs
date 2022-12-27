using Almostengr.VideoProcessor.Domain.Common;

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

    public override string TextColor()
    {
        return FfMpegColors.White;
    }

    public override string BoxColor()
    {
        if (IsChristmasVideo)
        {
            return FfMpegColors.Maroon;
        }

        return FfMpegColors.Green;
    }
}
