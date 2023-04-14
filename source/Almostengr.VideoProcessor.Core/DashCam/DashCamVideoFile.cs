using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed class DashCamVideoFile : VideoFile
{
    internal DashCamVideoType SubType { get; private set; }

    public DashCamVideoFile(string filePath) : base(filePath)
    {
        ValidateArguments(filePath);
        SetSubtype(FileName());
    }

    public DashCamVideoFile(VideoProjectArchiveFile videoProjectArchiveFile) : base(videoProjectArchiveFile)
    {
        ValidateArguments(videoProjectArchiveFile.FilePath);
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
        else if (title.Contains("nissan altima", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("gmc sierra", StringComparison.OrdinalIgnoreCase))
        {
            SubType = DashCamVideoType.CarRepair;
        }
    }

    private void ValidateArguments(string filePath)
    {
        if (filePath.Contains("bad drivers of montgomery", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Title contains invalid text");
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

    public override string BrandingText()
    {
        return "Kenny Ram Dash Cam";
    }

    internal enum DashCamVideoType
    {
        Normal,
        Night,
        Fireworks,
        CarRepair
    }
}