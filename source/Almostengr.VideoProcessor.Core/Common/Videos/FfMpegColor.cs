namespace Almostengr.VideoProcessor.Core.Constants;

public struct FfMpegColor
{
    public string Value { get; }
    public FfMpegColor(string value)
    {
        Value = value;
    }

    public static readonly FfMpegColor Black = new FfMpegColor("black");
    public static readonly FfMpegColor Blue = new FfMpegColor("blue");
    // public static readonly FfMpegColor DarkGreen = new FfMpegColor("darkGreen");
    // public static readonly FfMpegColor GhostWhite = new FfMpegColor("ghostwhite");
    public static readonly FfMpegColor Green = new FfMpegColor("green");
    public static readonly FfMpegColor Maroon = new FfMpegColor("maroon");
    public static readonly FfMpegColor Orange = new FfMpegColor("orange");
    // public static readonly FfMpegColor Red = new FfMpegColor("red");
    public static readonly FfMpegColor RhtYellow = new FfMpegColor("0xffc107");
    public static readonly FfMpegColor SteelBlue = new FfMpegColor("steelblue");
    public static readonly FfMpegColor White = new FfMpegColor("white");
    // public static readonly FfMpegColor SaddleBrown = new FfMpegColor("SaddleBrown");

    public override string ToString()
    {
        return Value.ToString();
    }
}
