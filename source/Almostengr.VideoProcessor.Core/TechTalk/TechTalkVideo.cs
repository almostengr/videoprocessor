using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.TechTalk.Exceptions;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed record TechTalkVideo : BaseVideo
{
    public bool IsChristmasLightVideo { get; private set; }
    public bool IsIndependenceDayVideo { get; private set; }

    public TechTalkVideo(string baseDirectory, string archiveFileName) : base(baseDirectory, archiveFileName)
    {
        IsChristmasLightVideo = false;
        IsIndependenceDayVideo = false;
    }

    public string ChristmasLightMetaFile()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Working, "christmas.txt");
    }

    public string IndependenceDayMetaFile()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Working, "independence.txt");
    }

    public void ConfirmChristmasLightVideo()
    {
        if (IsIndependenceDayVideo)
        {
            throw new ChristmasAndIndependenceVideoException();
        }

        IsChristmasLightVideo = true;
    }

    public void ConfirmIndependenceDayVideo()
    {
        if (IsChristmasLightVideo)
        {
            throw new ChristmasAndIndependenceVideoException();
        }

        IsIndependenceDayVideo = true;
    }

    public override string[] BrandingTextOptions()
    {
        const string TECH_TALK = "Tech Talk with RHT Services";

        if (IsChristmasLightVideo || IsIndependenceDayVideo)
        {
            return new string[] { RHT_WEBSITE, TECH_TALK, "twitter.com/hplightshow" };
        }

        return new string[] { RHT_WEBSITE, TECH_TALK };
    }

    public override string DrawTextFilterBackgroundColor()
    {
        if (IsChristmasLightVideo)
        {
            return FfMpegColor.Maroon;
        }
        else if (IsIndependenceDayVideo)
        {
            return FfMpegColor.Blue;
        }

        return FfMpegColor.Green;
    }

    public override string DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }
}