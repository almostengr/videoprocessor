using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract record BaseVideoFile
{
    public string Title { get; init; }
    public string TarballFilePath { get; }
    public string TarballFileName { get; init; }
    public string GraphicsSubtitleFileName { get; private set; }
    public List<DrawTextFilter> DrawTextFilters { get; private set; }
    public bool IsDraft { get; init; }
    public string OutputVideoFileName { get; init; }

    public readonly string ROBINSON_SERVICES = "Robinson Handy and Technology Services";
    public readonly string RHT_WEBSITE = "rhtservices.net";
    public readonly string RHT_SOCIAL_LINKS = "rhtservices.net/links";

    public BaseVideoFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is null or whitespace", nameof(filePath));
        }

        TarballFilePath = filePath; allFilePath);
        GraphicsSubtitleFileName = string.Empty;
        Title = SetTitle(TarballFileName);
        OutputVideoFileName = TarballFileName
            .Replace(FileExtension.TarXz.ToString(), string.Empty.ToString())
            .Replace(FileExtension.TarGz.ToString(), string.Empty.ToString())
            .Replace(FileExtension.Tar.ToString(), string.Empty.ToString())
            + FileExtension.Mp4;

        if (Title.ToLower().Contains("draft"))
        {
            IsDraft = true;
        }

        DrawTextFilters = new();
    }

    public abstract string[] BrandingTextOptions();

    public string VideoFilters()
    {
        StringBuilder stringBuilder = new();

        var lastItem = DrawTextFilters.Last();
        foreach (var filter in DrawTextFilters)
        {
            stringBuilder.Append(filter.ToString());

            if (!filter.Equals(lastItem))
            {
                stringBuilder.Append(Constant.CommaSpace);
            }
        }

        return stringBuilder.ToString();
    }

    public virtual string SetTitle(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Video title cannot be null or whitespace", nameof(fileName));
        }

        return fileName.Replace(FileExtension.TarGz.ToString(), string.Empty.ToString())
            .Replace(FileExtension.TarXz.ToString(), string.Empty.ToString())
            .Replace(FileExtension.Tar.ToString(), string.Empty.ToString())
            .Replace(Constant.Colon, string.Empty);
    }

    public void SetGraphicsSubtitleFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        GraphicsSubtitleFileName = fileName;
    }

    public virtual FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public virtual FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }

    public void AddDrawTextVideoFilter(
        string text, FfMpegColor textColor, Opacity textBrightness, FfmpegFontSize fontSize,
        FfMpegColor backgroundColor, Opacity backgroundBrightness, DrawTextPosition position,
        uint startSeconds, uint durationSeconds)
    {
        // todo split if there is semi colon in text. add addition filter for text with inverted colors

        DrawTextFilters.Add(
            new DrawTextFilter(text, textColor, textBrightness, backgroundColor,
            backgroundBrightness, fontSize, position, startSeconds, durationSeconds));
    }

    public virtual void AddDrawTextVideoFilterFromSubtitles(List<SubtitleFileEntry> subtitles)
    {
        foreach (var subtitle in subtitles)
        {
            if (string.IsNullOrWhiteSpace(subtitle.Text))
            {
                continue;
            }

            DrawTextFilters.Add(
                new DrawTextFilter(subtitle.Text, DrawTextFilterTextColor(), Opacity.Full,
                DrawTextFilterBackgroundColor(), Opacity.Full,
                FfmpegFontSize.XLarge, DrawTextPosition.SubtitlePrimary,
                subtitle.StartSeconds(), subtitle.Duration(), 25));
        }
    }

    public virtual void AddDrawTextVideoFilter(
        string text, FfMpegColor textColor, Opacity textBrightness, FfmpegFontSize fontSize, DrawTextPosition position,
        FfMpegColor backgroundColor, Opacity backgroundBrightness, int borderWidth = 10, string duration = "")
    {
        DrawTextFilters.Add(
            new DrawTextFilter(text, textColor, textBrightness, backgroundColor,
            backgroundBrightness, fontSize, position, 0, 0));
    }

    public virtual void AddLowerThird(
        uint startSeconds, uint durationSeconds, string primaryText, string? secondaryText = null)
    {
        DrawTextFilters.Add(
            new DrawTextFilter(primaryText, DrawTextFilterTextColor(), Opacity.Full,
            DrawTextFilterBackgroundColor(), Opacity.Full,
            FfmpegFontSize.Medium, DrawTextPosition.SubtitlePrimary, startSeconds, durationSeconds));

        if (string.IsNullOrEmpty(secondaryText))
        {
            return;
        }

        DrawTextFilters.Add(
            new DrawTextFilter(secondaryText, DrawTextFilterBackgroundColor(), Opacity.Full,
            DrawTextFilterTextColor(), Opacity.Full,
            FfmpegFontSize.Medium, DrawTextPosition.SubtitlePrimary, startSeconds, durationSeconds));
    }

    protected string FilterDuration(int duration = 239)
    {
        return $"enable=lt(mod(t\\,{duration})\\,{Constant.CallToActionDuration})";
    }

    public void AddSubscribeVideoFilter(int duration)
    {
        AddDrawTextVideoFilter(
            "SUBSCRIBE for future videos",
            FfMpegColor.White,
            Opacity.Full,
            FfmpegFontSize.Large,
            DrawTextPosition.LowerLeft,
            FfMpegColor.Red,
            Opacity.Full,
            10,
            FilterDuration(duration)
        );
    }

    public void AddLikeVideoFilter(int duration)
    {
        AddDrawTextVideoFilter(
            "Please LIKE this video",
            FfMpegColor.Blue,
            Opacity.Full,
            FfmpegFontSize.Large,
            DrawTextPosition.LowerLeft,
            FfMpegColor.White,
            Opacity.Full,
            10,
            FilterDuration(duration)
        );
    }

}

