using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Videos.Toastmasters;

public sealed record ToastmastersVideo : BaseVideo
{
    public ToastmastersVideo(string baseDirectory, string archiveFileName) : base(baseDirectory, archiveFileName)
    {
    }

    public override string[] BrandingTextOptions()
    {
        return new string[] { "towertoastmasters.org", "Tower Toastmasters", "toastmasters.org" };
    }

    public override string DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.SteelBlue;
    }

    public override string DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }
}