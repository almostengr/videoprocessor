namespace Almostengr.VideoProcessor.Core.Videos;

public struct DrawTextPosition
{
    public string Value { get; }
    public DrawTextPosition(string value)
    {
        Value = value;
    }

    private const uint PADDING = 70;
    public static readonly DrawTextPosition UpperLeft = new($"x={PADDING}:y={PADDING}");
    public static readonly DrawTextPosition UpperCenter = new($"x=(w-tw)/2:y={PADDING}");
    public static readonly DrawTextPosition UpperRight = new($"x=w-tw-{PADDING}:y={PADDING}");
    public static readonly DrawTextPosition Centered = new($"x=(w-tw)/2:y=(h-th)/2");
    public static readonly DrawTextPosition LowerLeft = new($"x={PADDING}:y=h-th-{PADDING}");
    public static readonly DrawTextPosition LowerCenter = new($"x=(w-tw)/2:y=h-th-{PADDING}");
    public static readonly DrawTextPosition LowerRight = new($"x=w-tw-{PADDING}:y=h-th-{PADDING}");

    private static readonly uint SubtitleXPadding = 100;
    private static readonly uint SubtitleTopYPadding = 150;
    private static readonly uint SubtitleBottomYPadding = SubtitleTopYPadding - (SubtitleTopYPadding / 2);
    public static readonly DrawTextPosition SubtitlePrimary = new($"x={SubtitleXPadding}:y=h-th-{SubtitleTopYPadding}");
    public static readonly DrawTextPosition SubtitleSecondary = new($"x={SubtitleXPadding}:y=h-th-{SubtitleBottomYPadding}");

    public static readonly DrawTextPosition ChannelBrand = new($"x=w-tw-{PADDING}:y={PADDING}");
    public static readonly DrawTextPosition DashCamInfo = LowerRight;

    public override string ToString()
    {
        return Value.ToString();
    }
}
