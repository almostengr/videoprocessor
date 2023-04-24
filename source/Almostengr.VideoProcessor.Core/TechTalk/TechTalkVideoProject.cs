using System.Text;
using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed class TechTalkVideoProject : BaseVideoProject
{
    public TechTalkVideoSubType SubType { get; private set; }

    public TechTalkVideoProject(string filePath) : base(filePath)
    {
        SubType = TechTalkVideoSubType.TechTalk;

        if (Title().ContainsIgnoringCase("christmas light show"))
        {
            SubType = TechTalkVideoSubType.Christmas;
        }
        else if (Title().ContainsIgnoringCase("4th of july"))
        {
            SubType = TechTalkVideoSubType.IndependenceDay;
        }
    }

    public override IEnumerable<string> BrandingTextOptions()
    {
        return new string[] {
            "Tech Talk with RHT Services",
            Constant.RHT_WEBSITE,
            "@rhtservicestech",
            "#rhtservicestech",
            "rhtservices.net/techtalk",
        };
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        switch (SubType)
        {
            case TechTalkVideoSubType.Christmas:
                return FfMpegColor.Maroon;

            case TechTalkVideoSubType.IndependenceDay:
                return FfMpegColor.Blue;

            default:
                return FfMpegColor.Green;
        }
    }

    internal string VideoGraphics(string brandingText)
    {
        StringBuilder stringBuilder = new(base.ChannelBrandDrawTextFilter(brandingText));

        switch (SubType)
        {
            case TechTalkVideoSubType.Christmas:
            case TechTalkVideoSubType.IndependenceDay:
                stringBuilder.Append(Constant.CommaSpace);
                stringBuilder.Append(new DrawTextFilter(Title(), DrawTextFilterTextColor(), Opacity.Full,
                        DrawTextFilterBackgroundColor(), Opacity.Medium, DrawTextPosition.LowerLeft).ToString());
                break;
        }

        return stringBuilder.ToString();
    }

    public enum TechTalkVideoSubType
    {
        TechTalk,
        Christmas,
        IndependenceDay
    }
}