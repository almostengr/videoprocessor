using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract record BaseVideo
{
    public string Title { get; init; }
    public string ArchiveFileName { get; init; }
    public string GraphicsSubtitleFileName { get; private set; }
    public string BaseDirectory { get; init; }
    public string VideoFilter { get; private set; }
    public bool IsDraft { get; private set; } = false;

    public readonly string ROBINSON_SERVICES = "Robinson Handy and Technology Services";
    public readonly string RHT_WEBSITE = "rhtservices.net";
    public readonly string RHT_SOCIAL_LINKS = "rhtservices.net/links";

    public BaseVideo(string baseDirectory, string archiveFileName)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory) || string.IsNullOrWhiteSpace(archiveFileName))
        {
            throw new ArgumentException("Arguments are null or whitespace");
        }

        ArchiveFileName = archiveFileName;
        BaseDirectory = baseDirectory;
        GraphicsSubtitleFileName = string.Empty;
        Title = SetTitle(archiveFileName);
        VideoFilter = string.Empty;

        if (Title.ToLower().Contains("draft"))
        {
            IsDraft = true;
        }
    }

    public virtual string EndScreenFilePath()
    {
        return Path.Combine(BaseDirectory, EndScreenFileName());
    }

    public string EndScreenFileName()
    {
        // return "endscreen.mp4";
        return "endscreen.ts";
    }

    public abstract string[] BrandingTextOptions();

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

    public string ErrorTarballFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Archive, OutputVideoFileName() + FileExtension.Err);
    }

    public string OutputVideoFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Upload, OutputVideoFileName());
    }

    public string UploadVideoFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Upload, OutputVideoFileName());
    }

    public string OutputVideoFileName()
    {
        return ArchiveFileName
            .Replace(FileExtension.TarXz.ToString(), string.Empty.ToString())
            .Replace(FileExtension.TarGz.ToString(), string.Empty.ToString())
            .Replace(FileExtension.Tar.ToString(), string.Empty.ToString())
            + FileExtension.Mp4;
    }

    // public bool IsDraft()
    // {
    //     return ArchiveFileName.ToLower().Contains(".draft");
    // }

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

    public virtual FfMpegColor SubtitleBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public virtual FfMpegColor SubtitleTextColor()
    {
        return FfMpegColor.White;
    }

    public string FfmpegInputFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Working, "ffmpeginput.txt");
    }

    public string IncomingTarballFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Incoming, ArchiveFileName);
    }

    public string ArchiveTarballFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Archive, ArchiveFileName);
    }

    public string DraftTarballFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Draft, ArchiveFileName);
    }

    public virtual void AddDrawTextVideoFilter(
        string text, FfMpegColor textColor, Opacity textBrightness, FfmpegFontSize fontSize, DrawTextPosition position,
        FfMpegColor backgroundColor, Opacity backgroundBrightness, int borderWidth = 5, string duration = "")
    {
        StringBuilder textFilter = new();

        if (VideoFilter.Length > 0)
        {
            textFilter.Append(Constant.CommaSpace);
        }

        textFilter.Append($"drawtext=textfile:'{text.Trim()}':");
        textFilter.Append($"fontcolor={textColor}@{textBrightness}:");
        textFilter.Append($"fontsize={fontSize}:");
        textFilter.Append($"{position}:");
        textFilter.Append(Constant.BorderBox);
        textFilter.Append($"boxborderw={borderWidth.ToString()}:");
        textFilter.Append($"boxcolor={backgroundColor}@{backgroundBrightness}");

        if (duration.Length > 0)
        {
            textFilter.Append(":");
            textFilter.Append(duration);
        }

        VideoFilter += textFilter.ToString();
    }

    public virtual void AddSubtitleVideoFilter(
        // string filePath, int leftMargin = 10, int rightMargin = 10, int verticalMargin = 10)
        string filePath, string outlineColor, string textColor, int fontSize = 30)
    {
        StringBuilder textFilter = new();

        if (VideoFilter.Length > 0)
        {
            textFilter.Append(Constant.CommaSpace);
        }

        textFilter.Append($"subtitles='{filePath}':");
        textFilter.Append($"force_style='OutlineColour={outlineColor},TextColour={textColor},BorderStyle=3,Outline=5,Shadow=1,Alignment=1,Fontsize={fontSize}'");

        VideoFilter += textFilter.ToString();
    }

    private string FilterDuration(int duration = 239)
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
