namespace Almostengr.VideoProcessor.Core.Constants;

// public static class Opacity
// {
//     public static readonly string Full = "1.0";
//     public static readonly string Heavy = "0.8";
//     public static readonly string Medium = "0.5";
//     public static readonly string Light = "0.3";
//     public static readonly string None = "0.0";
// }

public struct Opacity
{
    // public static readonly string Full = "1.0";
    // public static readonly string Heavy = "0.8";
    // public static readonly string Medium = "0.5";
    // public static readonly string Light = "0.3";
    // public static readonly string None = "0.0";

    private string Percentage;

    public Opacity(string percentage)
    {
        Percentage = percentage;
    }

    public static readonly Opacity Full = new Opacity("1.0");
    public static readonly Opacity Heavy = new Opacity("0.8");
    public static readonly Opacity Medium = new Opacity("0.5");
    public static readonly Opacity Light = new Opacity("0.3");
    public static readonly Opacity None = new Opacity("0.0");
}
