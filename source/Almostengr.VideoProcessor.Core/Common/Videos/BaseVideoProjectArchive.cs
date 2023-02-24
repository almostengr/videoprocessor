using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class BaseVideoProjectArchive
{
    public string FilePath { get; init; }
    public AssSubtitleFile? GraphicsSubtitleFile { get; private set; }
    public string BrandingText { get; private set; }

    public BaseVideoProjectArchive(string filePath)
    {
        if (
            !filePath.ToLower().EndsWith(FileExtension.Tar.Value) &&
            !filePath.ToLower().EndsWith(FileExtension.TarXz.Value) &&
            !filePath.ToLower().EndsWith(FileExtension.TarGz.Value))
        {
            throw new ArgumentException("File path is not valid", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new ArgumentException("Project file does not exist", nameof(filePath));
        }

        FilePath = filePath;
        BrandingText = Constant.RHT_WEBSITE;
    }

    public abstract string[] BrandingTextOptions();

    public string ArchiveFilePath()
    {
        return FilePath.Replace($"/incoming/", $"/archive/");
    }

    public virtual string Title()
    {
        return Path.GetFileName(FileName())
            .Replace(FileExtension.TarXz.Value, string.Empty)
            .Replace(FileExtension.TarGz.Value, string.Empty)
            .Replace(FileExtension.TarXz.Value, string.Empty)
            .Replace(Constant.Colon, string.Empty);
    }

    public string OutputFileName()
    {
        return Path.GetFileName(FilePath)
            .Replace(FileExtension.TarXz.Value, string.Empty)
            .Replace(FileExtension.TarGz.Value, string.Empty)
            .Replace(FileExtension.TarXz.Value, string.Empty)
            .Replace(Constant.Colon, string.Empty)
            + FileExtension.Mp4.Value;
    }

    public bool HasGraphicsSubtitle()
    {
        return GraphicsSubtitleFile == null;
    }

    public virtual FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public virtual FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }

    public virtual void SetBrandingText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentNullException("Branding text cannot be null or empty");
        }

        BrandingText = text;
    }

    public void SetGraphicsSubtitleFile(string filePath)
    {
        GraphicsSubtitleFile = new AssSubtitleFile(filePath);
    }

    public string FileName()
    {
        return Path.GetFileName(FilePath);
    }

    public string ThumbnailFileName()
    {
        return Path.GetFileNameWithoutExtension(FileName()) + FileExtension.Jpg.Value;
    }

    public virtual string VideoFilters()
    {
        StringBuilder stringBuilder = new();

        stringBuilder.Append(
            new DrawTextFilter(BrandingText, DrawTextFilterTextColor(), Opacity.Full,
            DrawTextFilterBackgroundColor(), Opacity.Medium, DrawTextPosition.ChannelBrand).ToString()
            );

        if (GraphicsSubtitleFile == null)
        {
            return stringBuilder.ToString();
        }

        foreach (var subtitle in GraphicsSubtitleFile.Subtitles)
        {
            if (string.IsNullOrWhiteSpace(subtitle.Text))
            {
                continue;
            }

            stringBuilder.Append(Constant.CommaSpace);

            var splitTitle = subtitle.Text.Split(Constant.SemiColon);

            stringBuilder.Append(
                new DrawTextFilter(splitTitle.First(), DrawTextFilterTextColor(), Opacity.Full,
                DrawTextFilterBackgroundColor(), Opacity.Full, DrawTextPosition.SubtitlePrimary,
                subtitle.StartTime, subtitle.EndTime).ToString());

            if (splitTitle.Length == 2)
            {
                stringBuilder.Append(Constant.CommaSpace);
                stringBuilder.Append(
                    new DrawTextFilter(splitTitle.Last(), DrawTextFilterBackgroundColor(), Opacity.Full,
                    DrawTextFilterTextColor(), Opacity.Full, DrawTextPosition.SubtitleSecondary,
                    subtitle.StartTime, subtitle.EndTime).ToString());
            }
        }

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

            // font size 
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