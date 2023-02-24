using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed class DashCamVideoProjectArchive : BaseVideoProjectArchive
{
    internal DashCamVideoType SubType { get; init; }
    public DashCamVideoProjectArchive(string filePath) : base(filePath)
    {
        SubType = DashCamVideoType.Normal;
        string title = Title();

        if (title.Contains("night"))
        {
            SubType = DashCamVideoType.Night;
        }
        else if (title.Contains("firework"))
        {
            SubType = DashCamVideoType.Fireworks;
        }
        else if (title.Contains("altima") || title.Contains("sierra"))
        {
            SubType = DashCamVideoType.CarRepair;
        }

        if (filePath.ToLower().Contains("bad drivers of montgomery"))
        {
            throw new ArgumentException("Title contains invalid text", nameof(filePath));
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
        return (new DrawTextFilter(
            Title(),
            DrawTextFilterTextColor(),
            Opacity.Full,
            DrawTextFilterBackgroundColor(),
            Opacity.Medium,
            DrawTextPosition.UpperLeft)).ToString();
    }

    public override string[] BrandingTextOptions()
    {
        return new string[] { "Kenny Ram Dash Cam" };
    }

    internal enum DashCamVideoType
    {
        Normal,
        Night,
        Fireworks,
        CarRepair
    }
}