using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;
using Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Domain.DashCam;

public sealed record DashCamVideo : BaseVideo
{
    public string DetailsFileName { get; private set; }

    public DashCamVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
        DetailsFileName = string.Empty;
    }

    public override string BannerBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    private bool IsNightVideo()
    {
        string title = Title.ToLower();
        return (title.Contains("night") || title.Contains("firework"));
    }

    public override string BannerTextColor()
    {
        if (IsNightVideo())
        {
            return FfMpegColor.Orange;
        }

        return FfMpegColor.White;
    }

    public override string SubtitleBackgroundColor()
    {
        if (IsNightVideo())
        {
            return FfMpegColor.DarkGreen;
        }

        return FfMpegColor.Green;
    }

    public override string SubtitleTextColor()
    {
        if (IsNightVideo())
        {
            return FfMpegColor.GhostWhite;
        }

        return FfMpegColor.White;
    }

    public void SetDetailsFileName(string? filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            DetailsFileName = string.Empty;
            return;
        }

        DetailsFileName = filename;
    }

    public string GetDetailsFileName()
    {
        return DetailsFileName;
    }

    public string DetailsFileSuffix()
    {
        return "details.txt";
    }

    public string DetailsFilePath()
    {
        return Path.Combine(WorkingDirectory, GetDetailsFileName());
    }

    public string NoMusicFilePath()
    {
        return Path.Combine(WorkingDirectory, "nomusic.txt");
    }

    public void AddDetailsContentToVideoFilter(string[] fileContents)
    {
        string separator = Constant.SemiColon;
        if (fileContents[0].Contains(Constant.Pipe))
        {
            separator = Constant.Pipe;
        }

        const int DISPLAY_DURATION = 5;
        for (int i = 0; i < fileContents.Count(); i++)
        {
            string[] splitLine = fileContents[i].Split(separator);
            string[] timesplit = splitLine[0].Split(Constant.Colon);

            TimeSpan videoTime;
            switch (timesplit.Count())
            {
                case 3:
                    videoTime = new TimeSpan(
                    Int32.Parse(timesplit[0]),
                    Int32.Parse(timesplit[1]),
                    Int32.Parse(timesplit[2]));
                    break;

                case 2:
                    videoTime = new TimeSpan(
                        0,
                        Int32.Parse(timesplit[0]),
                        Int32.Parse(timesplit[1]));
                    break;

                case 1:
                    videoTime = new TimeSpan(0, 0, Int32.Parse(timesplit[0]));
                    break;

                default:
                    throw new VideoProcessorException($"Error with parsing time in details file. Count {timesplit.Count()}");
            }

            string displayText = splitLine[1].ToUpper()
                .Replace(":", "\\:")
                .Replace("'", string.Empty);

            string textColor = SubtitleTextColor();
            string backgroundColor = SubtitleBackgroundColor();
            string backgroundOpacity = Opacity.Full;

            const string INFO = "INFO";
            const string VIDEO_EDITOR = "VIDEO EDITOR";
            const string SOFTWARE = "SOFTWARE";
            if (displayText.Contains("SPEED LIMIT"))
            {
                backgroundColor = FfMpegColor.White;
                textColor = FfMpegColor.Black;
            }
            else if (displayText.Contains("NATIONAL FOREST") || displayText.Contains("RECREATION"))
            {
                backgroundColor = FfMpegColor.SaddleBrown;
                textColor = FfMpegColor.White;
                displayText = displayText.Replace("RECREATION", string.Empty);
            }
            else if (displayText.StartsWith(INFO) || displayText.StartsWith("DISTANCE") ||
                displayText.StartsWith("VEHICLE") || displayText.StartsWith("CAMERA") ||
                displayText.StartsWith(SOFTWARE) || displayText.StartsWith(VIDEO_EDITOR))
            {
                backgroundColor = FfMpegColor.Black;
                textColor = FfMpegColor.White;
                backgroundOpacity = Opacity.Light;
                displayText = displayText.Replace(INFO, string.Empty)
                    .Replace(SOFTWARE, VIDEO_EDITOR);
            }
            else if (displayText.Contains("SUBSCRIBE"))
            {
                backgroundColor = FfMpegColor.Red;
                textColor = FfMpegColor.White;
                backgroundOpacity = Opacity.Full;
            }

            const int MAX_LINE_LENGTH = 55;
            if (displayText.Length > MAX_LINE_LENGTH)
            {
                throw new VideoFilterTextIsTooLongException();
            }

            int startSeconds = ((int)videoTime.TotalSeconds);
            int endSeconds = startSeconds + DISPLAY_DURATION;
            AddDrawTextFilter(displayText.Trim(), textColor, Opacity.Full, FfmpegFontSize.XLarge,
                DrawTextPosition.LowerRight, backgroundColor, backgroundOpacity, Constant.BorderBoxWidthLarge,
                $"enable='between(t,{startSeconds},{endSeconds})'");
        }
    }

    internal string IncomingDetailsFilePath()
    {
        return Path.Combine(IncomingDirectory, Title + ".details.txt");
    }

    public string StrippedTitle()
    {
        return (Title.Split(Constant.SemiColon))[0]
            .Replace(", BAD DRIVERS OF MONTGOMERY", string.Empty);
    }
}
