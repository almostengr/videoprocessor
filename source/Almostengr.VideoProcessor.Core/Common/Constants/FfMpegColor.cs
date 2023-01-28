namespace Almostengr.VideoProcessor.Core.Constants;

// public class FfMpegColor
// {
//     public static readonly string Black = "black";
//     public static readonly string Blue = "blue";
//     public static readonly string DarkGreen = "darkGreen";
//     public static readonly string GhostWhite = "ghostwhite";
//     public static readonly string Green = "green";
//     public static readonly string Maroon = "maroon";
//     public static readonly string Orange = "orange";
//     public static readonly string Red = "red";
//     public static readonly string RhtYellow = "0xffc107";
//     public static readonly string SteelBlue = "steelblue";
//     public static readonly string White = "white";
//     public static readonly string SaddleBrown = "SaddleBrown";
// }

// public struct FfMpegColor
// {
//     public const string Black = "black";
//     public const string Blue = "blue";
//     public const string DarkGreen = "darkGreen";
//     public const string GhostWhite = "ghostwhite";
//     public const string Green = "green";
//     public const string Maroon = "maroon";
//     public const string Orange = "orange";
//     public const string Red = "red";
//     public const string RhtYellow = "0xffc107";
//     public const string SteelBlue = "steelblue";
//     public const string White = "white";
//     public const string SaddleBrown = "SaddleBrown";
// }

public struct FfMpegColor
{
    private string Color;
    public FfMpegColor(string color)
    {
        Color = color;
    }

    public static readonly FfMpegColor Black = new FfMpegColor("black");
    public static readonly FfMpegColor Blue = new FfMpegColor("blue");
    public static readonly FfMpegColor DarkGreen = new FfMpegColor("darkGreen");
    public static readonly FfMpegColor GhostWhite = new FfMpegColor("ghostwhite");
    public static readonly FfMpegColor Green = new FfMpegColor("green");
    public static readonly FfMpegColor Maroon = new FfMpegColor("maroon");
    public static readonly FfMpegColor Orange = new FfMpegColor("orange");
    public static readonly FfMpegColor Red = new FfMpegColor("red");
    public static readonly FfMpegColor RhtYellow = new FfMpegColor("0xffc107");
    public static readonly FfMpegColor SteelBlue = new FfMpegColor("steelblue");
    public static readonly FfMpegColor White = new FfMpegColor("white");
    public static readonly FfMpegColor SaddleBrown = new FfMpegColor("SaddleBrown");
}
