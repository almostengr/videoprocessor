using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class VideoFile
{

    public string FilePath { get; init; }
    private AudioFile? AudioFile { get; set; }
    private AssSubtitleFile? GraphicsSubtitleFile { get; set; }
    // public VideoProjectArchiveFile? ProjectArchive { get; init; }

    public VideoFile(VideoProjectArchiveFile videoProjectArchiveFile)
    {
        // ProjectArchive = videoProjectArchiveFile;
        FilePath = videoProjectArchiveFile.FilePath;
        // FileName = Path.GetFileName(ProjectArchive.FilePath);
    }

    public VideoFile(string filePath)
    {
        ValidateVideoFile(filePath);
        FilePath = filePath;
        AudioFile = null;
    }

    // public VideoFile(string filePath, AudioFile audio)
    // {
    //     ValidateVideoFile(filePath);
    //     FilePath = filePath;
    //     AudioFile = audio;
    // }

    public string FileName()
    {
        return Path.GetFileName(FilePath);
    }

    private static void ValidateVideoFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new ArgumentException("File does not exist");
        }

        if (
            !filePath.ToLower().EndsWith(FileExtension.Avi.Value) &&
            !filePath.ToLower().EndsWith(FileExtension.Mkv.Value) &&
            !filePath.ToLower().EndsWith(FileExtension.Mov.Value) &&
            !filePath.ToLower().EndsWith(FileExtension.Mp4.Value)
        )
        {
            throw new ArgumentException("File type is not correct", nameof(filePath));
        }
    }

    public string? AudioFilePath()
    {
        if (AudioFile == null)
        {
            return null;
        }

        return AudioFile.FilePath;
    }

    public string? GraphicsFilePath()
    {
        if (GraphicsSubtitleFile == null)
        {
            return null;
        }

        return GraphicsSubtitleFile.FilePath;
    }

    public abstract string BrandingText();

    internal void SetAudioFile(AudioFile audioFile)
    {
        AudioFile = audioFile;
    }

    internal string TsOutputFileName()
    {
        return Path.GetFileNameWithoutExtension(FileName()) + FileExtension.Ts.Value;
    }

    internal void SetGraphicsSubtitleFile(string subtitleFilePath)
    {
        GraphicsSubtitleFile = new AssSubtitleFile(subtitleFilePath);
    }

    internal void SetGraphicsSubtitleFile(AssSubtitleFile subtitleFile)
    {
        GraphicsSubtitleFile = subtitleFile;
    }

    // public abstract string[] BrandingTextOptions();

    public virtual string Title()
    {
        return FileName()
            .Replace(FileExtension.Mp4.Value, string.Empty)
            .Replace(FileExtension.TarXz.Value, string.Empty)
            .Replace(FileExtension.TarGz.Value, string.Empty)
            .Replace(FileExtension.Tar.Value, string.Empty)
            .Replace(Constant.Colon, string.Empty);
    }

    public string OutputFileName()
    {
        // return Path.GetFileName(FilePath)
        return FileName()
            .Replace(FileExtension.TarXz.Value, string.Empty)
            .Replace(FileExtension.TarGz.Value, string.Empty)
            .Replace(FileExtension.Tar.Value, string.Empty)
            .Replace(Constant.Colon, string.Empty)
            + FileExtension.Mp4.Value;
    }

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

    // public virtual void SetBrandingText(string text)
    // {
    //     if (string.IsNullOrEmpty(text))
    //     {
    //         throw new ArgumentNullException("Branding text cannot be null or empty");
    //     }

    //     BrandingText = text;
    // }

    // public string ThumbnailFileName()
    // {
    //     return Path.GetFileNameWithoutExtension(FileName) + FileExtension.Jpg.Value;
    // }

    public virtual string VideoFilters()
    {
        StringBuilder stringBuilder = new();

        stringBuilder.Append(
            new DrawTextFilter(BrandingText(), DrawTextFilterTextColor(), Opacity.Full,
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

    internal void SetSubtitles(List<SubtitleFileEntry> subtitleFileEntries)
    {
        GraphicsSubtitleFile.SetSubtitles(subtitleFileEntries);
    }

    internal string GraphicsFileName()
    {
        return GraphicsSubtitleFile.FileName;
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