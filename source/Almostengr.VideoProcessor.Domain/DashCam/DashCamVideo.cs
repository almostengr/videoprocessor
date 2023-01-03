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
        return FfMpegColor.Black;
    }

    public override string BannerTextColor()
    {
        if (Title.ToLower().Contains(Night))
        {
            return FfMpegColor.Orange;
        }

        return FfMpegColor.White;
    }

    public override string SubtitleBackgroundColor()
    {
        if (Title.ToLower().Contains(Night))
        {
            return FfMpegColor.DarkGreen;
        }

        return FfMpegColor.Green;
    }

    public override string SubtitleTextColor()
    {
        if (Title.ToLower().Contains(Night))
        {
            return FfMpegColor.GhostWhite;
        }

        return FfMpegColor.White;
    }

    public string GetDetailsFilePath()
    {
        return Path.Combine(WorkingDirectory, "details.txt");
    }

    public string GetNoMusicFilePath()
    {
        return Path.Combine(WorkingDirectory, "nomusic.txt");
    }

    public void AddDetailsContentToVideoFilter(string[] fileContents)
    {
        const int DISPLAY_DURATION = 5;

        string separator = Constant.SemiColon;
        if (fileContents[0].Contains(Constant.Pipe))
        {
            separator = Constant.Pipe;
        }

        for(int i = 0 ; i < fileContents.Count(); i++)
        {
            string[] splitLine = fileContents[i].Split(separator);
            int startSeconds = Int32.Parse(splitLine[0]);
            int endSeconds = startSeconds + DISPLAY_DURATION;
            string displayText = splitLine[1].Replace(":", "\\:");
            string displayTextLowered = displayText.ToLower();

            string textColor = SubtitleTextColor();
            string backgroundColor = SubtitleBackgroundColor();

            if (displayTextLowered.Contains("speed limit"))
            {
                textColor = FfMpegColor.Black;
                backgroundColor = FfMpegColor.White;
            }
            else if (displayTextLowered.Contains("national forest"))
            {
                textColor = FfMpegColor.White;
                backgroundColor = FfMpegColor.SaddleBrown;
            }

            AddDrawTextFilter(displayText, textColor, Constant.SolidText, FfmpegFontSize.Medium, 
                DrawTextPosition.LowerRight, backgroundColor, Constant.SolidBackground, 
                $"enable='between(t,{startSeconds},{endSeconds})'");
        }
    }

    public string GetStrippedTitle()
    {
        return (Title.Split(Constant.SemiColon))[0]
            .Replace(", Bad Drivers of Montgomery", string.Empty);
    }
}