public sealed class DrawTextFilter
{
    private string Text { get; init; }
    private FfMpegColor TextColor { get; init; }
    private Opacity TextBrightness { get; init; }
    private FfmpegFontSize FontSize { get; init; }
    private DrawTextPosition Position { get; init; }
    private FfMpegColor BackgroundColor { get; init; }
    private Opacity BackgroundBrightness { get; init; }
    private uint StartSeconds { get; init; }
    private uint DurationSeconds { get; init; }
    private uint BorderWidth { get; init; }
    private string? Duration { get; init; }

    public DrawTextFilter(
        string text, FfMpegColor textColor, Opacity textBrightness,
        FfMpegColor backgroundColor, Opacity backgroundBrightness,
        FfmpegFontSize fontSize, DrawTextPosition position, uint startSeconds = 0, uint durationSeconds = 0,
        uint borderWidth = 10, string? duration = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text is null or whitespace");
        }

        // if (text.Length > 60)
        // {
        //     throw new ArgumentException("Text is too long");
        // }

        if (textBrightness.ToString() == Opacity.None.ToString())
        {
            throw new ArgumentException("Text brightness cannot be none");
        }

        if (textColor.ToString() == backgroundColor.ToString())
        {
            throw new ArgumentException("Text and background color cannot be the same");
        }

        // if (durationSeconds <= 3 && startSeconds != 0)
        // {
        //     throw new ArgumentException("Duration is too short");
        // }

        if (string.IsNullOrWhiteSpace(duration))
        {
            StartSeconds = startSeconds;
            DurationSeconds = durationSeconds;
        }

        Text = text;

        switch(Text.Length)
        {
            case int x when x <= 60:
                FontSize = FfmpegFontSize.XLarge;
                break;

            case int x when x <= 100:
                FontSize = FfmpegFontSize.Large;
                break;

            case int x when x <= 140:
                FontSize = FfmpegFontSize.Medium;
                break;

            default:
                throw new ArgumentException("Text length is too long");
                break;
        }
        // FontSize = fontSize;

        TextColor = textColor;
        TextBrightness = textBrightness;
        Position = position;
        BackgroundColor = backgroundColor;
        BackgroundBrightness = backgroundBrightness;
        Duration = duration;
        BorderWidth = borderWidth;
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

        if (StartSeconds > 0 && DurationSeconds > 0)
        {
            stringBuilder.Append(Constant.Colon);
            stringBuilder.Append($"enable='between(t,{StartSeconds}, {(StartSeconds + DurationSeconds)})'");
        }

        if (!string.IsNullOrWhiteSpace(Duration))
        {
            stringBuilder.Append(Constant.Colon));
            stringBuilder.Append(Duration);
        }

        return stringBuilder.ToString();
    }
}
