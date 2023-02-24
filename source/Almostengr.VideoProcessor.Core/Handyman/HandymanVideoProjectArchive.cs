using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed class HandymanVideoProjectArchive : BaseVideoProjectArchive
{
    public HandymanVideoProjectArchive(string filePath) : base(filePath)
    {
    }

    public override string[] BrandingTextOptions()
    {
        List<string> options = new();
        options.Add(Constant.ROBINSON_SERVICES);
        options.Add(Constant.RHT_WEBSITE);
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
