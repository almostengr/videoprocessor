using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Toastmasters;

public sealed record ToastmastersVideoFile : BaseVideoFile
{
    public ToastmastersVideoFile(string baseDirectory, string archiveFileName) : 
        base(baseDirectory, archiveFileName)
    {
    }

    public override string[] BrandingTextOptions()
    {
        return new string[] { "towertoastmasters.org", "Tower Toastmasters", "toastmasters.org" };
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.SteelBlue;
    }

    public override FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }
}