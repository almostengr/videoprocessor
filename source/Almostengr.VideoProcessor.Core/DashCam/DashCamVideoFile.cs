using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed record DashCamVideoFile : BaseVideoFile
{
    public DashCamVideoType SubType { get; private set; }

    public DashCamVideoFile(string archiveFilePath) : base(archiveFilePath)
    {
        SubType = DashCamVideoType.Normal;

        if (Title.ToLower().Contains("night"))
        {
            SubType = DashCamVideoType.Night;
        }
        else if (Title.ToLower().Contains("firework"))
        {
            SubType = DashCamVideoType.Fireworks;
        }

    }

    public override string[] BrandingTextOptions()
    {
        return new string[] { "Kenny Ram Dash Cam" };
    }

    public override string SetTitle(string fileName)
    {
        if (fileName.ToLower().Contains("bad drivers of montgomery"))
        {
            throw new ArgumentException("Title contains invalid text", nameof(fileName));
        }

        return base.SetTitle(fileName.Split(Constant.SemiColon).First());
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        // if (SubType == DashCamVideoType.Fireworks)
        // {
        //     return FfMpegColor.Blue;
        // }

        // return FfMpegColor.Black;

        switch(SubType)
        {
            case DashCamVideoType.Fireworks:
                return FfMpegColor.Blue;

            default:
                return FfMpegColor.Black;
        }
    }

    public override FfMpegColor DrawTextFilterTextColor()
    {
        // if (SubType == DashCamVideoType.Night)
        // {
        //     return FfMpegColor.Orange;
        // }

        // return FfMpegColor.White;

        switch(SubType)
        {
            case DashCamVideoType.Night:
                return FfMpegColor.Orange;

            default: 
                return FfMpegColor.White;
        }
    }
}


public enum DashCamVideoType
{
    Normal,
    Night,
    Fireworks
}