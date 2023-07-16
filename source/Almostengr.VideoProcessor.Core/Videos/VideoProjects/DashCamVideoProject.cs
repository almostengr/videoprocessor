using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Videos;

public abstract class DashCamVideoProject : BaseVideoProject
{
    public DashCamVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
        if (filePath.ContainsIgnoringCase("bad drivers of montgomery"))
        {
            throw new ArgumentException("Title contains invalid text");
        }
    }

    public override string UploadDirectory()
    {
        return Path.Combine(BaseDirectory, Constant.DashCamUploadDirectory);
    }

    public override string ArchiveDirectory()
    {
        return Path.Combine(BaseDirectory, Constant.DashCamArchiveDirectory);
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
        return FfMpegColor.Black;
    }

    public override List<string> BrandingTextOptions()
    {
        List<string> options = new();
        options.Add("Kenny Ram Dash Cam");
        options.Add("#KennyRamDashCam");
        return options;
    }
}


public class DayTimeDashCamVideoProject : DashCamVideoProject, IDayTimeDashCamVideoProject
{
    public DayTimeDashCamVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Green;
    }

    public string UploadDescriptionTextFile()
    {
        return Path.Combine(UploadDirectory(), FileName() + "description.txt");
    }

}

public sealed class NightTimeDashCamVideoProject : DayTimeDashCamVideoProject
{
    public NightTimeDashCamVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }

    public override FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.Orange;
    }

}

public sealed class FireworksDashCamVideoProject : DashCamVideoProject
{
    public FireworksDashCamVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Blue;
    }
}

public sealed class CarRepairDashCamVideoProject : DashCamVideoProject
{
    public CarRepairDashCamVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }
}

public sealed class ArmchairDashCamVideoProject : DashCamVideoProject
{
    public ArmchairDashCamVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }
}
