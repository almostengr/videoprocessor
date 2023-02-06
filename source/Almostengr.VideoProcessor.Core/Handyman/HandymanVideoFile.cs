using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed record HandymanVideoFile : BaseVideoFile
{
    public HandymanVideoFile(string filePath) : base(filePath)
    { }

    public override string[] BrandingTextOptions()
    {
        List<string> options = new();
        options.Add(ROBINSON_SERVICES);
        options.Add(RHT_WEBSITE);
        options.Add("@rhtservicesllc");
        options.Add("#rhtservicesllc");
        options.Add("rhtservices.net/handyman");
        return options.ToArray();
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public override FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.RhtYellow;
    }
}