using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Toastmasters;

public sealed record ToastmastersVideoFile : BaseVideoFile
{
    public ToastmastersVideoFile(string archiveFilePath) : base(archiveFilePath)
    { }

    public override string[] BrandingTextOptions()
    {
        return new string[] { "towertoastmasters.org", "Tower Toastmasters", "toastmasters.org" };
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.SteelBlue;
    }

    // public void AddMeetingFilter(int duration)
    // {
    //     AddDrawTextVideoFilter(
    //         "Join us on Tuesdays at 12 Noon!",
    //         FfMpegColor.Black,
    //         Opacity.Full,
    //         FfmpegFontSize.Large,
    //         DrawTextPosition.LowerLeft,
    //         FfMpegColor.White,
    //         Opacity.Full,
    //         10,
    //         base.FilterDuration(duration)
    //     );
    // }

    public string MeetingFilter()
    {
        return (new DrawTextFilter("Join us on Tuesdays at 12 Noon", FfMpegColor.Blue, Opacity.Full,
            FfMpegColor.White, Opacity.Full, DrawTextPosition.LowerLeft,
            new TimeSpan(0, 4, 5), new TimeSpan(0, 4, 25))).ToString();
    }
}