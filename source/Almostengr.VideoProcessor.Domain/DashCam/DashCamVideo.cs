using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;

namespace Almostengr.VideoProcessor.Domain.DashCam;

public sealed record DashCamVideo : BaseVideo
{
    public IEnumerable<DashCamVideoSubtitle> Subtitles;    
    private readonly string Night = "night";

    public DashCamVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public override string BannerBackgroundColor()
    {
        return FfMpegColors.Black;
    }

    public override string ChannelBannerText()
    {
        return "Kenny Ram Dash Cam";
    }

    public override string BannerTextColor()
    {
        if (Title.ToLower().Contains(Night))
        {
            return FfMpegColors.Orange;
        }

        return FfMpegColors.White;
    }

    public override string SubtitleBackgroundColor()
    {
        if (Title.ToLower().Contains(Night))
        {
            return FfMpegColors.DarkGreen;
        }

        return FfMpegColors.Green;
    }

    public override string SubtitleTextColor()
    {
        if (Title.ToLower().Contains(Night))
        {
            return FfMpegColors.GhostWhite;
        }

        return FfMpegColors.White;
    }

    public string GetDetailsFileName()
    {
        return "details.txt";
    }

    public string GetDetailsFilePath()
    {
        return Path.Combine(WorkingDirectory, GetDetailsFileName());
    }

    public void SetSubtitles(string text)
    {

    }
}

public sealed record DashCamVideoSubtitle
{
    public DashCamVideoSubtitle(string input)
    {
        string[] splitInput = input.Split(Constant.Comma);




    }

    public int StartSeconds { get; init; }
    // public int EndSeconds { get; private set; }
    public string Text { get; init; }


}