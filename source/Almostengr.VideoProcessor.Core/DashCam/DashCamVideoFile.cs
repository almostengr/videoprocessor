using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed record DashCamVideoFile : BaseVideoFile
{
    private DashCamVideoType SubType { get; init; }

    public DashCamVideoFile(string videoFilePath) : base(videoFilePath)
    {
        SubType = DashCamVideoType.Normal;

        if (Title.ToLower().Contains("night"))
        {
            SubType = DashCamVideoType.Night;
        }
        else if (Title.ToLower().Contains("firework"))
        {
            SubType = DashCamVideoType.Fireworks;
        }
    }

    public override string[] BrandingTextOptions()
    {
        return new string[] { "Kenny Ram Dash Cam" };
    }

    protected override string SetTitle(string fileName)
    {
        if (fileName.ToLower().Contains("bad drivers of montgomery"))
        {
            throw new ArgumentException("Title contains invalid text", nameof(fileName));
        }

        return base.SetTitle(fileName.Split(Constant.SemiColon).First());
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        // if (SubType == DashCamVideoType.Fireworks)
        // {
        //     return FfMpegColor.Blue;
        // }

        // return FfMpegColor.Black;

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
        // if (SubType == DashCamVideoType.Night)
        // {
        //     return FfMpegColor.Orange;
        // }

        // return FfMpegColor.White;

        switch (SubType)
        {
            case DashCamVideoType.Night:
                return FfMpegColor.Orange;

            default:
                return FfMpegColor.White;
        }
    }

    public string TitleDrawTextFilter()
    {
        return (new DrawTextFilter(
            Title,
            DrawTextFilterTextColor(),
            Opacity.Full,
            DrawTextFilterBackgroundColor(),
            Opacity.Medium,
            DrawTextPosition.UpperLeft)).ToString();
    }

    enum DashCamVideoType
    {
        Normal,
        Night,
        Fireworks
    }

    // public override void AddDrawTextVideoFilterFromSubtitles(List<SubtitleFileEntry> subtitles)
    // {
    //     foreach (var subtitle in subtitles)
    //     {
    //         if (string.IsNullOrWhiteSpace(subtitle.Text))
    //         {
    //             continue;
    //         }

    //         var splitTitles = subtitle.Text.Split(Constant.SemiColon);

    //         FfMpegColor bgColor = FfMpegColor.Green;
    //         Opacity bgOpacity = Opacity.Full;

    //         string textLowered = subtitle.Text.ToLower();
    //         if (textLowered.StartsWith("info"))
    //         {
    //             bgColor = FfMpegColor.Black;
    //             bgOpacity = Opacity.Medium;
    //         }

    //         DrawTextFilters.Add(
    //             new DrawTextFilter(splitTitles.First(), FfMpegColor.White, Opacity.Full,
    //             bgColor, bgOpacity, DrawTextPosition.DashCamInfo,
    //             subtitle.StartTime, (subtitle.StartTime.Add(TimeSpan.FromSeconds(5)))
    //             ));
    //     }
    // }
}


// public enum DashCamVideoType
// {
//     Normal,
//     Night,
//     Fireworks
// }