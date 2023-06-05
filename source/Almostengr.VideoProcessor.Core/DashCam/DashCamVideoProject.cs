using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed partial class DashCamVideoProject : BaseVideoProject
{
    internal DashCamVideoType SubType { get; private set; }

    public DashCamVideoProject(string filePath) : base(filePath)
    {

        if (filePath.ContainsIgnoringCase("bad drivers of montgomery"))
        {
            throw new ArgumentException("Title contains invalid text");
        }

        SetSubtype(FileName());
    }

    private void SetSubtype(string title)
    {
        SubType = DashCamVideoType.Normal;

        if (title.ContainsIgnoringCase("night") || title.ContainsIgnoringCase("sunset"))
        {
            SubType = DashCamVideoType.Night;
        }
        else if (title.ContainsIgnoringCase("firework"))
        {
            SubType = DashCamVideoType.Fireworks;
        }
        else if (title.ContainsIgnoringCase(Constant.NissanAltima) || title.ContainsIgnoringCase(Constant.GmcSierra))
        {
            SubType = DashCamVideoType.CarRepair;
        }
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        switch (SubType)
        {
            case DashCamVideoType.Fireworks:
                return FfMpegColor.Blue;

            default:
                return FfMpegColor.Black;
        }
    }

    public override FfMpegColor DrawTextFilterTextColor()
    {
        switch (SubType)
        {
            case DashCamVideoType.Night:
                return FfMpegColor.Orange;

            default:
                return FfMpegColor.White;
        }
    }

    public override IEnumerable<string> BrandingTextOptions()
    {
        return new string[] { "Kenny Ram Dash Cam", "#KennyRamDashCam" };
    }
}