namespace Almostengr.VideoProcessor.Core.Constants;

public struct DrawTextPosition
{
    private string Value;
    public DrawTextPosition(string value)
    {
        Value = value;
    }
    
    private const int Padding = 30;
    public static readonly DrawTextPosition UpperLeft = new($"x={Padding}:y={Padding}");
    public static readonly DrawTextPosition UpperCenter = new($"x=(w-tw)/2:y={Padding}");
    public static readonly DrawTextPosition UpperRight = new($"x=w-tw-{Padding}:y={Padding}");
    public static readonly DrawTextPosition Centered = new($"x=(w-tw)/2:y=(h-th)/2");
    public static readonly DrawTextPosition LowerLeft = new($"x={Padding}:y=h-th-{Padding}");
    public static readonly DrawTextPosition LowerCenter = new($"x=(w-tw)/2:y=h-th-{Padding}");
    public static readonly DrawTextPosition LowerRight = new($"x=w-tw-{Padding}:y=h-th-{Padding}");

    private static readonly uint SubtitleXPadding = 100;
    private static readonly uint SubtitleTopYPadding = 150;
    private static readonly uint SubtitleBottomYPadding = SubtitleTopYPadding - 75;
    public static readonly DrawTextPosition SubtitlePrimary =  new($"x={SubtitleXPadding}:y=h-th-{SubtitleTopYPadding}");
    public static readonly DrawTextPosition SubtitleSecondary =  new($"x={SubtitleXPadding}:y=h-th-{SubtitleBottomYPadding}");

    public static readonly DrawTextPosition ChannelBrand = UpperRight;

    public override string ToString()
    {
        return Value.ToString();
    }
}
