namespace Almostengr.VideoProcessor.Core.Constants;

// public class DrawTextPosition
// public struct DrawTextPosition
// {
//     private static readonly int Padding = 30;
//     public readonly string UpperLeft = $"x={Padding}:y={Padding}";
//     public readonly string UpperCenter = $"x=(w-tw)/2:y={Padding}";
//     public readonly string UpperRight = $"x=w-tw-{Padding}:y={Padding}";
//     public readonly string Centered = $"x=(w-tw)/2:y=(h-th)/2";
//     public readonly string LowerLeft = $"x={Padding}:y=h-th-{Padding}";
//     public readonly string LowerCenter = $"x=(w-tw)/2:y=h-th-{Padding}";
//     public readonly string LowerRight = $"x=w-tw-{Padding}:y=h-th-{Padding}";
// }
// public class DrawTextPosition
// {
//     private static readonly int Padding = 30;
//     public static readonly string UpperLeft = $"x={Padding}:y={Padding}";
//     public static readonly string UpperCenter = $"x=(w-tw)/2:y={Padding}";
//     public static readonly string UpperRight = $"x=w-tw-{Padding}:y={Padding}";
//     public static readonly string Centered = $"x=(w-tw)/2:y=(h-th)/2";
//     public static readonly string LowerLeft = $"x={Padding}:y=h-th-{Padding}";
//     public static readonly string LowerCenter = $"x=(w-tw)/2:y=h-th-{Padding}";
//     public static readonly string LowerRight = $"x=w-tw-{Padding}:y=h-th-{Padding}";
// }


// public struct DrawTextPosition2
// {
//     private const int Padding = 30;
//     public const string UpperLeft = $"x={Padding}:y={Padding}";
//     public const string UpperCenter = $"x=(w-tw)/2:y={Padding}";
//     public const string UpperRight = $"x=w-tw-{Padding}:y={Padding}";
//     public const string Centered = $"x=(w-tw)/2:y=(h-th)/2";
//     public const string LowerLeft = $"x={Padding}:y=h-th-{Padding}";
//     public const string LowerCenter = $"x=(w-tw)/2:y=h-th-{Padding}";
//     public const string LowerRight = $"x=w-tw-{Padding}:y=h-th-{Padding}";
// }

public struct DrawTextPosition
{
    private const int Padding = 30;
    private string Value;
    public DrawTextPosition(string value)
    {
        Value = value;
    }
    
    public static readonly DrawTextPosition UpperLeft = new($"x={Padding}:y={Padding}");
    public static readonly DrawTextPosition UpperCenter = new($"x=(w-tw)/2:y={Padding}");
    public static readonly DrawTextPosition UpperRight = new($"x=w-tw-{Padding}:y={Padding}");
    public static readonly DrawTextPosition Centered = new($"x=(w-tw)/2:y=(h-th)/2");
    public static readonly DrawTextPosition LowerLeft = new($"x={Padding}:y=h-th-{Padding}");
    public static readonly DrawTextPosition LowerCenter = new($"x=(w-tw)/2:y=h-th-{Padding}");
    public static readonly DrawTextPosition LowerRight = new($"x=w-tw-{Padding}:y=h-th-{Padding}");

    private static readonly uint SubtitleXPadding = 75;
    private static readonly uint SubtitleTopYPadding = 80;
    private static readonly uint SubtitleBottomYPadding = SubtitleTopYPadding - 50;
    public static readonly DrawTextPosition SubtitlePrimary =  new($"x={SubtitleXPadding}:y=h-th-{SubtitleTopYPadding}");
    public static readonly DrawTextPosition SubtitleSecondary =  new($"x={SubtitleXPadding}:y=h-th-{SubtitleBottomYPadding}");


    // public static readonly string UpperLeft = $"x={Padding}:y={Padding}";
    // public static readonly string UpperCenter = $"x=(w-tw)/2:y={Padding}";
    // public static readonly string UpperRight = $"x=w-tw-{Padding}:y={Padding}";
    // public static readonly string Centered = $"x=(w-tw)/2:y=(h-th)/2";
    // public static readonly string LowerLeft = $"x={Padding}:y=h-th-{Padding}";
    // public static readonly string LowerCenter = $"x=(w-tw)/2:y=h-th-{Padding}";
    // public static readonly string LowerRight = $"x=w-tw-{Padding}:y=h-th-{Padding}";

    public override string ToString()
    {
        return Value.ToString();
    }
}
