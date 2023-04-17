using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class BaseVideoProject
{
    public string FilePath { get; init; }

    public BaseVideoProject(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(Constant.ArgumentCannotBeNullOrEmpty, nameof(filePath));
        }

        if (
            !filePath.EndsWith(FileExtension.Tar.Value, StringComparison.OrdinalIgnoreCase) &&
            !filePath.EndsWith(FileExtension.TarGz.Value, StringComparison.OrdinalIgnoreCase) &&
            !filePath.EndsWith(FileExtension.TarXz.Value, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(Constant.FileTypeIsIncorrect, nameof(filePath));
        }

        FilePath = filePath;
    }

    public abstract string BrandingText();

    public virtual FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public virtual FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }

    public string ThumbnailFileName()
    {
        return Title() + FileExtension.ThumbTxt.Value;
    }

    public virtual string Title()
    {
        string title = FileName()
            .Replace(FileExtension.Mp4.Value, string.Empty)
            .Replace(FileExtension.TarXz.Value, string.Empty)
            .Replace(FileExtension.TarGz.Value, string.Empty)
            .Replace(FileExtension.Tar.Value, string.Empty)
            .Replace(Constant.Colon, string.Empty);

        const int MAX_TITLE_LENGTH = 100;

        if (title.Length > MAX_TITLE_LENGTH)
        {
            throw new VideoTitleTooLongException($"Title is {title.Length} characters long, greater than {MAX_TITLE_LENGTH}");
        }

        return title;
    }

    public string VideoFileName()
    {
        return Path.GetFileName(FilePath)
            .Replace(FileExtension.TarXz.Value, string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(FileExtension.TarGz.Value, string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(FileExtension.Tar.Value, string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(Constant.Colon, string.Empty)
            + FileExtension.Mp4.Value;
    }

    public string FileName()
    {
        return Path.GetFileName(FilePath);
    }


    public virtual string VideoFilters()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append(
            new DrawTextFilter(BrandingText(), DrawTextFilterTextColor(), Opacity.Full,
            DrawTextFilterBackgroundColor(), Opacity.Medium, DrawTextPosition.ChannelBrand).ToString()
            );

        return stringBuilder.ToString();
    }


    protected sealed class DrawTextFilter
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

            if (endTime < startTime)
            {
                throw new ArgumentException("Start time must be before end time");
            }

            if (text.Length > 80)
            {
                throw new ArgumentException($"Text length is too long: {text}", nameof(text));
            }

            Text = text.Replace("=", "\\=").Replace(":", "\\:");
            Position = position;

            if (Position.ToString() == DrawTextPosition.SubtitlePrimary.ToString() && Text.Length <= 60)
            {
                if (Text.Length > 60)
                {
                    throw new ArgumentException($"Primary subtitle length is too long: {text}", nameof(text));
                }
                FontSize = FfmpegFontSize.XLarge;
            }
            else if (Position.ToString() == DrawTextPosition.SubtitleSecondary.ToString() ||
                Position.ToString() == DrawTextPosition.DashCamInfo.ToString())
            {
                FontSize = FfmpegFontSize.Large;
            }
            else
            {
                FontSize = FfmpegFontSize.Medium;
            }

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

}
