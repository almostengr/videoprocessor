using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public sealed record SubtitleFileEntry
{
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public string Text { get; private set; }

    public SubtitleFileEntry(TimeSpan startTime, TimeSpan endTime, string text)
    {
        if (endTime >= startTime)
        {
            throw new ArgumentException("End time cannot be before start time", nameof(endTime));
        }

        text = text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or whitepsace", nameof(text));
        }

        StartTime = startTime;
        EndTime = endTime;
        SetText(text);
    }

    public void SetText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text canont be null or whitespace", nameof(text));
        }

        Text = FixMisspellings(text);
    }

    public string StartSeconds()
    {
        return StartTime.TotalSeconds.ToString();
    }

    public string EndSeconds()
    {
        return EndTime.TotalSeconds.ToString();
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
