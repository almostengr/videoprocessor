using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract record BaseVideoFile
{
    public string Title { get; init; }
    public string FilePath { get; init; }
    public string FileName { get; init; }
    // public string TarballFilePath { get; init; }
    // public string TarballFileName { get; init; }
    public bool IsDraft { get; init; }
    public string OutputVideoFileName { get; init; }
    public AssSubtitleFile? GraphicsSubtitleFile { get; private set; }
    public AudioFile AudioFile { get; private set; }
    public string BrandingText { get; private set; } = string.Empty;

    public readonly string ROBINSON_SERVICES = "Robinson Handy and Technology Services";
    public readonly string RHT_WEBSITE = "rhtservices.net";
    public readonly string RHT_SOCIAL_LINKS = "rhtservices.net/links";

    public BaseVideoFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is null or whitespace", nameof(filePath));
        }

        // TarballFilePath = filePath;
        // TarballFileName = Path.GetFileName(TarballFilePath);
        FilePath = filePath;
        FileName = Path.GetFileName(FilePath);
        Title = SetTitle(FileName);
        OutputVideoFileName = FileName
            .Replace(FileExtension.TarXz.ToString(), string.Empty)
            .Replace(FileExtension.TarGz.ToString(), string.Empty)
            .Replace(FileExtension.Tar.ToString(), string.Empty)
            + FileExtension.Mp4;

        // if (TarballFileName.ToLower().Contains("draft"))
        // {
        //     IsDraft = true;
        // }

        GraphicsSubtitleFile = null;
    }

    public abstract string[] BrandingTextOptions();

    public void SetGraphicsSubtitleFile(string filePath)
    {
        GraphicsSubtitleFile = new AssSubtitleFile(filePath);
    }

    public void SetAudioFile(AudioFile audioFile)
    {
        AudioFile = audioFile;
    }

    public void SetAudioFile(string filePath)
    {
        AudioFile = new AudioFile(filePath);
    }

    public void SetBrandingText(string brandingText)
    {
        if (string.IsNullOrWhiteSpace(brandingText))
        {
            throw new ArgumentException("Branding text cannot be null or whitespace", nameof(brandingText));
        }

        BrandingText = brandingText;
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
            stringBuilder.Append(Constant.CommaSpace);

            var splitTitle = subtitle.Text.Split(Constant.SemiColon);

            stringBuilder.Append(
                new DrawTextFilter(splitTitle.First(), DrawTextFilterTextColor(), Opacity.Full,
                DrawTextFilterBackgroundColor(), Opacity.Medium, DrawTextPosition.SubtitlePrimary,
                subtitle.StartTime, subtitle.EndTime).ToString());

            if (splitTitle.Length == 2)
            {
                stringBuilder.Append(
                    new DrawTextFilter(splitTitle.Last(), DrawTextFilterBackgroundColor(), Opacity.Full,
                    DrawTextFilterTextColor(), Opacity.Full, DrawTextPosition.SubtitleSecondary,
                    subtitle.StartTime, subtitle.EndTime).ToString());
            }
        }

        return stringBuilder.ToString();
    }

    protected virtual string SetTitle(string fileName)
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

    public virtual FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public virtual FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }

    protected sealed class DrawTextFilter // public sealed class DrawTextFilter
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
