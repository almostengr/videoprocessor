namespace Almostengr.VideoProcessor.Domain.Common.Constants;

public static class DrawTextPosition
{
    static int PADDING = 30;
    public static string UpperLeft = $"x={PADDING}:y={PADDING}";
    public static string UpperCenter = $"x=(w-tw)/2:y={PADDING}";
    public static string UpperRight = $"x=w-tw-{PADDING}:y={PADDING}";
    public static string Centered = $"x=(w-tw)/2:y=(h-th)/2";
    public static string LowerLeft = $"x={PADDING}:y=h-th-{PADDING}";
    public static string LowerCenter = $"x=(w-tw)/2:y=h-th-{PADDING}";
    public static string LowerRight = $"x=w-tw-{PADDING}:y=h-th-{PADDING}";
}