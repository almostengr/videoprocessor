using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Toastmasters;

public sealed class ToastmastersVideoProject : BaseVideoProject
{
    public ToastmastersVideoProject(string filePath) : base(filePath)
    {
    }

    public override string BrandingText()
    {
        Random random = new();
        List<string> options = new();
        options.Add("towertoastmasters.org");
        options.Add("Tower Toastmasters");
        options.Add("toastmasters.org");
        options.Add("facebook.com/TowerToastmasters");

        return options[random.Next(0, options.Count)];
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.SteelBlue;
    }

}