using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.TechTalk.Exceptions;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed record TechTalkVideoFile : BaseVideoFile
{
    public TechTalkVideoSubType SubType { get; private set; }

    // public TechTalkVideoFile(string baseDirectory, string archiveFileName) : base(baseDirectory, archiveFileName)
    // {
    //     SubType = TechTalkVideoSubType.TechTalk;
    // }
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

    // public string ChristmasLightFileName()
    // {
    //     return "christmas.txt";
    // }

    // public string IndependenceDayFileName()
    // {
    //     return "independence.txt";
    // }

    // public string ChristmasLightMetaFile()
    // {
    //     return Path.Combine(BaseDirectory, DirectoryName.Working, "christmas.txt");
    // }

    // public string IndependenceDayMetaFile()
    // {
    //     return Path.Combine(BaseDirectory, DirectoryName.Working, "independence.txt");
    // }

    // public void ConfirmChristmasLightVideo()
    // {
    //     if (SubType == TechTalkVideoSubType.IndependenceDay)
    //     {
    //         throw new TechLightShowVideoTypeException();
    //     }

    //     SubType = TechTalkVideoSubType.Christmas;
    // }

    // public void ConfirmIndependenceDayVideo()
    // {
    //     if (SubType == TechTalkVideoSubType.Christmas)
    //     {
    //         throw new TechLightShowVideoTypeException();
    //     }

    //     SubType = TechTalkVideoSubType.IndependenceDay;
    // }

    public override string[] BrandingTextOptions()
    {
        const string TECH_TALK = "Tech Talk with RHT Services";
        List<string> options = new();

        if (SubType == TechTalkVideoSubType.Christmas || SubType == TechTalkVideoSubType.IndependenceDay)
        {
            options.Add("twitter.com/hplightshow");
        }

        options.Add(TECH_TALK);
        options.Add(RHT_WEBSITE);
        options.Add(RHT_SOCIAL_LINKS);

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

    public override FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }
}
