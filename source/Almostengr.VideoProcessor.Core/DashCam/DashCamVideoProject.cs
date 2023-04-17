using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed partial class DashCamVideoProject : BaseVideoProject
{
    internal DashCamVideoType SubType { get; private set; }

    public DashCamVideoProject(string filePath) : base(filePath)
    {

        if (filePath.Contains("bad drivers of montgomery", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Title contains invalid text");
        }

        SetSubtype(FileName());
    }

    private void SetSubtype(string title)
    {
        SubType = DashCamVideoType.Normal;

        if (title.Contains("night", StringComparison.OrdinalIgnoreCase))
        {
            SubType = DashCamVideoType.Night;
        }
        else if (title.Contains("firework", StringComparison.OrdinalIgnoreCase))
        {
            SubType = DashCamVideoType.Fireworks;
        }
        else if (title.Contains(Constant.NissanAltima, StringComparison.OrdinalIgnoreCase) ||
            title.Contains(Constant.GmcSierra, StringComparison.OrdinalIgnoreCase))
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

    public string TitleDrawTextFilter()
    {
        switch (SubType)
        {
            case DashCamVideoType.CarRepair:
                return string.Empty;

            default:
                return (new DrawTextFilter(
                    Title(),
                    DrawTextFilterTextColor(),
                    Opacity.Full,
                    DrawTextFilterBackgroundColor(),
                    Opacity.Medium,
                    DrawTextPosition.UpperLeft)).ToString();
        }
    }

    public override string BrandingText()
    {
        return "Kenny Ram Dash Cam";
    }
}