using System.Text;
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

        if (title.ContainsIgnoringCase("night"))
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

    internal string ChannelBrandAndTitleGraphics(string brandingText)
    {
        StringBuilder stringBuilder = new(base.ChannelBrandDrawTextFilter(brandingText));
        
        switch (SubType)
        {
            case DashCamVideoType.CarRepair:
                break;

            default:
                stringBuilder.Append(
                    new DrawTextFilter(Title(), DrawTextFilterTextColor(), Opacity.Full,
                    DrawTextFilterBackgroundColor(), Opacity.Medium, DrawTextPosition.UpperLeft).ToString());
                break;
        }
        return stringBuilder.ToString();
    }

    public override IEnumerable<string> BrandingTextOptions()
    {
        return new string[] { "Kenny Ram Dash Cam" };
    }
}