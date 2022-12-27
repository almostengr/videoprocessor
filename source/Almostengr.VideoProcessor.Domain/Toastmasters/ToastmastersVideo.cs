using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Toastmasters;

public sealed record ToastmastersVideo : BaseVideo
{
    public ToastmastersVideo(string baseDirectory) : base(baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    public override string BoxColor()
    {
        return FfMpegColors.SteelBlue;
    }

    public override string ChannelBannerText()
    {
        string[] text = {"towertoastmasters.org", "Tower Toastmasters"};
        return text[_random.Next(0, text.Count())];
    }

    public override string TextColor()
    {
        return FfMpegColors.White;
    }
}