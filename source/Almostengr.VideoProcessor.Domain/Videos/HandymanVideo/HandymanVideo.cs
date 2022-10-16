namespace Almostengr.VideoProcessor.Domain.Videos.HandymanVideo;

internal sealed record HandymanVideo : BaseVideo
{
    // public readonly string xResolution = "1920";
    // public readonly string yResolution = "1080";
    // public readonly string audioBitRate = "196000";
    // public readonly string audioSampleRate = "48000";
    // public readonly string NO_INTRO_FILE = "nointro.txt";
    // public readonly string SHOW_INTRO_FILENAME_MP4 = "rhtservicesintro.mp4";
    public readonly string ShowIntroFilePath = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ytvideostructure/rhtservicesintro.mp4";

    public HandymanVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public override string ChannelBannerText()
    {
        // Random random = new Random();
        // string[] bannerText = {
        //             "rhtservices.net",
        //             "Robinson Handy and Technology Services",
        //             "rhtservices.net/facebook",
        //             "rhtservices.net/instagram",
        //             "rhtservices.net/youtube",
        //             "@rhtservicesllc"
        //             };

        // return bannerText.ElementAt(random.Next(0, bannerText.Length));
        return ChannelBannerTextHandymanTechnology();
    }

    public override string TextColor()
    {
        return FfMpegColors.RhtYellow;
    }

    public override string BoxColor()
    {
        return FfMpegColors.Black;
    }

    // public string NoIntroFilePath()
    // {
    //     return Path.Combine(WorkingDirectory, NO_INTRO_FILE);
    // }
}