namespace Almostengr.VideoProcessor.Api.Constants
{
    public class FfMpegConstants
    {
        public const string FONT_SIZE = "h/35";
        public const string DIMMED_BACKGROUND = "0.3";
    }

    public class FfMpegPositions
    {
        public const string PADDING = "20";
        public readonly string UPPERLEFT = $"x={PADDING}:y={PADDING}";
        public readonly string UPPERCENTER = $"x=(w-text_w)/2:y={PADDING}";
        public readonly string UPPERRIGHT = $"x=w-tw-{PADDING}:y={PADDING}";
        public readonly string CENTERED = $"x=(w-text_w)/2:y=(h-text_h)/2";
        public readonly string LOWERLEFT = $"x={PADDING}:y=h-th-{PADDING}-30";
        public readonly string LOWERCENTER = $"x=(w-text_w)/2:y=h-th-{PADDING}-30";
        public readonly string LOWERRIGHT = $"x=w-tw-{PADDING}:y=h-th-{PADDING}-30";
    }

    public class FfMpegColors
    {
        public const string WHITE = "white";
        public const string BLACK = "black";
        public const string ORANGE = "orange";
    }

    public class FfMpegLogLevel
    {
        public const string ERROR = "error";
        public const string WARNING = "warning";
    }

}