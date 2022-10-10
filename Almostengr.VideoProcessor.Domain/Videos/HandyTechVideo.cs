using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos;

public sealed record HandyTechVideo : VideoBase
{
    private const string _xResolution = "1920";
    private const string _yResolution = "1080";
    private const string _audioBitRate = "196000";
    private const string _audioSampleRate = "48000";
    private const string NO_INTRO_FILE = "nointro.txt";
    private const string SHOW_INTRO_FILENAME_MP4 = "rhtservicesintro.mp4";

    public HandyTechVideo()
    {
        BaseDirectory = "/home/almostengineer";
    }

    // public HandyTechVideo(string tarballFilePath) : base(tarballFilePath)
    // {
    // }

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