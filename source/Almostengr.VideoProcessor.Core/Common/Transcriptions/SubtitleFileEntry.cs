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
        Text = FixMisspellings(text);
    }

    private string FixMisspellings(string input)
    {
        return input
            .Replace("um", string.Empty)
            .Replace("uh", string.Empty)
            .Replace("[music] you", "[music]")
            .Replace(Constant.DoubleWhitespace, Constant.Whitespace)
            .Replace("all right", "alright")
            .Trim();
    }
}
