using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Videos;

public sealed class HandymanVideoProject : BaseVideoProject
{
    public HandymanVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }

    public override List<string> BrandingTextOptions()
    {
        List<string> options = new();
        options.Add(Constant.ROBINSON_SERVICES);
        options.Add(Constant.RHT_WEBSITE);
        options.Add("@rhtservicesllc");
        options.Add("#rhtservicesllc");
        return options;
    }

    public override string UploadDirectory()
    {
        return Path.Combine(BaseDirectory, Constant.HandymanUploadDirectory);
    }

    public override string ArchiveDirectory()
    {
        return Path.Combine(BaseDirectory, Constant.HandymanArchiveDirectory);
    }

    public override string ArchiveFilePath()
    {
        return Path.Combine(ArchiveDirectory(), FileName());
    }

    public override string UploadFilePath()
    {
        return Path.Combine(UploadDirectory(), OutputFileName());
    }
    
    public override FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.RhtYellow;
    }
}