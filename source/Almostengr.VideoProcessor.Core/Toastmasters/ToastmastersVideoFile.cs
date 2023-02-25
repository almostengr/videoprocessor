using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Toastmasters;

public sealed class ToastmastersVideoFile : VideoFile
{
    public ToastmastersVideoFile(VideoProjectArchiveFile videoProjectArchiveFile) : base(videoProjectArchiveFile)
    {
    }

    public override string BrandingText()
    {
        Random random = new();
        List<string> options = new();
        options.Add("towertoastmasters.org");
        options.Add("Tower Toastmasters");
        options.Add("toastmasters.org");

        return options[random.Next(0, options.Count)];
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.SteelBlue;
    }

    public string MeetingFilter()
    {
        return (new DrawTextFilter("Join us on Tuesdays at 12 Noon", FfMpegColor.Blue, Opacity.Full,
            FfMpegColor.White, Opacity.Full, DrawTextPosition.LowerLeft,
            new TimeSpan(0, 4, 5), new TimeSpan(0, 4, 25))).ToString();
    }
}