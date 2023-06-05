using System.Text;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

internal sealed class DrawTextFilter
{
    private string Text { get; init; }
    private FfMpegColor TextColor { get; init; }
    private Opacity TextBrightness { get; init; }
    private FfmpegFontSize FontSize { get; init; }
    private DrawTextPosition Position { get; init; }
    private FfMpegColor BackgroundColor { get; init; }
    private Opacity BackgroundBrightness { get; init; }
    private uint BorderWidth { get; init; }
    private TimeSpan? StartTime { get; init; }
    private TimeSpan? EndTime { get; init; }

    public DrawTextFilter(string text, FfMpegColor textColor, Opacity textBrightness,
        FfMpegColor backgroundColor, Opacity backgroundBrightness, DrawTextPosition position,
        TimeSpan? startTime = null, TimeSpan? endTime = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text is null or whitespace");
        }

        if (textBrightness.ToString() == Opacity.None.ToString())
        {
            throw new ArgumentException("Text brightness cannot be none");
        }

        if (textColor.ToString() == backgroundColor.ToString())
        {
            throw new ArgumentException("Text and background color cannot be the same");
        }

        if (endTime <= startTime)
        {
            throw new ArgumentException("Start time must be before end time");
        }

        const uint TEXT_MAX_CHARACTERS = 80;
        if (text.Length > TEXT_MAX_CHARACTERS)
        {
            throw new ArgumentException($"Text length is too long: {text}", nameof(text));
        }

        FontSize = FfmpegFontSize.Medium;

        if (position.ToString() == DrawTextPosition.SubtitlePrimary.ToString())
        {
            if (text.Length < 40)
            {
                FontSize = FfmpegFontSize.XLarge;
            }
            else if (text.Length < 60)
            {
                FontSize = FfmpegFontSize.Large;
            }
        }
        else if (position.ToString() == DrawTextPosition.DashCamInfo.ToString() ||
                position.ToString() == DrawTextPosition.ChannelBrand.ToString())
        {
            FontSize = FfmpegFontSize.Large;
        }

        Text = text.Replace("=", "\\=").Replace(":", "\\:");
        Position = position;
        TextColor = textColor;
        TextBrightness = textBrightness;
        BackgroundColor = backgroundColor;
        BackgroundBrightness = backgroundBrightness;
        BorderWidth = 20;
        StartTime = startTime;
        EndTime = endTime;
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append($"drawtext=textfile:'{Text.Trim()}':");
        stringBuilder.Append($"fontcolor={TextColor.ToString()}@{TextBrightness.ToString()}:");
        stringBuilder.Append($"fontsize={FontSize.ToString()}:");
        stringBuilder.Append($"{Position.ToString()}:");
        stringBuilder.Append(Constant.BorderBox);
        stringBuilder.Append($"boxborderw={BorderWidth.ToString()}:");
        stringBuilder.Append($"boxcolor={BackgroundColor.ToString()}@{BackgroundBrightness.ToString()}");

        if (StartTime != null && EndTime != null)
        {
            stringBuilder.Append(Constant.Colon);
            stringBuilder.Append($"enable='between(t, {(uint)StartTime.Value.TotalSeconds}, {(uint)EndTime.Value.TotalSeconds})'");
        }

        return stringBuilder.ToString();
    }
}
