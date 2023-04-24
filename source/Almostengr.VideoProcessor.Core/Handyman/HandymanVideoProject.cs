using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed class HandymanVideoProject : BaseVideoProject
{
    public HandymanVideoProject(string filePath) : base(filePath)
    {
    }

    public override IEnumerable<string> BrandingTextOptions()
    {
        return new string[] {
            Constant.ROBINSON_SERVICES,
            Constant.RHT_WEBSITE,
            "@rhtservicesllc",
            "#rhtservicesllc",
            "rhtservices.net/handyman",
        };
    }

    public override FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.RhtYellow;
    }
}