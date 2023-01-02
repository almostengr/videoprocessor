using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;

namespace Almostengr.VideoProcessor.Domain.DashCam;

public sealed record DashCamVideo : BaseVideo
{
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


    public void AddDetailsContentToVideoFilter(string[] fileContents)
    {
        const int DISPLAY_DURATION = 5;
        const string SPEED_LIMIT = "speed limit";

        foreach (string line in fileContents)
        {
            string[] splitLine = line.Split(Constant.Pipe);
            int startSeconds = Int32.Parse(splitLine[0]);
            int endSeconds = startSeconds + DISPLAY_DURATION;
            string displayText = splitLine[1].Replace(":", "\\:");

            string textColor = FfMpegColors.White;
            string backgroundColor = FfMpegColors.Green;

            if (displayText.ToLower().Contains(SPEED_LIMIT))
            {
                textColor = FfMpegColors.Black;
                backgroundColor = FfMpegColors.White;
            }

            AddDrawTextFilter(displayText, textColor, Constant.SolidText, FfmpegFontSize.Small, 
                DrawTextPosition.LowerRight, backgroundColor, Constant.SolidBackground, 
                $"enable='between(t,{startSeconds},{endSeconds})'");
        }
    }
}
