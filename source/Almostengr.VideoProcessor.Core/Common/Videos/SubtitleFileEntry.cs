using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public sealed record SubtitleFileEntry
{
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public string Text { get; private set; }

    public SubtitleFileEntry(TimeSpan startTime, TimeSpan endTime, string text)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("Start time must be before end time", nameof(endTime));
        }

        text = text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            // throw new ArgumentException("Text cannot be null or whitepsace", nameof(text));
            return;
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

    public uint StartSeconds()
    {
        return (uint)StartTime.TotalSeconds;
    }

    public uint EndSeconds()
    {
        return (uint)EndTime.TotalSeconds;
    }

    public uint Duration()
    {
        return (uint)(EndTime - StartTime).TotalSeconds;
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
