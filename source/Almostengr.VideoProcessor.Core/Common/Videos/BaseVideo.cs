using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract record BaseVideo
{
    public string Title { get; init; }
    public string ArchiveFileName { get; init; }
    public string GraphicsSubtitleFileName { get; private set; }
    public string BaseDirectory { get; init; }
    public string VideoFilter { get; private set; }

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
            throw new VideoTitleIsNullOrWhiteSpaceException();
        }

        return fileName.Replace(FileExtension.TarGz, string.Empty)
            .Replace(FileExtension.TarXz, string.Empty)
            .Replace(FileExtension.Tar, string.Empty)
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

    public string OutputVideoFileName()
    {
        return ArchiveFileName
            .Replace(FileExtension.TarXz, string.Empty)
            .Replace(FileExtension.TarGz, string.Empty)
            .Replace(FileExtension.Tar, string.Empty)
            + FileExtension.Mp4;
    }

    public bool IsDraft()
    {
        return ArchiveFileName.ToLower().Contains(".draft");
    }

    public void SetGraphicsSubtitleFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        GraphicsSubtitleFileName = fileName;
    }

    public virtual string DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public virtual string DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }

    public virtual string SubtitleBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public virtual string SubtitleTextColor()
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

    public virtual void AddDrawTextVideoFilter(
        string text, string textColor, string textBrightness, string fontSize, string position,
        string backgroundColor, string backgroundBrightness, int borderWidth = 5, string duration = "")
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
        string filePath, int leftMargin = 10, int rightMargin = 10, int verticalMargin = 10)
    {
        StringBuilder textFilter = new();

        if (VideoFilter.Length > 0)
        {
            textFilter.Append(Constant.CommaSpace);
        }

        textFilter.Append($"subtitles='{filePath}':");
        // textFilter.Append($"force_style='OutlineColor=")
        // todo over styles

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
