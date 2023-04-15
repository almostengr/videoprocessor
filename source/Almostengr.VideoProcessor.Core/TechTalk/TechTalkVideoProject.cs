using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed class TechTalkVideoProject : BaseVideoProject
{
    private TechTalkVideoSubType SubType { get; set; }

    public TechTalkVideoProject(string filePath) : base(filePath)
    {
        SubType = TechTalkVideoSubType.TechTalk;

        if (Title().Contains("christmas light show", StringComparison.OrdinalIgnoreCase))
        {
            SubType = TechTalkVideoSubType.Christmas;
        }
        else if (Title().Contains("4th of july", StringComparison.OrdinalIgnoreCase))
        {
            SubType = TechTalkVideoSubType.IndependenceDay;
        }
    }

    public override string BrandingText()
    {
        Random random = new();

        List<string> options = new();
        options.Add("Tech Talk with RHT Services");
        options.Add(Constant.RHT_WEBSITE);
        options.Add("@rhtservicestech");
        options.Add("#rhtservicestech");
        options.Add("#TechTuesday");
        options.Add("rhtservices.net/techtalk");

        return options[random.Next(0, options.Count)];
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