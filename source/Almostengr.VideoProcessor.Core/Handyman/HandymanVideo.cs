using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Handyman;

internal sealed record HandymanVideo : BaseVideo
{
    internal HandymanVideo(string baseDirectory, string archiveFileName) : base(baseDirectory, archiveFileName)
    {
    }

    internal override string[] BrandingTextOptions()
    {
        return new string[] { ROBINSON_SERVICES, RHT_WEBSITE, "@rhtservicesllc", RHT_SOCIALS };
    }

    internal override string DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.RhtYellow;
    }

    internal override string DrawTextFilterTextColor()
    {
        return FfMpegColor.Black;
    }
}