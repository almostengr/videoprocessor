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
        Text = text.ReplaceIgnoringCase("um", string.Empty)
            .ReplaceIgnoringCase("uh", string.Empty)
            .ReplaceIgnoringCase("[music] you", "[music]")
            .ReplaceIgnoringCase("all right", "alright")
            .Replace(Constant.DoubleWhitespace, Constant.Whitespace)
            .Trim();
    }
}
