using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Toastmasters;

public sealed class ToastmastersVideoProject : BaseVideoProject
{
    public ToastmastersVideoProject(string filePath) : base(filePath)
    {
    }

    public override IEnumerable<string> BrandingTextOptions()
    {
        return new string[] {
            "towertoastmasters.org",
            "Tower Toastmasters",
            "toastmasters.org",
            "facebook.com/TowerToastmasters",
        };
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.SteelBlue;
    }
}