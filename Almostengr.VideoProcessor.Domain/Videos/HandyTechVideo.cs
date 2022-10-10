using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos;

public sealed record HandyTechVideo : VideoBase
{
    public readonly string xResolution = "1920";
    public readonly string yResolution = "1080";
    public readonly string audioBitRate = "196000";
    public readonly string audioSampleRate = "48000";
    public readonly string NO_INTRO_FILE = "nointro.txt";
    public readonly string SHOW_INTRO_FILENAME_MP4 = "rhtservicesintro.mp4";

    public HandyTechVideo()
    {
        BaseDirectory = Constants.HandyTechBaseDirectory;
    }

    public override string ChannelBannerText()
    {
        Random random = new Random();
        string[] bannerText = {
                    "rhtservices.net",
                    "Robinson Handy and Technology Services",
                    "rhtservices.net/facebook",
                    "rhtservices.net/instagram",
                    "rhtservices.net/youtube",
                    "@rhtservicesllc"
                    };

        return bannerText.ElementAt(random.Next(0, bannerText.Length));
    }

    public override string TextColor()
    {
        string title = Title.ToLower();
        if (title.Contains("christmas"))
        {
            return FfMpegColors.Green;
        }

        return FfMpegColors.White;
    }

    public override string BoxColor()
    {
        return FfMpegColors.Black;
    }

    public string NoIntroFilePath()
    {
        return Path.Combine(WorkingDirectory, NO_INTRO_FILE);
    }

    public string ShowIntroFilePath()
    {
        return Path.Combine(WorkingDirectory, SHOW_INTRO_FILENAME_MP4);
    }
}