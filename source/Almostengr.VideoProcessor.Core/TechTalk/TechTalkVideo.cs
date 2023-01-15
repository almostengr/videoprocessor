using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.TechTalk.Exceptions;

namespace Almostengr.VideoProcessor.Core.TechTalk;

internal sealed record TechTalkVideo : BaseVideo
{
    internal TechTalkVideoSubType SubType { get; private set; }

    internal TechTalkVideo(string baseDirectory, string archiveFileName) : base(baseDirectory, archiveFileName)
    {
        SubType = TechTalkVideoSubType.TechTalk;
    }

    internal string ChristmasLightMetaFile()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Working, "christmas.txt");
    }

    internal string IndependenceDayMetaFile()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Working, "independence.txt");
    }

    internal void ConfirmChristmasLightVideo()
    {
        if (SubType == TechTalkVideoSubType.IndependenceDay)
        {
            throw new TechLightShowVideoTypeException();
        }

        SubType = TechTalkVideoSubType.Christmas;
    }

    internal void ConfirmIndependenceDayVideo()
    {
        if (SubType == TechTalkVideoSubType.Christmas)
        {
            throw new TechLightShowVideoTypeException();
        }

        SubType = TechTalkVideoSubType.IndependenceDay;
    }

    internal override string[] BrandingTextOptions()
    {
        const string TECH_TALK = "Tech Talk with RHT Services";

        List<string> options = new();

        if (SubType != TechTalkVideoSubType.TechTalk)
        {
            options.Add("twitter.com/hplightshow");
        }

        options.Add(TECH_TALK);
        options.Add(RHT_WEBSITE);

        return options.ToArray();
    }

    internal override string DrawTextFilterBackgroundColor()
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

    internal override string DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }
}


internal enum TechTalkVideoSubType
{
    TechTalk,
    Christmas,
    IndependenceDay
}