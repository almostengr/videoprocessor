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
    public string VideoFilter { get; private set; }
    public bool IsDraft { get; private set; } = false;
    public string OutputVideoFileName { get; private set; }

    public readonly string ROBINSON_SERVICES = "Robinson Handy and Technology Services";
    public readonly string RHT_WEBSITE = "rhtservices.net";
    public readonly string RHT_SOCIAL_LINKS = "rhtservices.net/links";

    public BaseVideoFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Arguments are null or whitespace");
        }

        TarballFilePath = filePath;

        TarballFileName = Path.GetFileName(TarballFilePath);
        GraphicsSubtitleFileName = string.Empty;
        Title = SetTitle(TarballFileName);
        VideoFilter = string.Empty;

        OutputVideoFileName = TarballFileName
            .Replace(FileExtension.TarXz.ToString(), string.Empty.ToString())
            .Replace(FileExtension.TarGz.ToString(), string.Empty.ToString())
            .Replace(FileExtension.Tar.ToString(), string.Empty.ToString())
            + FileExtension.Mp4;

        if (Title.ToLower().Contains("draft"))
        {
            IsDraft = true;
        }
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
        textFilter.Append($"fontcolor={textColor.ToString()}@{textBrightness.ToString()}:");
        textFilter.Append($"fontsize={fontSize.ToString()}:");
        textFilter.Append($"{position.ToString()}:");
        textFilter.Append(Constant.BorderBox);
        textFilter.Append($"boxborderw={borderWidth.ToString()}:");
        textFilter.Append($"boxcolor={backgroundColor.ToString()}@{backgroundBrightness.ToString()}");

        if (duration.Length > 0)
        {
            textFilter.Append(":");
            textFilter.Append(duration);
        }

        VideoFilter += textFilter.ToString();
    }

    public virtual void AddSubtitleVideoFilter(
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
