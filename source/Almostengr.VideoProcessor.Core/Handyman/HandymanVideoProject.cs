using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed class HandymanVideoProject : BaseVideoProject
{
    public HandymanVideoProject(string filePath) : base(filePath)
    {
    }

    public override string BrandingText()
    {
        Random random = new();
        List<string> options = new();

        options.Add(Constant.ROBINSON_SERVICES);
        options.Add(Constant.RHT_WEBSITE);
        options.Add("@rhtservicesllc");
        options.Add("#rhtservicesllc");
        options.Add("rhtservices.net/handyman");

        return options[random.Next(0, options.Count)];
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