using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Toastmasters;

internal sealed record ToastmastersVideo : BaseVideo
{
    internal ToastmastersVideo(string baseDirectory, string archiveFileName) : 
        base(baseDirectory, archiveFileName)
    {
    }

    internal override string[] BrandingTextOptions()
    {
        return new string[] { "towertoastmasters.org", "Tower Toastmasters", "toastmasters.org" };
    }

    internal override string DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.SteelBlue;
    }

    internal override string DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }
}