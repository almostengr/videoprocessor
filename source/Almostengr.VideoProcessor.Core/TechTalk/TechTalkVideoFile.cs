using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed class TechTalkVideoFile : BaseVideoFile
{
    private TechTalkVideoSubType SubType { get; init; }

    public TechTalkVideoFile(string archiveFilePath) : base(archiveFilePath)
    {
        SubType = TechTalkVideoSubType.TechTalk;

        if (Title.ToLower().Contains("christmas light show"))
        {
            SubType = TechTalkVideoSubType.Christmas;
        }
        else if (Title.ToLower().Contains("4th of july"))
        {
            SubType = TechTalkVideoSubType.IndependenceDay;
        }
    }

    public override string[] BrandingTextOptions()
    {
        List<string> options = new();
        options.Add("Tech Talk with RHT Services");
        options.Add(RHT_WEBSITE);
        options.Add("@rhtservicestech");
        options.Add("#rhtservicestech");
        options.Add("rhtservices.net/techtalk");
        return options.ToArray();
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

    enum TechTalkVideoSubType
    {
        TechTalk,
        Christmas,
        IndependenceDay
    }
}
