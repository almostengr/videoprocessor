using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public sealed class SubtitleFileEntry
{
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public string Text { get; init; }

    public SubtitleFileEntry(TimeSpan startTime, TimeSpan endTime, string text)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("Start time must be before end time", nameof(endTime));
        }

        text = text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        StartTime = startTime;
        EndTime = endTime;
        Text = text.Replace("um", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("uh", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("[music] you", "[music]", StringComparison.OrdinalIgnoreCase)
            .Replace("all right", "alright", StringComparison.OrdinalIgnoreCase)
            .Replace(Constant.DoubleWhitespace, Constant.Whitespace)
            .Trim();
    }
}
