using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.TechTalk.Exceptions;

namespace Almostengr.VideoProcessor.Core.TechTalk;

internal sealed record TechTalkVideo : BaseVideo
{
    internal bool IsChristmasLightVideo { get; private set; }
    internal bool IsIndependenceDayVideo { get; private set; }

    internal TechTalkVideo(string baseDirectory, string archiveFileName) : base(baseDirectory, archiveFileName)
    {
        IsChristmasLightVideo = false;
        IsIndependenceDayVideo = false;
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
        if (IsIndependenceDayVideo)
        {
            throw new ChristmasAndIndependenceVideoException();
        }

        IsChristmasLightVideo = true;
    }

    internal void ConfirmIndependenceDayVideo()
    {
        if (IsChristmasLightVideo)
        {
            throw new ChristmasAndIndependenceVideoException();
        }

        IsIndependenceDayVideo = true;
    }

    internal override string[] BrandingTextOptions()
    {
        const string TECH_TALK = "Tech Talk with RHT Services";

        if (IsChristmasLightVideo || IsIndependenceDayVideo)
        {
            return new string[] { RHT_WEBSITE, TECH_TALK, "twitter.com/hplightshow" };
        }

        return new string[] { RHT_WEBSITE, TECH_TALK };
    }

    internal override string DrawTextFilterBackgroundColor()
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

    internal override string DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }
}