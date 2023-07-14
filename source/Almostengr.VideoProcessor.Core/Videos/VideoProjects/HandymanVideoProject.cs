using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Videos;

public sealed class HandymanVideoProject : BaseVideoProject
{
    public HandymanVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }

    public override List<string> BrandingTextOptions()
    {
    //     return new string[] {
    //         Constant.ROBINSON_SERVICES);
    //         Constant.RHT_WEBSITE);
    //         "@rhtservicesllc");
    //         "#rhtservicesllc");
    //     };

        List<string> options = new();
        options.Add(Constant.ROBINSON_SERVICES);
        options.Add(Constant.RHT_WEBSITE);
        options.Add("@rhtservicesllc");
        options.Add("#rhtservicesllc");
        return options;
    }

    public override string ArchiveDirectory()
    {
        return Path.Combine(BaseDirectory, "archivehandyman");
    }

    public override string UploadDirectory()
    {
        return Path.Combine(BaseDirectory, "uploadhandyman");
    }
    
    public override FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.RhtYellow;
    }
}