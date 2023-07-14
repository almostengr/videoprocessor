using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Videos;

public sealed class ToastmastersVideoProject : BaseVideoProject
{
    public ToastmastersVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }

    public override List<string> BrandingTextOptions()
    {
        List<string> options = new();
        options.Add("towertoastmasters.org");
        options.Add("Tower Toastmasters");
        options.Add("toastmasters.org");
        options.Add("facebook.com/TowerToastmasters");
        return options;
    }

    public override string ArchiveDirectory()
    {
        return Path.Combine(BaseDirectory, "archivetoastmasters");
    }

    public override string UploadDirectory()
    {
        return Path.Combine(BaseDirectory, "uploadtoastmasters");
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.SteelBlue;
    }
}