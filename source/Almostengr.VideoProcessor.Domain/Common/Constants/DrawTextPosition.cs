namespace Almostengr.VideoProcessor.Domain.Common.Constants;

public static class DrawTextPosition
{
    private static readonly int Padding = 30;
    public static readonly string UpperLeft = $"x={Padding}:y={Padding}";
    public static readonly string UpperCenter = $"x=(w-tw)/2:y={Padding}";
    public static readonly string UpperRight = $"x=w-tw-{Padding}:y={Padding}";
    public static readonly string Centered = $"x=(w-tw)/2:y=(h-th)/2";
    public static readonly string LowerLeft = $"x={Padding}:y=h-th-{Padding}";
    public static readonly string LowerCenter = $"x=(w-tw)/2:y=h-th-{Padding}";
    public static readonly string LowerRight = $"x=w-tw-{Padding}:y=h-th-{Padding}";
}