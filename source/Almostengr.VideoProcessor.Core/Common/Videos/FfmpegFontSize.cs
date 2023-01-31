namespace Almostengr.VideoProcessor.Core.Constants;

public struct FfmpegFontSize
{
    private string Value;
    public FfmpegFontSize(string value)
    {
        Value = value;
    }

    public static readonly FfmpegFontSize XLarge = new FfmpegFontSize("h/21");
    public static readonly FfmpegFontSize Large = new FfmpegFontSize("h/28");
    public static readonly FfmpegFontSize Medium = new FfmpegFontSize("h/35");
    public static readonly FfmpegFontSize Small = new FfmpegFontSize("h/42");

    public override string ToString()
    {
        return Value.ToString();
    }
}
