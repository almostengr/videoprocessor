using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos;

namespace Almostengr.VideoProcessor.Domain.Toastmasters;

public sealed record ToastmastersVideo : BaseVideo
{
    public ToastmastersVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    internal override void SetChannelBannerText(string text)
    {
        AddDrawTextFilter(text, 
            BannerTextColor(), 
            Opacity.Full,
            FfmpegFontSize.XLarge, 
            DrawTextPosition.UpperRight, 
            BannerBackgroundColor(), 
            Opacity.Full,
            Constant.BorderBoxWidthLarge);
    }

    public override string BannerBackgroundColor()
    {
        return FfMpegColor.SteelBlue;
    }

    public override string BannerTextColor()
    {
        return FfMpegColor.White;
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