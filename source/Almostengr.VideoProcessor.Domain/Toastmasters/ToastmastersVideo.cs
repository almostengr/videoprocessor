using System.Text;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;

namespace Almostengr.VideoProcessor.Domain.Toastmasters;

public sealed record ToastmastersVideo : BaseVideo
{
    public ToastmastersVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public override void AddChannelBannerTextFilter()
    {
        if (VideoFilter.Contains(ChannelBannerText()))
        {
            return;
        }

        StringBuilder textFilter = new($"drawtext=textfile:'{ChannelBannerText()}':");
        textFilter.Append($"fontcolor={BannerTextColor()}:");
        textFilter.Append($"fontsize={FfmpegFontSize.Large}:");
        textFilter.Append($"{DrawTextPosition.UpperRight}:");
        textFilter.Append(Constant.BorderChannelText);
        textFilter.Append($"boxcolor={BannerBackgroundColor()}@{Constant.DimBackground}");
        // return videoFilter.ToString();
        // VideoFilter = textFilter.ToString();
        AddDrawTextFilter(textFilter.ToString());
    }

    public override string BannerBackgroundColor()
    {
        return FfMpegColors.SteelBlue;
    }

    public override string ChannelBannerText()
    {
        string[] text = {"towertoastmasters.org", "Tower Toastmasters"};
        return text[_random.Next(0, text.Count())];
    }

    public override string BannerTextColor()
    {
        return FfMpegColors.White;
    }

    public override string SubtitleBackgroundColor()
    {
        return BannerBackgroundColor();
    }

    public override string SubtitleTextColor()
    {
        return BannerTextColor();
    }
}