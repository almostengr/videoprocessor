namespace Almostengr.VideoProcessor.Core.Constants;

// public static class FfmpegFontSize
// {
//     public static readonly string XLarge = "h/21";
//     public static readonly string Large = "h/28";
//     public static readonly string Medium = "h/35";
//     public static readonly string Small = "h/42";
// }

public struct FfmpegFontSize
{
    private string Size;
    public FfmpegFontSize(string size)
    {
        Size = size;
    }

    public static readonly FfmpegFontSize XLarge = new FfmpegFontSize("h/21");
    public static readonly FfmpegFontSize Large = new FfmpegFontSize("h/28");
    public static readonly FfmpegFontSize Medium = new FfmpegFontSize("h/35");
    public static readonly FfmpegFontSize Small = new FfmpegFontSize("h/42");
    // public const string XLarge = "h/21";
    // public const string Large = "h/28";
    // public const string Medium = "h/35";
    // public const string Small = "h/42";
}
