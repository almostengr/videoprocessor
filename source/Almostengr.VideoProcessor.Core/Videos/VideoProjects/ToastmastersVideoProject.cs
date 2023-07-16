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
    
    public override string UploadDirectory()
    {
        return Path.Combine(BaseDirectory, Constant.ToastmastersUploadDirectory);
    }

    public override string ArchiveDirectory()
    {
        return Path.Combine(BaseDirectory, Constant.ToastmastersArchiveDirectory);
    }

    public override string ArchiveFilePath()
    {
        return Path.Combine(ArchiveDirectory(), FileName());
    }

    public override string UploadFilePath()
    {
        return Path.Combine(UploadDirectory(), OutputFileName());
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.SteelBlue;
    }
}