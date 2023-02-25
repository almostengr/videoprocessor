using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed class TechTalkVideoFile : VideoFile
{
    private TechTalkVideoSubType SubType { get; set; }

    public TechTalkVideoFile(VideoProjectArchiveFile videoProjectArchiveFile) : base(videoProjectArchiveFile)
    {
        SetVideoType();
    }

    public TechTalkVideoFile(string filePath) : base(filePath)
    {
        SetVideoType();
    }

    private void SetVideoType()
    {
        SubType = TechTalkVideoSubType.TechTalk;

        if (Title().ToLower().Contains("christmas light show"))
        {
            SubType = TechTalkVideoSubType.Christmas;
        }
        else if (Title().ToLower().Contains("4th of july"))
        {
            SubType = TechTalkVideoSubType.IndependenceDay;
        }
    }

    // public override string[] BrandingTextOptions()
    public override string BrandingText()
    {
        Random random = new();
        
        List<string> options = new();
        options.Add("Tech Talk with RHT Services");
        options.Add(Constant.RHT_WEBSITE);
        options.Add("@rhtservicestech");
        options.Add("#rhtservicestech");
        options.Add("rhtservices.net/techtalk");
        
        // return options.ToArray();
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

    // {
    //     throw new NotImplementedException();
    // }

    enum TechTalkVideoSubType
    {
        TechTalk,
        Christmas,
        IndependenceDay
    }
}