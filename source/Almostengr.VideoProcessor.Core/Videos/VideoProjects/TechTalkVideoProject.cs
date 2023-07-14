using System.Text;
using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Videos;

public sealed class TechTalkVideoProject : BaseVideoProject
{
    public TechTalkVideoSubType SubType { get; private set; }

    public TechTalkVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
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

    public override List<string> BrandingTextOptions()
    {
        List<string> options = new();
        options.Add("Tech Talk with RHT Services");
        options.Add(Constant.ROBINSON_SERVICES);
        options.Add(Constant.RHT_WEBSITE);
        options.Add("@rhtservicestech");
        options.Add("#rhtservicestech");
        return options;
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

    public override string ArchiveDirectory()
    {
        return Path.Combine(BaseDirectory, "archivetech");
    }

    public override string UploadDirectory()
    {
        return Path.Combine(BaseDirectory, "uploadtech");
    }

    public enum TechTalkVideoSubType
    {
        TechTalk,
        Christmas,
        IndependenceDay
    }
}