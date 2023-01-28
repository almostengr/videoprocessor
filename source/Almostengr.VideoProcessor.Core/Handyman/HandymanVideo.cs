using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed record HandymanVideo : BaseVideo
{
    public HandymanVideo(string baseDirectory, string archiveFileName) : base(baseDirectory, archiveFileName)
    {
    }

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