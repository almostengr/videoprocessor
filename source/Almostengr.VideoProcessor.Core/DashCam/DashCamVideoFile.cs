using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed class DashCamVideoFile : VideoFile
{
    internal DashCamVideoType SubType { get; private set; }

    public DashCamVideoFile(string filePath) : base(filePath)
    {
        // string title = Title();
        // SetSubtype(title);
        ValidateArguments(filePath);
        SetSubtype(FileName());

        // if (filePath.ToLower().Contains("bad drivers of montgomery"))
        // {
        //     throw new ArgumentException("Title contains invalid text", nameof(filePath));
        // }
    }

    public DashCamVideoFile(VideoProjectArchiveFile videoProjectArchiveFile) : base(videoProjectArchiveFile)
    {
        ValidateArguments(videoProjectArchiveFile.FilePath);
        SetSubtype(FileName());
    }

    private void SetSubtype(string title)
    {
        SubType = DashCamVideoType.Normal;

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
    }

    private void ValidateArguments(string filePath)
    {
        if (filePath.ToLower().Contains("bad drivers of montgomery"))
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

    // public override string[] BrandingTextOptions()
    // {
    //     return new string[] { "Kenny Ram Dash Cam" };
    // }

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