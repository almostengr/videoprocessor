using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed record HandymanVideoFile : BaseVideoFile
{
    public HandymanVideoFile(string archiveFilePath) : base(archiveFilePath)
    {}

    public override string[] BrandingTextOptions()
    {
        return new string[] { ROBINSON_SERVICES, RHT_WEBSITE, "@rhtservicesllc", RHT_SOCIAL_LINKS };
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.RhtYellow;
    }

    public override FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.Black;
    }
}