using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;

namespace Almostengr.VideoProcessor.Domain.DashCam;

public sealed record DashCamVideo : BaseVideo
{
    private readonly string Night = "night";
    private readonly string DetailsFileName = "details.txt";

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

    public string GetDetailsFilePath()
    {
        return Path.Combine(WorkingDirectory, DetailsFileName);
    }

    public void AddDetailsContentToVideoFilter(string[] fileContents)
    {
        const int DISPLAY_DURATION = 5;
        const string SPEED_LIMIT = "speed limit";

        for(int i = 0 ; i < fileContents.Count(); i++)
        {
            string[] splitLine = fileContents[i].Split(Constant.Pipe);
            int startSeconds = Int32.Parse(splitLine[0]);
            int endSeconds = startSeconds + DISPLAY_DURATION;
            string displayText = splitLine[1].Replace(":", "\\:");

            string textColor = SubtitleTextColor();
            string backgroundColor = SubtitleBackgroundColor();

            if (displayText.ToLower().Contains(SPEED_LIMIT))
            {
                textColor = FfMpegColors.Black;
                backgroundColor = FfMpegColors.White;
            }

            AddDrawTextFilter(displayText, textColor, Constant.SolidText, FfmpegFontSize.Small, 
                DrawTextPosition.LowerRight, backgroundColor, Constant.SolidBackground, 
                $"enable='between(t,{startSeconds},{endSeconds})'");

            // todo - dim text after being displayed
            // string nextSeconds = (fileContents[i+1].Split(Constant.Pipe))[0];
            // AddDrawTextFilter(displayText, textColor, Constant.DimText, FfmpegFontSize.Small, 
            //     DrawTextPosition.LowerRight, backgroundColor, Constant.DimBackground, 
            //     $"enable='between(t,{endSeconds},{nextSeconds})'");
        }
    }
}
